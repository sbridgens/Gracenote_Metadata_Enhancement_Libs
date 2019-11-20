using SchTech.Business.Manager.Abstract.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.Entities.ConcreteTypes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SchTech.Business.Manager.Concrete.EntityFramework
{
    public class AdiEnrichmentManager : IAdiEnrichmentService
    {
        private readonly IAdiDataDal _adiDataDal;

        public AdiEnrichmentManager(
            IAdiDataDal adiDataDal)
        {
            _adiDataDal = adiDataDal;
        }

        public List<Adi_Data> GetList(Expression<Func<Adi_Data, bool>> filter = null)
        {
            return _adiDataDal.GetList(filter);
        }

        public Adi_Data Get(Expression<Func<Adi_Data, bool>> filter)
        {
            return _adiDataDal.Get(filter);
        }

        public Adi_Data Add(Adi_Data adiData)
        {
            return _adiDataDal.Add(adiData);
        }

        public Adi_Data Delete(Adi_Data adiData)
        {
            return _adiDataDal.Delete(adiData);
        }

        public bool CleanAdiDataWithNoMapping()
        {
            return _adiDataDal.CleanAdiDataWithNoMapping();
        }

        public Adi_Data Update(Adi_Data adiData)
        {
            return _adiDataDal.Update(adiData);
        }

        public Adi_Data GetAdiData(string titlPaid)
        {
            return _adiDataDal.GetAdiData(titlPaid);
        }
    }
}