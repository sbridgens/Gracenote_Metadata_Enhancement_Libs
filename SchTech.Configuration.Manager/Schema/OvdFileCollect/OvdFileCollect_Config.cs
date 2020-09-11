using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SchTech.Configuration.Manager.Schema.OvdFileCollect
{
	[XmlRoot(ElementName = "OVDFileCollectConfig")]
	public class OVDFileCollectConfig
	{
		[XmlElement(ElementName = "SQLiteDB")]
		public static string SQLiteDB { get; set; }
		[XmlElement(ElementName = "RemoteFileExtension")]
		public static string RemoteFileExtension { get; set; }
		[XmlElement(ElementName = "FileNameRegex")]
		public static string FileNameRegex { get; set; }
		[XmlElement(ElementName = "CheckFileDays")]
		public static string CheckFileDays { get; set; }
		[XmlElement(ElementName = "PollOVDMinutes")]
		public static string PollOVDMinutes { get; set; }



		[XmlElement(ElementName = "UseProxy")]
		public static string UseProxy { get; set; }
		[XmlElement(ElementName = "ProxyHost")]
		public static string ProxyHost { get; set; }
		[XmlElement(ElementName = "ProxyPort")]
		public static string ProxyPort { get; set; }
		[XmlElement(ElementName = "ProxyUsername")]
		public static string ProxyUsername { get; set; }
		[XmlElement(ElementName = "ProxyPassword")]
		public static string ProxyPassword { get; set; }



		[XmlElement(ElementName = "OVDHost")]
		public static string OVDHost { get; set; }
		[XmlElement(ElementName = "OVDUserName")]
		public static string OVDUserName { get; set; }
		[XmlElement(ElementName = "OVDPassword")]
		public static string OVDPassword { get; set; }
		[XmlElement(ElementName = "OVDRemoteDirectory")]
		public static string OVDRemoteDirectory { get; set; }
		[XmlElement(ElementName = "LocalFileDirectory")]
		public static string LocalFileDirectory { get; set; }
	}

}
