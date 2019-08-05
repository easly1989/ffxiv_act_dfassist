using DFAssist.Contracts.DataModel;

namespace DFAssist.Contracts.Repositories
{
    public interface IDataRepository : IRepository
    {
        Instance GetInstance(int code);
        Roulette GetRoulette(int code);
    }
}