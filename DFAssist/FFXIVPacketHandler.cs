using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace DFAssist
{
    // ReSharper disable InconsistentNaming
    public static class FFXIVPacketHandler
    // ReSharper restore InconsistentNaming
    {
        private static int _lastMember;

        public delegate void EventHandler(int pid, EventType eventType, int[] args);
        public static event EventHandler OnEventReceived;

        public static void Analyze(int pid, byte[] payload, ref State state)
        {
            try
            {
                while (true)
                {
                    if (payload.Length < 4)
                    {
                        break;
                    }

                    var type = BitConverter.ToUInt16(payload, 0);

                    if (type == 0x0000 || type == 0x5252)
                    {
                        if (payload.Length < 28)
                        {
                            break;
                        }

                        var length = BitConverter.ToInt32(payload, 24);

                        if (length <= 0 || payload.Length < length)
                        {
                            break;
                        }

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
                                    stream.Seek(2, SeekOrigin.Current); // .Net DeflateStream Bug (Force the previous 2 bytes)

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
                                        //Logger.LogError("l-analyze-error-length", read, i, messageCount);
                                        break;
                                    }
                                    var messageLength = BitConverter.ToInt32(buffer, 0);

                                    var message = new byte[messageLength];
                                    messages.Seek(-4, SeekOrigin.Current);
                                    messages.Read(message, 0, messageLength);

                                    HandleMessage(pid, message, ref state);
                                }
                                catch (Exception ex)
                                {
                                    Logger.LogException(ex, "l-analyze-error-general");
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
                        // Forward - cut packet workaround
                        // Discard one truncated packet and find just the next packet ...
                        // TODO: Correcting without discarding packets properly
                        for (var offset = 0; offset < payload.Length - 2; offset++)
                        {
                            var possibleType = BitConverter.ToUInt16(payload, offset);
                            if (possibleType != 0x5252)
                                continue;

                            payload = payload.Skip(offset).ToArray();
                            Analyze(pid, payload, ref state);
                            break;
                        }
                    }
                    break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "l-analyze-error");
            }
        }

        private static void HandleMessage(int pid, byte[] message, ref State state)
        {
            try
            {
                if (message.Length < 32)
                {
                    // type == 0x0000 (Messages were filtered here)
                    return;
                }

                var opcode = BitConverter.ToUInt16(message, 18);
                var data = message.Skip(32).ToArray();

                if (opcode == 0x022F) // Entering/Leaving an instance
                {
                    var code = BitConverter.ToInt16(data, 4);
                    var type = data[8];

                    if (type == 0x0B)
                    {
                        Logger.LogInfo("Entering instance = " + code);
                        FireEvent(pid, EventType.INSTANCE_ENTER, new int[] { code });
                    }
                    else if (type == 0x0C)
                    {
                        Logger.LogInfo("Leaving instance = " + code);
                        FireEvent(pid, EventType.INSTANCE_EXIT, new int[] { code });
                    }
                }
                else if (opcode == 0x0143) // Fate in progress
                {
                    var type = data[0];

                    if (type == 0x9B)
                    {
                        var code = BitConverter.ToUInt16(data, 4);
                        var progress = data[8];
                        FireEvent(pid, EventType.FATE_PROGRESS, new int[] { code, progress });
                    }
                    else if (type == 0x79) // Fate is over
                    {
                        var code = BitConverter.ToUInt16(data, 4);
                        var status = BitConverter.ToUInt16(data, 28);
                        Logger.LogInfo("Fate Abrupted = " + code + ", status = " + status);
                        FireEvent(pid, EventType.FATE_END, new int[] { code, status });
                    }
                    else if (type == 0x74) // Fate just started
                    {
                        var code = BitConverter.ToUInt16(data, 4);
                        Logger.LogInfo("Unforeseen occurrence = " + code);
                        FireEvent(pid, EventType.FATE_BEGIN, new int[] { code });
                    }
                }
                else if (opcode == 0x0078) // Duties
                {
                    var status = data[0];
                    var reason = data[4];

                    if (status == 0) // Apply for Duties
                    {
                        state = State.QUEUED;

                        var rouletteCode = data[20];

                        if (rouletteCode != 0 && (data[15] == 0 || data[15] == 64)) // Roulette, on Korean Server || on Global Server
                        {
                            Logger.LogInfo("Applied for Duty Roulette = " + rouletteCode);
                            FireEvent(pid, EventType.MATCH_BEGIN, new[] { (int)MatchType.ROULETTE, rouletteCode });
                        }
                        else // Specific Duty (Dungeon/Trial/Raid)
                        {
                            var instances = new List<int>();
                            for (var i = 0; i < 5; i++)
                            {
                                var code = BitConverter.ToUInt16(data, 22 + (i * 2));
                                if (code == 0)
                                    break;

                                instances.Add(code);
                            }

                            if (!instances.Any())
                                return;

                            var args = new List<int> { (int)MatchType.SELECTIVE, instances.Count };
                            foreach (var item in instances)
                            {
                                args.Add(item);
                            }

                            Logger.LogInfo("Applied for Specific Duty = ", string.Join(", ", instances) + ", count = " + instances.Count);
                            FireEvent(pid, EventType.MATCH_BEGIN, args.ToArray());
                        }
                    }
                    else if (status == 3) // Cancel
                    {
                        state = reason == 8 ? State.QUEUED : State.IDLE;

                        Logger.LogInfo("Duty matching canceled, reason = " + reason);
                        FireEvent(pid, EventType.MATCH_END, new[] { (int)MatchEndType.CANCELLED });
                    }
                    else if (status == 6) // Entered
                    {
                        state = State.IDLE;

                        Logger.LogInfo("Duty started");
                        FireEvent(pid, EventType.MATCH_END, new[] { (int)MatchEndType.ENTER_INSTANCE });
                    }
                    else if (status == 4) // Matched
                    {
                        var roulette = data[20];
                        var code = BitConverter.ToUInt16(data, 22);

                        state = State.MATCHED;
                        
                        Logger.LogInfo("Matched, Type = " + roulette + ", Duty = " + code);
                        FireEvent(pid, EventType.MATCH_ALERT, new int[] { roulette, code });
                    }
                }
                else if (opcode == 0x006F)
                {

                }
                else if (opcode == 0x0121) // Global Server
                {
                    var status = data[5];

                    if (status == 128)

                        Logger.LogInfo("Matching, Click ok in the application confirmation window (Global)");
                }
                else if (opcode == 0x0079) // Status during matching
                {
                    var code = BitConverter.ToUInt16(data, 0);
                    var status = data[4];
                    var tank = data[5];
                    var dps = data[6];
                    var healer = data[7];

                    if (status == 1)
                    {
                        var member = tank * 10000 + dps * 100 + healer;

                        if (state == State.MATCHED && _lastMember != member)
                        {
                            state = State.QUEUED;
                            Logger.LogInfo("Matching progress, someone canceled");
                        }
                        else if (state == State.IDLE)
                        {
                            state = State.QUEUED;
                        }

                        _lastMember = member;
                    }
                    else if (status == 2)
                    {
                        return;
                    }

                    Logger.LogInfo("Matching progress, Duty =" + code + ", " + status + ", Tank: " + tank + ", Healer: " + healer + ", DPS: " + dps);
                    FireEvent(pid, EventType.MATCH_PROGRESS, new int[] { code, status, tank, healer, dps });
                }
                else if (opcode == 0x0080)
                {
                    var roulette = data[2];
                    var code = BitConverter.ToUInt16(data, 4);

                    state = State.MATCHED;

                    Logger.LogSuccess("l-queue-matched " + code);
                    FireEvent(pid, EventType.MATCH_ALERT, new int[] { roulette, code });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "[" + pid + "]l-analyze-error-general");
            }
        }

        private static void FireEvent(int pid, EventType eventType, int[] args)
        {
            OnEventReceived?.Invoke(pid, eventType, args);
        }
    }
}
