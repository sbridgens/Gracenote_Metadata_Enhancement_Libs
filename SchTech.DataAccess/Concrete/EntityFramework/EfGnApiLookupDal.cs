using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using SchTech.Core.DataAccess.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.DataAccess.Concrete.EntityFramework.Contexts;
using SchTech.Entities.ConcreteTypes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace SchTech.DataAccess.Concrete.EntityFramework
{
    class EfGnApiLookupDal : EfEntityRepositoryBase<GN_Api_Lookup, ADI_EnrichmentContext>, IGnApiLookupDal
    {
    }
}
