using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchTech.Core.DataAccess.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.DataAccess.Concrete.EntityFramework.Contexts;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Concrete.EntityFramework
{
    public class EfGnImageLookupDal : EfEntityRepositoryBase<GN_ImageLookup, ADI_EnrichmentContext>, IGnImageLookupDal
    {
        //public static List<GN_ImageLookup> ReturnImageLookups()
        //{
        //    using (var images = new ADI_EnrichmentContext())
        //    {
        //        return images.GN_ImageLookup.OrderBy(o => o.Image_AdiOrder).ToList();
        //    }
        //}
    }
}
