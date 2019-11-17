using SchTech.Configuration.Manager.Schema.ADIWFE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SchTech.Configuration.Manager.Concrete
{
    public class ConfigSerializationHelper
    {
        public bool LoadConfigurationFile(string configFile)
        {
            bool configLoaded = true;

            XDocument xDoc = XDocument.Load(configFile);
            var configs = xDoc.Elements(xDoc.Root.Name);
            PropertyInfo[] properties = typeof(ADIWF_Config).GetProperties();
            foreach (XElement ele in configs.Descendants())
            {
                foreach (PropertyInfo pinf in properties)
                {
                    if (pinf.Name == ele.Name.LocalName)
                    {
                        switch (ele.Name.LocalName)
                        {
                            //case "ExtraDataConfig":
                            //    ExtraDataConfig.Providers = ele.Attribute("providers").Value;
                            //    ExtraDataConfig.Extradata1 = ele.Attribute("extradata1").Value;
                            //    ExtraDataConfig.Extradata3 = ele.Attribute("extradata3").Value;

                            //    B_IsRunning = true;
                            //    break;
                            case "Block_Platform":
                                Block_Platform.Providers = ele.Attribute("providers").Value;
                                Block_Platform.BlockPlatformValue = ele.Attribute("BlockPlatformValue").Value;
                                break;
                            default:
                                pinf.SetValue(pinf.Name, ele.Value);

                                if (pinf.GetValue(null, null) != null)
                                    continue;
                                else
                                {
                                    Console.WriteLine($"Failed to load config value: \"{pinf.Name}\", please check configuration");
                                    configLoaded = false;
                                }
                                break;
                        }
                    }
                }
            }

            return configLoaded;
        }
    }
}
