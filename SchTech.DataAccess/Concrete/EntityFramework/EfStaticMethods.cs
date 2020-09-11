using log4net;
using SchTech.DataAccess.Concrete.EntityFramework.Contexts;
using System;
using System.Linq;

namespace SchTech.DataAccess.Concrete.EntityFramework
{
    public class EfStaticMethods
    {
        /// <summary>
        ///     Initialize Log4net
        /// </summary>
        public static readonly ILog Log = LogManager.GetLogger(typeof(EfStaticMethods));

        public static string GetPaidLastValue(string paidvalue)
        {
            return paidvalue.Replace("TITL", "")
                .Replace("i", "")
                .TrimStart('0');
        }

        public static string GetEnrichedAdiFile(Guid adiGuid)
        {
            using (var adiContext = new ADI_EnrichmentContext())
            {
                return adiContext.Adi_Data.FirstOrDefault(p => p.IngestUUID == adiGuid)?.EnrichedAdi;
            }
        }
    }
}