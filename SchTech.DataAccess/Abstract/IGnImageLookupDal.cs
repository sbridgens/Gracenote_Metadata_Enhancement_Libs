using SchTech.Core.DataAccess;
using SchTech.DataAccess.Concrete;

namespace SchTech.DataAccess.Abstract
{
    public interface IGnImageLookupDal : IEntityRepository<GN_ImageLookup>
    {
        //List<GN_ImageLookup> ReturnImageLookups();
    }
}