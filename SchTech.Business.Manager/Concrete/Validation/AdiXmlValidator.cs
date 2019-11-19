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

        public string ValidatePaidValue(string TitlPaidValue)
        {
            if (TitlPaidValue.Length != 20)
            {
                var tmpPaid = Regex.Replace(TitlPaidValue, "[A-Za-z ]", "").TrimStart('0');
                TitlPaidValue = $"TITL{new string('0', 16 - tmpPaid.Length)}{tmpPaid}";
                Log.Info($"Qam asset detected setting GN_Paid = {TitlPaidValue}, " +
                         $"TitlPaid Value = {TitlPaidValue}");
                EnrichmentWorkflowEntities.AdiFile.Asset.Metadata.AMS.Asset_ID = TitlPaidValue;
            }

            var OnapiProviderid = $"{EnrichmentWorkflowEntities.AdiFile.Asset.Metadata.AMS.Provider_ID}{TitlPaidValue}";
            Log.Info($"On api Provider id = {OnapiProviderid}");

            return OnapiProviderid;
        }
    }
}