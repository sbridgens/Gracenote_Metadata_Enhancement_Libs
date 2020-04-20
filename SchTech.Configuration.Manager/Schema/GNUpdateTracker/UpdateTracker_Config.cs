using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SchTech.Configuration.Manager.Schema.GNUpdateTracker
{
    [XmlRoot(ElementName = "GN_UpdateTracker_Config")]
    public class GN_UpdateTracker_Config
    {
        [XmlElement(ElementName = "OnApi")]
        public static string OnApi { get; set; }
        [XmlElement(ElementName = "ApiKey")]
        public static string ApiKey { get; set; }
        [XmlElement(ElementName = "MediaCloud")]
        public static string MediaCloud { get; set; }
        [XmlElement(ElementName = "Database_Host")]
        public static string Database_Host { get; set; }
        [XmlElement(ElementName = "Database_Name")]
        public static string Database_Name { get; set; }
        [XmlElement(ElementName = "Integrated_Security")]
        public static string Integrated_Security { get; set; }
        [XmlElement(ElementName = "PollIntervalInMinutes")]
        public static string PollIntervalInMinutes { get; set; }
    }
}
