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
    public interface IGnUpdateTrackingDal : IEntityRepository<GN_UpdateTracking>
    {
        GN_UpdateTracking GetTrackingItemByUid(Guid ingestUUID);

        GN_UpdateTracking GetTrackingItemByPidPaid(string gnProviderId);

        string GetLowestMappingUpdateId();

        string GetLowestLayer1UpdateId();

        string GetLowestLayer2UpdateId();
        
    }
}
