using SchTech.Core.DataAccess;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Abstract
{
    public interface IAdiDataDal : IEntityRepository<Adi_Data>
    {
        bool CleanAdiDataWithNoMapping();
    }
}