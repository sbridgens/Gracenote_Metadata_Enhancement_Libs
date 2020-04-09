using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SchTech.Business.Manager.Abstract.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.Business.Manager.Concrete.EntityFramework
{
    public class GnProgramTypeLookupManager : IGnProgramTypeLookupService
    {
        private readonly IGnProgramTypeLookupDal _gnProgramTypeLookupDal;
        
        public GnProgramTypeLookupManager(IGnProgramTypeLookupDal programTypeLookupDal)
        {
            _gnProgramTypeLookupDal = programTypeLookupDal;
        }

        public List<GnProgramTypeLookup> GetList(Expression<Func<GnProgramTypeLookup, bool>> filter = null)
        {
            return _gnProgramTypeLookupDal.GetList(filter);
        }

        public GnProgramTypeLookup Get(Expression<Func<GnProgramTypeLookup, bool>> filter)
        {
            return _gnProgramTypeLookupDal.Get(filter);
        }

        public GnProgramTypeLookup Add(GnProgramTypeLookup entity)
        {
            return _gnProgramTypeLookupDal.Add(entity);
        }

        public GnProgramTypeLookup Update(GnProgramTypeLookup entity)
        {
            return _gnProgramTypeLookupDal.Update(entity);
        }

        public GnProgramTypeLookup Delete(GnProgramTypeLookup entity)
        {
            return _gnProgramTypeLookupDal.Delete(entity);
        }
    }
}
