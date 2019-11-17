using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SchTech.DataAccess.Abstract;
using SchTech.DataAccess.Concrete.EntityFramework.Contexts;

namespace SchTech.DataAccess.Concrete.EntityFramework
{
    public class EfStaticMethods
    {
        /// <summary>
        /// Initialize Log4net
        /// </summary>
        public static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(EfStaticMethods));

        public static string GetPaidLastValue(string paidvalue)
        {
            return paidvalue.Replace("TITL", "")
                            .Replace("i", "")
                            .TrimStart('0');
        }

        public static string GetEnrichedAdiFile(string paidvalue)
        {

            using (var adiContext = new ADI_EnrichmentContext())
            {
                return adiContext.Adi_Data.FirstOrDefault(p => p.TitlPaid == paidvalue)
                    ?.EnrichedAdi;
            }
        }

        public static bool CheckAndUpdateTmsId(string paidValue, string currentTmsId)
        {
            using (var adiContext = new ADI_EnrichmentContext())
            {
                var dbPaid = adiContext.GN_Mapping_Data.FirstOrDefault(p => p.GN_Paid.Contains(GetPaidLastValue(p.GN_Paid)));

                if (dbPaid != null && dbPaid.GN_TMSID != currentTmsId)
                {
                    Log.Warn(
                        $"DB TMSID and API TMSID Mismatch: DB TMSID is {dbPaid.GN_Paid} and API TMSID is: {currentTmsId} GN Mapping update occured, updating db with new GN Data");
                    dbPaid.GN_TMSID = currentTmsId;
                    var adiRow =
                        adiContext.Adi_Data.FirstOrDefault(p => p.TitlPaid.Contains(GetPaidLastValue(paidValue)));

                    if (adiRow != null)
                    {
                        adiRow.TmsId = currentTmsId;

                        adiContext.SaveChanges();
                        Log.Info("db TMSID updated with new api value.");
                        return true;
                    }

                    Log.Error("Error retrieving adi_data row in order to update.");
                    return false;
                }
            }

            return true;
        }
    }
}
