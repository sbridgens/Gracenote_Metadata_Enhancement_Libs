using SchTech.Core.DataAccess.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.DataAccess.Concrete.EntityFramework.Contexts;
using SchTech.Entities.ConcreteTypes;
using System;
using System.Linq;

namespace SchTech.DataAccess.Concrete.EntityFramework
{
    public class EfAdiEnrichmentDal : EfEntityRepositoryBase<Adi_Data, ADI_EnrichmentContext>, IAdiDataDal
    {
        public static bool ExpiryProcessing { get; private set; }

        public static bool IsWorkflowProcessing { get; set; }

        private ADI_EnrichmentContext CurrentContext { get; set; }

        public Adi_Data GetAdiData(Guid adiGuid)
        {
            using (var db = new ADI_EnrichmentContext())
            {
                return db.Adi_Data.FirstOrDefault(i => i.IngestUUID.Equals(adiGuid));
            }
        }
    }
}