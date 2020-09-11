using SchTech.Core.DataAccess;
using SchTech.Entities.ConcreteTypes;
using System;

namespace SchTech.DataAccess.Abstract
{
    public interface IAdiDataDal : IEntityRepository<Adi_Data>
    {
        Adi_Data GetAdiData(Guid adiGuid);
    }
}