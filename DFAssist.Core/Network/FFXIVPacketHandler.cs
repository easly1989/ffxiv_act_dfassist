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
                                    // .Net DeflateStream (Force the previous 2 bytes)
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
                if (opcode != 0x00AE &&
                    opcode != 0x0304 &&
                    opcode != 0x0121 &&
                    opcode != 0x015E &&
                    opcode != 0x006F &&
                    opcode != 0x00B3 &&
                    opcode != 0x008F &&
                    opcode != 0x022F)
                    return;
#endif
#if DEBUG
                _logger.Write($"--- Received opcode: {opcode}", LogLevel.Warn);
#endif
                var data = message.Skip(32).ToArray();
                if (opcode == 0x022F) // Entering/Leaving an instance
                {
                    var code = BitConverter.ToInt16(data, 4);
                    if (code == 0)
                        return;

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
                else if (opcode == 0x008F) // 5.1 Duties
                {
                    // ReSharper disable UnusedVariable
                    var status = data[0];
                    var reason = data[4];
                    // ReSharper restore UnusedVariable

                    state = MatchingState.QUEUED;

                    _rouletteCode = data[20];
                    _rouletteCode = data[8];

                    if (_rouletteCode != 0 && (data[15] == 0 || data[15] == 64)) // Roulette, on Korean Server || on Global Server
                    {
                        _logger.Write($"Q: Duty Roulette Matching Started [{_rouletteCode}] - {_dataRepository.GetRoulette(_rouletteCode).Name}", LogLevel.Debug);
                    }
                    else // Specific Duty (Dungeon/Trial/Raid)
                    {
                        _rouletteCode = 0;
                        _logger.Write("Q: Matching started for duties: ", LogLevel.Debug);
                        for (var i = 0; i < 5; i++)
                        {
                            var code = BitConverter.ToUInt16(data, 12 + (i * 4));
                            if (code == 0)
                                break;

                            _logger.Write($" {i}. [{code}] - {_dataRepository.GetInstance(code).Name}", LogLevel.Debug);
                        }
                    }
                }
                else if (opcode == 0x00B3) // 5.1 Duty Matched
                {
                    var matchedRoulette = BitConverter.ToUInt16(data, 2);
                    var matchedCode = BitConverter.ToUInt16(data, 20);

                    state = MatchingState.MATCHED;
                    FireEvent(pid, EventType.MATCH_ALERT, new int[] { matchedRoulette, matchedCode });

                    var instanceString = $"{matchedCode} - {_dataRepository.GetInstance(matchedCode).Name}";
                    _logger.Write(matchedRoulette != 0
                            ? $"Q: Matched [{matchedRoulette} - {_dataRepository.GetRoulette(matchedRoulette).Name}] - [{instanceString}]"
                            : $"Q: Matched [{instanceString}]", LogLevel.Info);
                }
                else if (opcode == 0x006F)
                {
                    // used on standalone version to stop blink
                }
                else if (opcode == 0x015E) // v5.1 Cancel Duty
                {
                    if (data[3] != 0) return;

                    state = MatchingState.IDLE;

                    _logger.Write("Duty Canceled!", LogLevel.Debug);
                }
                else if (opcode == 0x0121)
                {
                    // used on standalone version to stop blink, for Global Server
                }
                else if (opcode == 0x0304) // Status during matching
                {
                    // ReSharper disable UnusedVariable
                    var order = data[6];
                    var waitTime = data[7];
                    var tank = data[8];
                    var tankMax = data[9];
                    var healer = data[10];
                    var healerMax = data[11];
                    var dps = data[12];
                    var dpsMax = data[13];
                    // ReSharper restore UnusedVariable

                    var member = tank * 10000 + dps * 100 + healer;

                    switch (state)
                    {
                        case MatchingState.MATCHED when _lastMember != member:
                        // Plugin started with duty finder in progress
                        case MatchingState.IDLE:
                            // We get here when the queue is stopped by someone else (?)
                            state = MatchingState.QUEUED;
                            break;
                        case MatchingState.QUEUED:
                            // in queue
                            break;
                    }

                    _lastMember = member;

                    var memberinfo = $"Tanks: {tank}, Healers: {healer}, Dps: {dps} | Total Members: {member}";
                    _logger.Write($"Q: Matching State Updated [{memberinfo}]", LogLevel.Debug);
                }
                else if (opcode == 0x00AE) // Participant check status packet (received after matching)
                {
                    var code = BitConverter.ToUInt16(data, 0);
                    if (code == 0)
                        return;

                    var instance = _dataRepository.GetInstance(code);
                    var tank = data[12];
                    var healer = data[14];
                    var dps = data[16];

                    var memberinfo = $"Tanks: {tank}/{instance.Tank}, Healers: {healer}/{instance.Healer}, Dps: {dps}/{instance.Dps}";
                    _logger.Write($"Q: Matching State Updated [{instance.Name} - {memberinfo}]", LogLevel.Debug);
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