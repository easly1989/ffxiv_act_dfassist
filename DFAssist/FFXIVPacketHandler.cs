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
                                        Logger.Error("l-analyze-error-length", read, i, messageCount);
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
                                    Logger.Exception(ex, "l-analyze-error-general");
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
                Logger.Exception(ex, "l-analyze-error");
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
                        Logger.Info("l-field-instance-entered", Data.GetInstance(code).Name);
                        FireEvent(pid, EventType.INSTANCE_ENTER, new int[] { code });
                    }
                    else if (type == 0x0C)
                    {
                        Logger.Info("l-field-instance-left");
                        FireEvent(pid, EventType.INSTANCE_EXIT, new int[] { code });
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
                            Logger.Info("l-queue-started-roulette", Data.GetRoulette(rouletteCode));
                            FireEvent(pid, EventType.MATCH_BEGIN, new[] { (int)MatchType.ROULETTE, rouletteCode });
                        }
                        else // Specific Duty (Dungeon/Trial/Raid)
                        {
                            var instances = new List<int>();
                            for (var i = 0; i < 5; i++)
                            {
                                var code = BitConverter.ToUInt16(data, 22 + i * 2);
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

                            Logger.Info("l-queue-started-general", string.Join(", ", instances.Select(x => Data.GetInstance(x).Name).ToArray()));
                            FireEvent(pid, EventType.MATCH_BEGIN, args.ToArray());
                        }
                    }
                    else if (status == 3) // Cancel
                    {
                        state = reason == 8 ? State.QUEUED : State.IDLE;
                        Logger.Info("l-queue-stopped");
                        FireEvent(pid, EventType.MATCH_END, new[] { (int)MatchEndType.CANCELLED });
                    }
                    else if (status == 6) // Entered
                    {
                        state = State.IDLE;
                        Logger.Info("l-queue-entered");
                        FireEvent(pid, EventType.MATCH_END, new[] { (int)MatchEndType.ENTER_INSTANCE });
                    }
                    else if (status == 4) // Matched
                    {
                        var roulette = data[20];
                        var code = BitConverter.ToUInt16(data, 22); 

                        state = State.MATCHED;
                        
                        Logger.Info("l-queue-matched", code);
                        FireEvent(pid, EventType.MATCH_ALERT, new int[] { roulette, code });
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
                    var status = data[4];
                    var tank = data[5];
                    var dps = data[6];
                    var healer = data[7];
                    var instance = Data.GetInstance(code, true); // Matching still uses the old ID 

                    if (status == 1)
                    {
                        var member = tank * 10000 + dps * 100 + healer;

                        if (state == State.MATCHED && _lastMember != member)
                        {
                            // someone else canceled the duty
                            state = State.QUEUED;
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

                    Logger.Info("l-queue-updated", instance.Name, status, tank, instance.Tank, healer, instance.Healer, dps, instance.Dps);
                    FireEvent(pid, EventType.MATCH_PROGRESS, new int[] { code, status, tank, healer, dps });
                }
                else if (opcode == 0x0080)
                {
                    var roulette = data[2];
                    var code = BitConverter.ToUInt16(data, 4);

                    state = State.MATCHED;

                    Logger.Success("l-queue-matched ", code);
                    FireEvent(pid, EventType.MATCH_ALERT, new int[] { roulette, code });
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "l-analyze-error-general");
            }
        }

        private static void FireEvent(int pid, EventType eventType, int[] args)
        {
            OnEventReceived?.Invoke(pid, eventType, args);
        }
    }
}
