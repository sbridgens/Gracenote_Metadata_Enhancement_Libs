using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchTech.Core.DataAccess;
using SchTech.DataAccess.Concrete;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Abstract
{
    public interface IGnImageLookupDal : IEntityRepository<GN_ImageLookup>
    {
        //List<GN_ImageLookup> ReturnImageLookups();
    }
}
