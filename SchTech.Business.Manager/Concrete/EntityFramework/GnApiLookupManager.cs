using SchTech.Business.Manager.Abstract.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.Entities.ConcreteTypes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SchTech.Business.Manager.Concrete.EntityFramework
{
    public class GnApiLookupManager : IGnApiLookupService
    {
        private readonly IGnApiLookupDal _gnApiLookupDal;

        public GnApiLookupManager(IGnApiLookupDal gnApiLookupDal)
        {
            _gnApiLookupDal = gnApiLookupDal;
        }

        public List<GN_Api_Lookup> GetList(Expression<Func<GN_Api_Lookup, bool>> filter = null)
        {
            return _gnApiLookupDal.GetList(filter);
        }

        public GN_Api_Lookup Get(Expression<Func<GN_Api_Lookup, bool>> filter)
        {
            return _gnApiLookupDal.Get(filter);
        }

        public GN_Api_Lookup Add(GN_Api_Lookup entity)
        {
            return _gnApiLookupDal.Add(entity);
        }

        public GN_Api_Lookup Update(GN_Api_Lookup entity)
        {
            return _gnApiLookupDal.Update(entity);
        }

        public GN_Api_Lookup Delete(GN_Api_Lookup entity)
        {
            return _gnApiLookupDal.Delete(entity);
        }
    }
}
