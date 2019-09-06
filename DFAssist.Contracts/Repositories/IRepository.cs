namespace DFAssist.Contracts.Repositories
{
    public interface IRepository
    {
        bool Initialized { get; }
        decimal Version { get; }
        string CurrentLanguage { get; }

        /// <summary>
        /// Used to update the repository based on the choosen language for the UI
        /// it uses the files that resides locally on the computer
        /// </summary>
        void LocalUpdate(string pluginPath, string language);
        /// <summary>
        /// Used to updated the local file from the github reference files
        /// It simply downloads the new files, and overwrites the old ones
        /// </summary>
        void WebUpdate();
    }
}
