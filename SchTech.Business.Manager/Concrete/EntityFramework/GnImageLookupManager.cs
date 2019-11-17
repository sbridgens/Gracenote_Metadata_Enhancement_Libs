using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SchTech.Business.Manager.Abstract.EntityFramework;
using SchTech.Core.DataAccess;
using SchTech.DataAccess.Abstract;
using SchTech.DataAccess.Concrete;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.Business.Manager.Concrete.EntityFramework
{
    public class GnImageLookupManager : IGnImageLookupService
    {

        private readonly IGnImageLookupDal _gnImageLookupDal;

        public GnImageLookupManager(IGnImageLookupDal gnImageLookupDal)
        {
            _gnImageLookupDal = gnImageLookupDal;
        }

        public List<GN_ImageLookup> GetList(Expression<Func<GN_ImageLookup, bool>> filter = null)
        {
            return _gnImageLookupDal.GetList(filter);
        }

        public GN_ImageLookup Get(Expression<Func<GN_ImageLookup, bool>> filter)
        {
            return _gnImageLookupDal.Get(filter);
        }

        GN_ImageLookup IEntityRepository<GN_ImageLookup>.Add(GN_ImageLookup entity)
        {
            return _gnImageLookupDal.Add(entity);
        }

        GN_ImageLookup IEntityRepository<GN_ImageLookup>.Update(GN_ImageLookup entity)
        {
            return _gnImageLookupDal.Update(entity);
        }

        GN_ImageLookup IEntityRepository<GN_ImageLookup>.Delete(GN_ImageLookup entity)
        {
            return _gnImageLookupDal.Delete(entity);
        }
    }
}
