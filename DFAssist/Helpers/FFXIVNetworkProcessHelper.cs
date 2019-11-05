using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Advanced_Combat_Tracker;
using DFAssist.Contracts;
using DFAssist.Contracts.Duty;
using DFAssist.Contracts.Repositories;
using DFAssist.Core.Network;
using Splat;
using Timer = System.Timers.Timer;

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
        private Timer _timer = new Timer { Interval = 10000 };
        private ConcurrentDictionary<int, ProcessNetwork> _networks = new ConcurrentDictionary<int, ProcessNetwork>();

        public Process ActiveProcess;

        public FFXIVNetworkProcessHelper()
        {
            _timer.Elapsed += Timer_Tick;
        }

        public void Subscribe()
        {
            UpdateProcesses();
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!DFAssistPlugin.Instance.IsPluginEnabled)
                return;

            UpdateProcesses();
        }

        private void UpdateProcesses()
        {
            lock (this)
            {
                var process = Process.GetProcessesByName("ffxiv_dx11").FirstOrDefault();
                if (process == null)
                    return;

                ActiveProcess = process;

                try
                {
                    if (!_networks.ContainsKey(process.Id))
                    {
                        var pn = new ProcessNetwork(process, new Network());
                        _packetHandler.OnEventReceived += Network_onReceiveEvent;
                        _networks.TryAdd(process.Id, pn);
                        _logger.Write($"P: FFXIV Process Selected: {process.Id}", LogLevel.Info);
                    }
                }
                catch (Exception e)
                {
                    _logger.Write(e, "P: Failed to set FFXIV Process", LogLevel.Error);
                }

                var toDelete = new List<int>();
                foreach (var entry in _networks)
                {
                    if (entry.Value.Process.HasExited)
                    {
                        entry.Value.Network.StopCapture();
                        toDelete.Add(entry.Key);
                    }
                    else
                    {
                        if (entry.Value.Network.IsRunning)
                            entry.Value.Network.UpdateGameConnections(entry.Value.Process);
                        else
                        {
                            if (!entry.Value.Network.StartCapture(entry.Value.Process))
                                toDelete.Add(entry.Key);
                        }
                    }
                }

                foreach (var t in toDelete)
                {
                    try
                    {
                        _networks.TryRemove(t, out _);
                        _packetHandler.OnEventReceived -= Network_onReceiveEvent;
                    }
                    catch (Exception e)
                    {
                        _logger.Write(e, "P: Failed to remove FFXIV Process", LogLevel.Error);
                    }
                }
            }
        }

        private void Network_onReceiveEvent(int pid, EventType eventType, int[] args)
        {
            var processMainModule = _networks[pid].Process.MainModule;
            var server = processMainModule != null && processMainModule.FileName.Contains("KOREA") ? "KOREA" : "GLOBAL";
            var text = pid + "|" + server + "|" + eventType + "|";
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
            if (_timer != null)
            {
                if (_timer.Enabled)
                    _timer.Stop();

                _timer.Elapsed -= Timer_Tick;
                _timer.Dispose();
                _timer = null;
            }

            foreach (var entry in _networks)
            {
                entry.Value.Network.StopCapture();
            }

            _networks.Clear();
            _networks = null;
            _packetHandler = null;
            _logger = null;
            _dataRepository = null;
            _instance = null;
        }
    }
    // ReSharper restore InconsistentNaming
}