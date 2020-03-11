using System.Threading.Tasks;
using SchTech.Core.DataAccess;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Abstract
{
    public interface IAdiDataDal : IEntityRepository<Adi_Data>
    {
        Task<bool> CheckAndClearExpiredData(bool timerElapsed);

        Adi_Data GetAdiData(string titlPaid);
    }
}