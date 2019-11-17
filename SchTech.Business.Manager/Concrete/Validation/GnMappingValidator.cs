using System;
using System.Linq;
using SchTech.Business.Manager.Concrete.EntityFramework;
using SchTech.DataAccess.Concrete.EntityFramework;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.Business.Manager.Concrete.Validation
{
    public class GnMappingValidator
    {
        //public static bool ValidateMappingEntry(string tmsId, string paid, bool isUpdate)
        //{
        //    var gnMappingDataService =
        //            new GnMappingDataManager(new EfGnMappingDataDal());

        //    var mappingExists =
        //        gnMappingDataService.GetList(p => p.GN_Paid == paid).FirstOrDefault();


        //    var hasEntry = mappingExists?.GN_TMSID == tmsId && mappingExists?.GN_Paid == paid;

        //    return isUpdate && hasEntry;
        //}

    }
}
