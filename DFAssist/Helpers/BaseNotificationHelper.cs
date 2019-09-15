using System;
using DFAssist.Contracts.Repositories;
using Splat;

namespace DFAssist.Helpers
{
    public abstract class BaseNotificationHelper<T> : IDisposable
        where T : class
    {
        private static T _instance;
        public static T Instance => _instance ?? (_instance = Activator.CreateInstance<T>());

        protected IActLogger Logger;
        protected ILocalizationRepository LocalizationRepository;
        protected MainControl MainControl;

        protected BaseNotificationHelper()
        {
            Logger = Locator.Current.GetService<IActLogger>();
            LocalizationRepository = Locator.Current.GetService<ILocalizationRepository>();
            MainControl = Locator.Current.GetService<MainControl>();
        }

        public void SendNotification(string title = "", string message = "", string testing = "")
        {
            OnSendNotification(title, message, testing);
        }

        protected abstract void OnSendNotification(string title, string message, string testing);

        public void Dispose()
        {
            OnDisposeOwnedObjects();
            OnSetNullOwnedObjects();
        }

        protected virtual void OnDisposeOwnedObjects()
        {
        }

        protected virtual void OnSetNullOwnedObjects()
        {
            LocalizationRepository = null;
            MainControl = null;
            Logger = null;
            _instance = null;
        }
    }
}