using SchTech.Core.DataAccess.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.DataAccess.Concrete.EntityFramework.Contexts;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Concrete.EntityFramework
{
    public class EfGnProgramTypeLookupDal : EfEntityRepositoryBase<GnProgramTypeLookup, ADI_EnrichmentContext>, IGnProgramTypeLookupDal
    {
    }
}
