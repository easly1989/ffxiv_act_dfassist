using System;
using System.Diagnostics;
using Advanced_Combat_Tracker;
using DFAssist.Contracts;
using DFAssist.Contracts.Duty;
using DFAssist.Contracts.Repositories;
using Machina.FFXIV;
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

        private FFXIVNetworkMonitor _ffxivNetworkMonitor;

        public Process ActiveProcess
        {
            get
            {
                if (_ffxivNetworkMonitor == null)
                    return default;

                var pid = Convert.ToInt32(_ffxivNetworkMonitor.ProcessID);
                if(pid == 0)
                    return default;

                var activeProcess = Process.GetProcessById(pid);
                return activeProcess;
            }
        }

        public FFXIVNetworkProcessHelper()
        {
            _ffxivNetworkMonitor = new FFXIVNetworkMonitor
            {
                MessageReceived = (connection, epoch, message) => _packetHandler.HandleMessage(message, OnMessageReceived)
            };
        }

        public void Subscribe()
        {
            _ffxivNetworkMonitor.Start();
            _logger.Write("N: FFXIV Network Monitor Started!", LogLevel.Info);
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
            if (_ffxivNetworkMonitor != null)
            {
                _ffxivNetworkMonitor.Stop();
                _ffxivNetworkMonitor.MessageReceived = null;
            }

            _ffxivNetworkMonitor = null;
            _packetHandler = null;
            _logger = null;
            _dataRepository = null;
            _instance = null;
        }
    }
    // ReSharper restore InconsistentNaming
}