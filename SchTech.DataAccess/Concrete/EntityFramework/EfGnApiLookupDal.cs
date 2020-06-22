﻿using SchTech.Core.DataAccess.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.DataAccess.Concrete.EntityFramework.Contexts;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Concrete.EntityFramework
{
    public class EfGnApiLookupDal : EfEntityRepositoryBase<GN_Api_Lookup, ADI_EnrichmentContext>, IGnApiLookupDal
    {
    }
}
