using System.Xml.Serialization;

namespace SchTech.Configuration.Manager.Schema.ADIWFE
{
    //[XmlRoot(ElementName = "ExtraDataConfig")]
    //public class ExtraDataConfig
    //{
    //    [XmlAttribute(AttributeName = "providers")]
    //    public static string Providers { get; set; }
    //    [XmlAttribute(AttributeName = "extradata1")]
    //    public static string Extradata1 { get; set; }
    //    [XmlAttribute(AttributeName = "extradata3")]
    //    public static string Extradata3 { get; set; }
    //}
    [XmlRoot(ElementName = "LegacyGoAllowedProviders")]
    public class LegacyGoAllowedProviders
    {
        [XmlAttribute(AttributeName = "GoProviders")]
        public static string GoProviders { get; set; }

        [XmlAttribute(AttributeName = "MoveNonLegacyToDirectory")]
        public static string MoveNonLegacyToDirectory { get; set; }
    }

    [XmlRoot(ElementName = "Block_Platform")]
    public class Block_Platform
    {
        [XmlAttribute(AttributeName = "providers")]
        public static string Providers { get; set; }

        [XmlAttribute(AttributeName = "BlockPlatformValue")]
        public static string BlockPlatformValue { get; set; }
    }

    [XmlRoot(ElementName = "ADIWF_Config")]
    public class ADIWF_Config
    {
        [XmlElement(ElementName = "InputDirectory")]
        public static string InputDirectory { get; set; }

        [XmlElement(ElementName = "PollIntervalInSeconds")]
        public static string PollIntervalInSeconds { get; set; }

        [XmlElement(ElementName = "ExpiredAssetCleanupIntervalHours")]
        public static string ExpiredAssetCleanupIntervalHours { get; set; }

        [XmlElement(ElementName = "MinusExpiredAssetWindowHours")]
        public static string MinusExpiredAssetWindowHours { get; set; }

        [XmlElement(ElementName = "AllowSDContentIngest")]
        public static string AllowSdContentIngest { get; set; }

        [XmlElement(ElementName = "UnrequiredSDContentDirectory")]
        public static string UnrequiredSdContentDirectory { get; set; }

        [XmlElement(ElementName = "TempWorkingDirectory")]
        public static string TempWorkingDirectory { get; set; }

        [XmlElement(ElementName = "FailedDirectory")]
        public static string FailedDirectory { get; set; }

        [XmlElement(ElementName = "IngestDirectory")]
        public static string IngestDirectory { get; set; }

        [XmlElement(ElementName = "UpdatesFailedDirectory")]
        public static string UpdatesFailedDirectory { get; set; }

        [XmlElement(ElementName = "MoveNonMappedDirectory")]
        public static string MoveNonMappedDirectory { get; set; }

        [XmlElement(ElementName = "ProcessExpiredAssets")]
        public static bool ProcessExpiredAssets { get; set; }

        [XmlElement(ElementName = "ProcessMappingFailures")]
        public static bool ProcessMappingFailures { get; set; }

        [XmlElement(ElementName = "RepollNonMappedIntervalHours")]
        public static string RepollNonMappedIntervalHours { get; set; }

        [XmlElement(ElementName = "FailedToMap_Max_Retry_Days")]
        public static string FailedToMapMaxRetryDays { get; set; }

        [XmlElement(ElementName = "TVOD_Delivery_Directory")]
        public static string TvodDeliveryDirectory { get; set; }

        [XmlElement(ElementName = "OnApi")] public static string OnApi { get; set; }

        [XmlElement(ElementName = "ApiKey")] public static string ApiKey { get; set; }

        [XmlElement(ElementName = "MediaCloud")]
        public static string MediaCloud { get; set; }

        [XmlElement(ElementName = "Database_Host")]
        public static string DatabaseHost { get; set; }

        [XmlElement(ElementName = "Database_Name")]
        public static string DatabaseName { get; set; }

        [XmlElement(ElementName = "Integrated_Security")]
        public static string IntegratedSecurity { get; set; }

        //[XmlElement(ElementName = "ExtraDataConfig")]
        //public static ExtraDataConfig ExtraDataConfig { get; set; }
        [XmlElement(ElementName = "Block_Platform")]
        public static Block_Platform BlockPlatform { get; set; }

        [XmlElement(ElementName = "LegacyGoAllowedProviders")]
        public static LegacyGoAllowedProviders LegacyGoAllowedProviders { get; set; }

        [XmlElement(ElementName = "Prefix_Show_ID_Value")]
        public static string PrefixShowIdValue { get; set; }

        [XmlElement(ElementName = "Prefix_Series_ID_Value")]
        public static string PrefixSeriesIdValue { get; set; }
    }
}