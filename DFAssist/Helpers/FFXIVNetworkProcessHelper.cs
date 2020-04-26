using System;
using System.Diagnostics;
using System.Linq;
using Advanced_Combat_Tracker;
using DFAssist.Contracts;
using DFAssist.Contracts.Duty;
using DFAssist.Contracts.Repositories;
using Splat;

namespace DFAssist.Helpers
{
    // ReSharper disable InconsistentNaming
    public class FFXIVNetworkProcessHelper : IDisposable
    {
        private static FFXIVNetworkProcessHelper _instance;
        public static FFXIVNetworkProcessHelper Instance => _instance ?? (_instance = new FFXIVNetworkProcessHelper());

        private IActLogger _logger = Locator.Current.GetService<IActLogger>();
        private IPacketHandler _packetHandler = Locator.Current.GetService<IPacketHandler>();
        private IDataRepository _dataRepository = Locator.Current.GetService<IDataRepository>();
        private FFXIV_ACT_Plugin.FFXIV_ACT_Plugin _ffPlugin = Locator.Current.GetService<FFXIV_ACT_Plugin.FFXIV_ACT_Plugin>();

        private Process _process;
        public Process ActiveProcess
        {
            get
            {
                if (_ffPlugin == null)
                    return default;


                if (_process != null && !_process.HasExited)
                    return _process;

                _process = _ffPlugin.DataRepository.GetCurrentFFXIVProcess()
                           ?? Process
                    .GetProcessesByName("ffxiv_dx11")
                    .FirstOrDefault();

                return _process;
            }
        }

        public void Subscribe()
        {
            var opcode = new OpCodeHelper();


            var alertOpCode = opcode.GetAlertOpCode();
            _logger.Write($"alertOpCode:0x{alertOpCode:X}",LogLevel.Warn);
            _packetHandler.RegisterMessageHandler(alertOpCode, AlertEventHandler);
            
            _ffPlugin.DataSubscription.NetworkReceived += DataSubscriptionOnNetworkReceived;
            _logger.Write("N: FFXIV Network Monitor Started!", LogLevel.Info);
        }

        private void AlertEventHandler(byte[] data)
        {
            var matchedRoulette = BitConverter.ToUInt16(data, 2);
            var matchedCode = BitConverter.ToUInt16(data, 20);



            var args = new int[] {matchedRoulette, matchedCode};
            var instanceString = $"{matchedCode} - {_dataRepository.GetInstance(matchedCode).Name}";
            _logger.Write(matchedRoulette != 0
                ? $"Q: Matched [{matchedRoulette} - {_dataRepository.GetRoulette(matchedRoulette).Name}] - [{instanceString}]"
                : $"Q: Matched [{instanceString}]", LogLevel.Info);




            var text = string.Empty;
            if (ActiveProcess != null)
            {
                var processMainModule = ActiveProcess.MainModule;
                var server = processMainModule != null && processMainModule.FileName.Contains("KOREA") ? "KOREA" : "GLOBAL";
                text = ActiveProcess.Id + "|" + server + "|";
            }



            text += EventType.MATCH_ALERT + "|";
            var pos = 0;

            if (args[0] != 0)
            {
                text += _dataRepository.GetRoulette(args[0]).Name + "|";
                pos++;
            }
            text += _dataRepository.GetInstance(args[1]).Name + "|";
            pos++;

            for (var i = pos; i < args.Length; i++)
            {
                text += args[i] + "|";
            }

            ActGlobals.oFormActMain.ParseRawLogLine(false, DateTime.Now, "00|" + DateTime.Now.ToString("O") + "|0048|F|" + text);
            DFAssistPlugin.Instance.OnNetworkEventReceived(EventType.MATCH_ALERT, args);
        }

        private void DataSubscriptionOnNetworkReceived(string connection, long epoch, byte[] message)
        {
            _packetHandler.HandleMessage(message);
        }

        private void OnMessageReceived(EventType eventType, int[] args)
        {
            _logger.Write("N: FFXIV Network packet received...", LogLevel.Debug);

            var text = string.Empty;
            if (ActiveProcess != null)
            {
                var processMainModule = ActiveProcess.MainModule;
                var server = processMainModule != null && processMainModule.FileName.Contains("KOREA") ? "KOREA" : "GLOBAL";
                text = ActiveProcess.Id + "|" + server + "|";
            }

            text += eventType + "|";
            var pos = 0;

            switch (eventType)
            {
                case EventType.INSTANCE_ENTER:
                case EventType.INSTANCE_EXIT:
                    if (args.Length > 0)
                    {
                        text += _dataRepository.GetInstance(args[0]).Name + "|";
                        pos++;
                    }

                    break;
                case EventType.MATCH_BEGIN:
                    text += (MatchType)args[0] + "|";
                    pos++;
                    switch ((MatchType)args[0])
                    {
                        case MatchType.ROULETTE:
                            text += _dataRepository.GetRoulette(args[0]).Name + "|";
                            pos++;
                            break;
                        case MatchType.SELECTIVE:
                            text += args[1] + "|";
                            pos++;
                            var p = pos;
                            for (var i = p; i < args.Length; i++)
                            {
                                text += _dataRepository.GetInstance(args[1]).Name + "|";
                                pos++;
                            }

                            break;
                    }

                    break;
                case EventType.MATCH_END:
                    text += (MatchEndType)args[0] + "|";
                    pos++;
                    break;
                case EventType.MATCH_PROGRESS:
                    text += _dataRepository.GetInstance(args[0]).Name + "|";
                    pos++;
                    break;
                case EventType.MATCH_ALERT:
                    if (args[0] != 0)
                    {
                        text += _dataRepository.GetRoulette(args[0]).Name + "|";
                        pos++;
                    }
                    text += _dataRepository.GetInstance(args[1]).Name + "|";
                    pos++;
                    break;
            }

            for (var i = pos; i < args.Length; i++)
            {
                text += args[i] + "|";
            }

            ActGlobals.oFormActMain.ParseRawLogLine(false, DateTime.Now, "00|" + DateTime.Now.ToString("O") + "|0048|F|" + text);
            DFAssistPlugin.Instance.OnNetworkEventReceived(eventType, args);
        }

        public void Dispose()
        {
            if (_ffPlugin != null)
            {
                _ffPlugin.DataSubscription.NetworkReceived -= DataSubscriptionOnNetworkReceived;
            }

            _packetHandler?.UnregisterHandlers();

            _ffPlugin = null;
            _packetHandler = null;
            _logger = null;
            _dataRepository = null;
            _instance = null;
        }
    }
    // ReSharper restore InconsistentNaming
}