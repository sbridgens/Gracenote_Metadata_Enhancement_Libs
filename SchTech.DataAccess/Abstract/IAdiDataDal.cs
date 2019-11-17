using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchTech.Core.DataAccess;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Abstract
{
    public interface IAdiDataDal : IEntityRepository<Adi_Data>
    {
        bool CleanAdiDataWithNoMapping();
    }
}
