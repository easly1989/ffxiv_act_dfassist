using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using DFAssist.Contracts;
using DFAssist.Contracts.Duty;
using DFAssist.Contracts.Repositories;
using Splat;

namespace DFAssist.Core.Network
{
    // ReSharper disable InconsistentNaming
    public class FFXIVPacketHandler : IPacketHandler
    // ReSharper restore InconsistentNaming
    {
        public event PacketDelegate OnEventReceived;

        private readonly ILogger _logger;
        private readonly IDataRepository _dataRepository;

        private int _lastMember;
        private byte _rouletteCode;
        private bool _netCompatibility;

        public FFXIVPacketHandler()
        {
            _logger = Locator.Current.GetService<ILogger>();
            _dataRepository = Locator.Current.GetService<IDataRepository>();
        }

        public void Analyze(int processId, byte[] payload, ref MatchingState state)
        {
            try
            {
                while (true)
                {
                    if (payload.Length < 4)
                        break;

                    var type = BitConverter.ToUInt16(payload, 0);
                    if (type == 0x0000 || type == 0x5252)
                    {
                        if (payload.Length < 28)
                            break;

                        var length = BitConverter.ToInt32(payload, 24);
                        if (length <= 0 || payload.Length < length)
                            break;

                        using (var messages = new MemoryStream(payload.Length))
                        {
                            using (var stream = new MemoryStream(payload, 0, length))
                            {
                                stream.Seek(40, SeekOrigin.Begin);

                                if (payload[33] == 0x00)
                                {
                                    stream.CopyTo(messages);
                                }
                                else
                                {
                                    // .Net DeflateStream Bug (Force the previous 2 bytes)
                                    stream.Seek(2, SeekOrigin.Current);
                                    using (var z = new DeflateStream(stream, CompressionMode.Decompress))
                                    {
                                        z.CopyTo(messages);
                                    }
                                }
                            }

                            messages.Seek(0, SeekOrigin.Begin);

                            var messageCount = BitConverter.ToUInt16(payload, 30);
                            for (var i = 0; i < messageCount; i++)
                            {
                                try
                                {
                                    var buffer = new byte[4];
                                    var read = messages.Read(buffer, 0, 4);
                                    if (read < 4)
                                    {
                                        _logger.Write(
                                            $"A: Length Error while analyzing Message: {read} {i}/{messageCount}",
                                            LogLevel.Error);
                                        break;
                                    }

                                    var messageLength = BitConverter.ToInt32(buffer, 0);

                                    var message = new byte[messageLength];
                                    messages.Seek(-4, SeekOrigin.Current);
                                    messages.Read(message, 0, messageLength);

                                    HandleMessage(processId, message, ref state);
                                }
                                catch (Exception ex)
                                {
                                    _logger.Write(ex, "A: Error while analyzing Message", LogLevel.Error);
                                }
                            }
                        }

                        if (length < payload.Length)
                        {
                            // Packets still need to be processed
                            payload = payload.Skip(length).ToArray();
                            continue;
                        }
                    }
                    else
                    {
                        // Forward-Cut packet workaround
                        // Discard one truncated packet and find just the next packet
                        for (var offset = 0; offset < payload.Length - 2; offset++)
                        {
                            var possibleType = BitConverter.ToUInt16(payload, offset);
                            if (possibleType != 0x5252)
                                continue;

                            payload = payload.Skip(offset).ToArray();
                            Analyze(processId, payload, ref state);
                            break;
                        }
                    }

                    break;
                }
            }
            catch (Exception ex)
            {
                _logger.Write(ex, "A: Error while handling Message", LogLevel.Error);
            }
        }

        private void HandleMessage(int pid, byte[] message, ref MatchingState state)
        {
            try
            {
                if (message.Length < 32)
                {
                    // type == 0x0000 (Messages were filtered here)
                    return;
                }

                var opcode = BitConverter.ToUInt16(message, 18);

#if !DEBUG
                if (opcode != 0x0078 &&
                    opcode != 0x0079 &&
                    opcode != 0x0080 &&
                    opcode != 0x006C &&
                    opcode != 0x006F &&
                    opcode != 0x0121 &&
                    opcode != 0x0143 &&
                    opcode != 0x022F)
                    return;
#endif
                var data = message.Skip(32).ToArray();
                if (opcode == 0x022F) // Entering/Leaving an instance
                {
                    var code = BitConverter.ToInt16(data, 4);
                    var type = data[8];

                    if (type == 0x0B)
                    {
                        _logger.Write($"I: Entered Instance Area [{code}] - {_dataRepository.GetInstance(code).Name}", LogLevel.Debug);
                    }
                    else if (type == 0x0C)
                    {
                        _logger.Write($"I: Left Instance Area [{code}] - {_dataRepository.GetInstance(code).Name}", LogLevel.Debug);
                    }
                }
                else if (opcode == 0x0078) // Duties
                {
                    var status = data[0];
                    var reason = data[4];

                    if (status == 0) // Apply for Duties
                    {
                        _netCompatibility = false;
                        state = MatchingState.QUEUED;

                        _rouletteCode = data[20];

                        if (_rouletteCode != 0 && (data[15] == 0 || data[15] == 64)
                        ) // Roulette, on Korean Server || on Global Server
                        {
                            _logger.Write($"Q: Duty Roulette Matching Started [{_rouletteCode}] - {_dataRepository.GetRoulette(_rouletteCode).Name}", LogLevel.Debug);
                        }
                        else // Specific Duty (Dungeon/Trial/Raid)
                        {
                            _logger.Write("Q: Matching started for duties: ", LogLevel.Debug);
                            for (var i = 0; i < 5; i++)
                            {
                                var code = BitConverter.ToUInt16(data, 22 + i * 2);
                                if (code == 0)
                                    break;

                                _logger.Write($" {i}. [{code}] - {_dataRepository.GetInstance(code).Name}", LogLevel.Debug);
                            }
                        }
                    }
                    else if (status == 3) // Cancel
                    {
                        state = reason == 8 ? MatchingState.QUEUED : MatchingState.IDLE;
                        _logger.Write("Q: Matching Stopped", LogLevel.Debug);
                    }
                    else if (status == 6) // Entered
                    {
                        state = MatchingState.IDLE;
                        _logger.Write("Q: Entered Instance Area", LogLevel.Debug);
                    }
                    else if (status == 4) // Matched
                    {
                        var roulette = data[20];
                        var code = BitConverter.ToUInt16(data, 22);

                        state = MatchingState.MATCHED;
                        FireEvent(pid, EventType.MATCH_ALERT, new int[] { roulette, code });

                        _logger.Write($"Q: Matched [{roulette} - - {_dataRepository.GetInstance(code).Name}] - [{code} - {_dataRepository.GetInstance(code).Name}]", LogLevel.Info);
                    }
                }
                else if (opcode == 0x006F)
                {
                    // used on standalone version to stop blink
                }
                else if (opcode == 0x0121)
                {
                    // used on standalone version to stop blink, for Global Server
                }
                else if (opcode == 0x0079) // Status during matching
                {
                    var code = BitConverter.ToUInt16(data, 0);
                    byte status;
                    byte tank;
                    byte dps;
                    byte healer;
                    var member = 0;
                    var instance = _dataRepository.GetInstance(code);

                    if (_netCompatibility)
                    {
                        status = data[8];
                        tank = data[9];
                        dps = data[10];
                        healer = data[11];
                    }
                    else
                    {
                        status = data[4];
                        tank = data[5];
                        dps = data[6];
                        healer = data[7];
                    }

                    if (status == 0 && tank == 0 && healer == 0 && dps == 0) // v4.5~ compatibility (data location changed, original location sends "0")
                    {
                        _netCompatibility = true;
                        status = data[8];
                        tank = data[9];
                        dps = data[10];
                        healer = data[11];
                    }

                    if (status == 1)
                    {
                        member = tank * 10000 + dps * 100 + healer;

                        if (state == MatchingState.MATCHED && _lastMember != member)
                        {
                            // We get here when the queue is stopped by someone else (?)
                            state = MatchingState.QUEUED;
                        }
                        else if (state == MatchingState.IDLE)
                        {
                            // Plugin started with duty finder in progress
                            state = MatchingState.QUEUED;
                        }
                        else if (state == MatchingState.QUEUED)
                        {
                            // in queue
                        }

                        _lastMember = member;
                    }
                    else if (status == 2)
                    {
                        // info about player partecipating in the duty (?)
                        return;
                    }
                    else if (status == 4)
                    {
                        // Duty Accepted status
                    }

                    var memberinfo = $"{tank}/{instance.Tank}, {healer}/{instance.Healer}, {dps}/{instance.Dps} | {member}";
                    _logger.Write($"Q: Matching State Updated [{instance.Name}, {memberinfo}]", LogLevel.Debug);
                }
                else if (opcode == 0x0080)
                {
                    var roulette = data[2];
                    var code = BitConverter.ToUInt16(data, 4);

                    state = MatchingState.MATCHED;
                    FireEvent(pid, EventType.MATCH_ALERT, new int[] { roulette, code });

                    _logger.Write($"Q: Matched [{code}] - {_dataRepository.GetInstance(code).Name}", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                _logger.Write(ex, "A: Error while analyzing Message", LogLevel.Error);
            }
        }

        private void FireEvent(int pid, EventType eventType, int[] args)
        {
            OnEventReceived?.Invoke(pid, eventType, args);
        }
    }
}