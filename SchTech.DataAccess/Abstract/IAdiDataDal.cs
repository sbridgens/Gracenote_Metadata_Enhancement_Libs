using System;
using System.Threading.Tasks;
using SchTech.Core.DataAccess;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Abstract
{
    public interface IAdiDataDal : IEntityRepository<Adi_Data>
    {
        Adi_Data GetAdiData(Guid adiGuid);
    }
}