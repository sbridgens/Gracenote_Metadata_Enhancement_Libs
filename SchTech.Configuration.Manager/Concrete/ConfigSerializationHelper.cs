using SchTech.Configuration.Manager.Schema.ADIWFE;
using System;
using System.Xml.Linq;

namespace SchTech.Configuration.Manager.Concrete
{
    public class ConfigSerializationHelper<T>
    {
        private readonly Type _type;

        public ConfigSerializationHelper()
        {
            _type = typeof(T);
        }

        public bool LoadConfigurationFile(string configFile)
        {
            var configLoaded = true;

            var xDoc = XDocument.Load(configFile);
            if (xDoc.Root == null)
                return false;
            var configs = xDoc.Elements(xDoc.Root.Name);
            var properties = _type.GetProperties();
            foreach (var ele in configs.Descendants())
            foreach (var pinf in properties)
                if (pinf.Name == ele.Name.LocalName)
                    switch (ele.Name.LocalName)
                    {
                        case "ProcessMappingFailures":
                            //Adi and Legacy only hence static ref
                            ADIWF_Config.ProcessMappingFailures = Convert.ToBoolean(ele.Value);
                            break;
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