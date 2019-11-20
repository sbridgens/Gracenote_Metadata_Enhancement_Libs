using System.Text.RegularExpressions;
using log4net;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.Business.Manager.Concrete.Validation
{
    public class AdiXmlValidator
    {
        /// <summary>
        ///     Initialize Log4net
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(EnhancementDataValidator));

        public string NewTitlPaid { get; set; }
        public string ValidatePaidValue(string adiPaid)
        {
            if (adiPaid.Length == 20)
                return null;

            var tmpPaid = Regex.Replace(adiPaid, "[A-Za-z]", "").TrimStart('0');
            NewTitlPaid = $"TITL{new string('0', 16 - tmpPaid.Length)}{tmpPaid}";
            Log.Info($"Qam asset detected setting GN_Paid = {adiPaid}, " +
                     $"ADI Titl Paid Value = {NewTitlPaid}");

            var onapiProviderid = $"{EnrichmentWorkflowEntities.AdiFile.Asset.Metadata.AMS.Provider_ID}{adiPaid}";
            EnrichmentWorkflowEntities.AdiFile.Asset.Metadata.AMS.Asset_ID = NewTitlPaid;
            Log.Info($"On api Provider id = {onapiProviderid}");

            return onapiProviderid;

        }
    }
}