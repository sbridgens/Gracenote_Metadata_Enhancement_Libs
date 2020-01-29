using SchTech.Configuration.Manager.Schema.ADIWFE;
using System;
using System.Xml.Linq;

namespace SchTech.Configuration.Manager.Concrete
{
    public class ConfigSerializationHelper
    {
        public bool LoadConfigurationFile(string configFile)
        {
            var configLoaded = true;

            var xDoc = XDocument.Load(configFile);
            var configs = xDoc.Elements(xDoc.Root.Name);
            var properties = typeof(ADIWF_Config).GetProperties();
            foreach (var ele in configs.Descendants())
                foreach (var pinf in properties)
                    if (pinf.Name == ele.Name.LocalName)
                        switch (ele.Name.LocalName)
                        {
                            //case "ExtraDataConfig":
                            //    ExtraDataConfig.Providers = ele.Attribute("providers").Value;
                            //    ExtraDataConfig.Extradata1 = ele.Attribute("extradata1").Value;
                            //    ExtraDataConfig.Extradata3 = ele.Attribute("extradata3").Value;

                            //    B_IsRunning = true;
                            //    break;
                            case "Block_Platform":
                                Block_Platform.Providers = ele.Attribute("providers")?.Value;
                                Block_Platform.BlockPlatformValue = ele.Attribute("BlockPlatformValue")?.Value;
                                break;
                            case "LegacyGoAllowedProviders":
                                LegacyGoAllowedProviders.GoProviders = ele.Attribute("GoProviders")?.Value;
                                LegacyGoAllowedProviders.MoveNonLegacyToDirectory =
                                    ele.Attribute("MoveNonLegacyToDirectory")?.Value;
                                break;
                            default:
                                pinf.SetValue(pinf.Name, ele.Value);

                                if (pinf.GetValue(null, null) != null)
                                {
                                }
                                else
                                {
                                    Console.WriteLine(
                                        $"Failed to load config value: \"{pinf.Name}\", please check configuration");
                                    configLoaded = false;
                                }

                                break;
                        }

            return configLoaded;
        }
    }
}