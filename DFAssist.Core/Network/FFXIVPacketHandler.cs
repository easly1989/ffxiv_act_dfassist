using System;
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
        private readonly ILogger _logger;
        private readonly IDataRepository _dataRepository;

        public FFXIVPacketHandler()
        {
            _logger = Locator.Current.GetService<ILogger>();
            _dataRepository = Locator.Current.GetService<IDataRepository>();
        }

        public void HandleMessage(byte[] message, Action<EventType, int[]> fireEvent)
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
                if(opcode != 0x0288 && 
                   opcode != 0x01F8 &&
                   opcode != 0x022F) 
                    return;
#endif
#if DEBUG
                _logger.Write($"--- Received opcode: {opcode}", LogLevel.Warn);
#endif
                var data = message.Skip(32).ToArray();
                if (opcode == 0x0228) // 5.18 Duties
                {
                    var rouletteCode = data[8];

                    if (rouletteCode != 0 && (data[15] == 0 || data[15] == 64)) // Roulette, on Korean Server || on Global Server
                    {
                        _logger.Write($"Q: Duty Roulette Matching Started [{rouletteCode}] - {_dataRepository.GetRoulette(rouletteCode).Name}", LogLevel.Debug);
                    }
                    else // Specific Duty (Dungeon/Trial/Raid)
                    {
                        _logger.Write("Q: Matching started for duties: ", LogLevel.Debug);
                        for (var i = 0; i < 5; i++)
                        {
                            var code = BitConverter.ToUInt16(data, 12 + (i * 4));
                            var instanceName = code == 0 ? "Unknown Instance" : _dataRepository.GetInstance(code).Name;
                            _logger.Write($" {i}. [{code}] - {instanceName}", LogLevel.Debug);
                        }
                    }
                }
                else if (opcode == 0x145) 
                {
                    var matchedRoulette = BitConverter.ToUInt16(data, 2);
                    var matchedCode = BitConverter.ToUInt16(data, 20);

                    fireEvent?.Invoke(EventType.MATCH_ALERT, new int[] { matchedRoulette, matchedCode });

                    var instanceString = $"{matchedCode} - {_dataRepository.GetInstance(matchedCode).Name}";
                    _logger.Write(matchedRoulette != 0
                        ? $"Q: Matched [{matchedRoulette} - {_dataRepository.GetRoulette(matchedRoulette).Name}] - [{instanceString}]"
                        : $"Q: Matched [{instanceString}]", LogLevel.Info);
                }
                else if (opcode == 0x03CF)
                {
                    switch (data[0])
                    {
                        case 0x73: // Duty Canceled (by me?)
                            _logger.Write("Duty Canceled!", LogLevel.Debug);
                            break;
                        case 0x81: // Duty Requested
                            _logger.Write("Duty Requested!", LogLevel.Debug);
                            break;
                    }
                }
                else if (opcode == 0x0304)
                {
                    var waitList = data[6];
                    var waitTime = data[7];
                    var tank = data[8];
                    var tankMax = data[9];
                    var healer = data[10];
                    var healerMax = data[11];
                    var dps = data[12];
                    var dpsMax = data[13];

                    var memberinfo = $"Tanks: {tank}/{tankMax}, Healers: {healer}/{healerMax}, Dps: {dps}/{dpsMax}";
                    _logger.Write($"Q: Matching State Updated [{memberinfo}] - WaitList: {waitList} | WaitTime: {waitTime}", LogLevel.Debug);
                }
                else if (opcode == 0x032F)
                {
                    var tank = data[12];
                    var tankMax = data[13];
                    var healer = data[14];
                    var healerMax = data[15];
                    var dps = data[16];
                    var dpsMax = data[17];
                    var memberinfo = $"Tanks: {tank}/{tankMax}, Healers: {healer}/{healerMax}, Dps: {dps}/{dpsMax}";

                    var code = BitConverter.ToUInt16(data, 8);
                    _logger.Write(code != 0
                            ? $"Q: Matching State Updated [{_dataRepository.GetInstance(code).Name} - {memberinfo}]"
                            : $"Q: Matching State Updated [{memberinfo}]", LogLevel.Debug);
                }
                else if (opcode == 0x022F)
                {
                    var code = BitConverter.ToInt16(data, 4);

                    switch (data[8])
                    {
                        case 0x0B: // Entering
                            var instanceName = code == 0 ? "Unknown Instance" : _dataRepository.GetInstance(code).Name;
                            _logger.Write($"I: Entered Instance Area [{code}] - {instanceName}", LogLevel.Debug);
                            break;
                        case 0x0C: // Leaving
                            _logger.Write($"I: Left Instance Area [{code}]", LogLevel.Debug);
                            break;
                    }
                }
                else if (opcode == 0x0002)
                {
                    _logger.Write("Q: Matching Completed!", LogLevel.Debug);
                }

            }
            catch (Exception ex)
            {
                _logger.Write(ex, "A: Error while analyzing Message", LogLevel.Error);
            }
        }
    }
}
