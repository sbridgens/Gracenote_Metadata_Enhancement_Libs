using SchTech.Core.DataAccess;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Abstract
{
    public interface IAdiDataDal : IEntityRepository<Adi_Data>
    {
        bool CleanAdiDataWithNoMapping(bool timerElapsed);

        Adi_Data GetAdiData(string titlPaid);
    }
}