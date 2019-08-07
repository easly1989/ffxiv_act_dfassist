using System;
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

        public ToastHelper()
        {
            _logger = Locator.Current.GetService<IActLogger>();
            _localizationRepository = Locator.Current.GetService<ILocalizationRepository>();
            _mainControl = Locator.Current.GetService<MainControl>();
        }

        public void SendNotification(string title, string message, string testing = "", bool isRoulette = false)
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
                var toastImagePath = isRoulette ? "images/roulette.png" : "images/dungeon.png";//todo handle instance type from data
                var attribution = _localizationRepository.GetText("app-name");
                void ToastCallback(int code)
                {
                    //todo handle all the return types and log it
                }

                if (string.IsNullOrWhiteSpace(testing))
                {
                    WinToastWrapper.CreateToast(
                        DFAssistPlugin.AppId,
                        DFAssistPlugin.AppId,
                        title,
                        message,
                        toastImagePath,
                        ToastCallback,
                        attribution,
                        true);
                }
                else
                {
                    WinToastWrapper.CreateToast(
                        DFAssistPlugin.AppId,
                        DFAssistPlugin.AppId,
                        title,
                        message,
                        $"Code [{testing}]",
                        toastImagePath,
                        ToastCallback,
                        attribution);
                }
                _logger.Write("UI: Show Toast Requested...", LogLevel.Debug);
            }
            catch (Exception e)
            {
                _logger.Write(e, "UI: Unable to show toast notification", LogLevel.Error);
            }
        }

        public void Dispose()
        {
            _localizationRepository = null;
            _mainControl = null;
            _logger = null;
            _instance = null;
        }
    }
}
