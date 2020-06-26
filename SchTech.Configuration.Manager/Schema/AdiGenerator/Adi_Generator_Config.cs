using System.Xml.Serialization;

namespace SchTech.Configuration.Manager.Schema.AdiGenerator
{
    [XmlRoot(ElementName = "Adi_Generator_Config")]
    public class Adi_Generator_Config
    {
        [XmlElement(ElementName = "Database_Host")]
        public static string Database_Host { get; set; }

        [XmlElement(ElementName = "Database_Name")]

        [XmlElement(ElementName = "OnApi")] 
        public static string OnApi { get; set; }

        [XmlElement(ElementName = "ApiKey")] 
        public static string ApiKey { get; set; }

        public static string Database_Name { get; set; }

        [XmlElement(ElementName = "Integrated_Security")]
        public static string Integrated_Security { get; set; }

        [XmlElement(ElementName = "PollIntervalInMinutes")]
        public static string PollIntervalInMinutes { get; set; }

        [XmlElement(ElementName = "TempWorkingDirectory")]
        public static string TempWorkingDirectory { get; set; }

        [XmlElement(ElementName = "DeliveryDirectory")]
        public static string DeliveryDirectory { get; set; }
    }
}
