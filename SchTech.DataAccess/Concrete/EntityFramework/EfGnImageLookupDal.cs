using SchTech.Core.DataAccess.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.DataAccess.Concrete.EntityFramework.Contexts;

namespace SchTech.DataAccess.Concrete.EntityFramework
{
    public class EfGnImageLookupDal : EfEntityRepositoryBase<GN_ImageLookup, ADI_EnrichmentContext>, IGnImageLookupDal
    {
    }
}