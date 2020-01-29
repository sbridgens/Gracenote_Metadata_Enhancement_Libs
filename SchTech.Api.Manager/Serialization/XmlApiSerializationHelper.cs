using System;
using System.IO;
using System.Xml.Serialization;

namespace SchTech.Api.Manager.Serialization
{
    public class XmlApiSerializationHelper<T>
    {
        private readonly Type _apiType;

        public XmlApiSerializationHelper()
        {
            _apiType = typeof(T);
        }

        public T Read(string fileContent)
        {
            T result;
            using (TextReader textReader = new StringReader(fileContent))
            {
                var deserializer = new XmlSerializer(_apiType);
                result = (T)deserializer.Deserialize(textReader);
            }

            return result;
        }
    }
}