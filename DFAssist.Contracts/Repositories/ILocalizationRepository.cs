namespace DFAssist.Contracts.Repositories
{
    public interface ILocalizationRepository : IRepository
    {
        string GetText(string codeToTranslate, string fallBackMessage, params object[] arguments);
        string GetText(string codeToTranslate, params object[] arguments);
    }
}