using System;
using System.IO;
using Advanced_Combat_Tracker;
using DFAssist.Contracts.Repositories;
using DFAssist.Core.Toast;
using Splat;

namespace DFAssist.Helpers
{
    public class ToastHelper : IDisposable
    {
        private static ToastHelper _instance;
        public static ToastHelper Instance => _instance ?? (_instance = new ToastHelper());

        private IActLogger _logger;
        private ILocalizationRepository _localizationRepository;
        private MainControl _mainControl;
        private ActPluginData _pluginData;
        private WinToastWrapper.ToastEventCallback _toastEventCallback;

        public ToastHelper()
        {
            _logger = Locator.Current.GetService<IActLogger>();
            _localizationRepository = Locator.Current.GetService<ILocalizationRepository>();
            _mainControl = Locator.Current.GetService<MainControl>();
            _pluginData = Locator.Current.GetService<ActPluginData>();

            _toastEventCallback = delegate (int code)
            {
                // todo: handle the various codes
                _logger.Write($"This is the Code: {code}", LogLevel.Info);
            };
        }

        public void SendNotification(string title, string message, string testing = "")
        {
            _logger.Write("UI: Request Showing Taost received...", LogLevel.Debug);
            if (_mainControl.DisableToasts.Checked)
            {
                _logger.Write("UI: Toasts are disabled!", LogLevel.Debug);
                return;
            }

            _logger.Write("UI: Using Windows Toasts", LogLevel.Debug);
            try
            {
                _logger.Write("UI: Creating new Toast...", LogLevel.Debug);
                var attribution = _localizationRepository.GetText("app-name");

                if (string.IsNullOrWhiteSpace(testing))
                {
                    WinToastWrapper.CreateToast(
                        DFAssistPlugin.AppId,
                        DFAssistPlugin.AppId,
                        title,
                        message,
                        _toastEventCallback,
                        attribution,
                        true,
                        Duration.Long);
                }
                else
                {
                    WinToastWrapper.CreateToast(
                        DFAssistPlugin.AppId,
                        DFAssistPlugin.AppId,
                        title,
                        message,
                        $"Code [{testing}]",
                        _toastEventCallback,
                        attribution,
                        Duration.Long);
                }
            }
            catch (Exception e)
            {
                _logger.Write(e, "UI: Unable to show toast notification", LogLevel.Error);
            }
        }

        public void Dispose()
        {
            _toastEventCallback = null;
            _pluginData = null;
            _localizationRepository = null;
            _mainControl = null;
            _logger = null;
            _instance = null;
        }
    }
}
