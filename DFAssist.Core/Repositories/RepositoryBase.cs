using System;
using System.IO;
using System.Net;
using DFAssist.Contracts.Repositories;
using Splat;

namespace DFAssist.Core.Repositories
{
    public abstract class RepositoryBase : IRepository
    {
        private static readonly string[] SupportedLanguages = { "en-us", "fr-fr", "ja-jp", "ko-kr"};
        
        public bool Initialized { get; protected set; }
        public decimal Version { get; protected set; }
        public string CurrentLanguage { get; protected set; }

        protected ILogger Logger { get; }
        
        protected RepositoryBase()
        {
            Initialized = false;
            Version = default;
            CurrentLanguage = string.Empty;

            Logger = Locator.Current.GetService<ILogger>();
        }

        public void LocalUpdate(string pluginPath, string language)
        {
            if(CurrentLanguage.Equals(language, StringComparison.InvariantCultureIgnoreCase))
            {
                Logger.Write($"Local data already updated for language {language}", LogLevel.Debug);
                return;
            }

            OnLocalUpdatedRequested(pluginPath, language);
        }

        public void WebUpdate(string pluginPath)
        {
            OnWebUpdateRequested(pluginPath);
        }

        protected void WebUpdateRoutine(string pluginPath, string folderName)
        {
            foreach (var supportedLanguage in SupportedLanguages)
            {
                Logger.Write($"Downloading {supportedLanguage} file", LogLevel.Debug);
                var json = DownloadString($"https://raw.githubusercontent.com/easly1989/ffxiv_act_dfassist/master/DFAssist/Resources/{folderName}/{supportedLanguage}.json");
                if(string.IsNullOrWhiteSpace(json))
                {
                    Logger.Write($"Unable to update {supportedLanguage} file", LogLevel.Warn);
                    continue;
                }

                SaveToFile(json, Path.Combine(pluginPath, folderName, $"{supportedLanguage}.json"));
                Logger.Write($"Updated {supportedLanguage} file", LogLevel.Debug);
            }
        }

        protected string DownloadString(string url)
        {
            try
            {
                var webClient = new WebClient();
                webClient.Headers.Add("user-agent", "avoid 403");
                webClient.Encoding = System.Text.Encoding.UTF8;
                var downloadString = webClient.DownloadString(url);
                webClient.Dispose();
                return downloadString;
            }
            catch (Exception e)
            {
                Logger.Write(e, $"An error occured while processing DFAssist Data, method: {nameof(DownloadString)}", LogLevel.Error);
            }

            return string.Empty;
        }

        protected bool SaveToFile(string content, string path)
        {
            if(string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(content))
            {
                Logger.Write("Unable to save with path or content null, contact the dev!", LogLevel.Warn);
                return false;
            }

            try
            {
                File.WriteAllText(path, content);
            }
            catch (Exception e)
            {
                Logger.Write(e, $"An error occured while processing DFAssist Data, method: {nameof(SaveToFile)}", LogLevel.Error);
            }

            return false;
        }

        protected string ReadFromFile(string path)
        {
            if(string.IsNullOrWhiteSpace(path))
            {
                Logger.Write("Unable to read from a null path, contact the dev!", LogLevel.Warn);
                return string.Empty;
            }

            try
            {
                return File.ReadAllText(path);
            }
            catch (Exception e)
            {
                Logger.Write(e, $"An error occured while processing DFAssist Data, method: {nameof(ReadFromFile)}", LogLevel.Error);
            }

            return string.Empty;
        }

        protected abstract void OnLocalUpdatedRequested(string pluginPath, string language);
        protected abstract void OnWebUpdateRequested(string pluginPath);
    }
}