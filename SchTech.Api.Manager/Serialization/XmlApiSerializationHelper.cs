using System;
using System.IO;
using System.Xml.Serialization;

namespace SchTech.Api.Manager.Serialization
{
    public class XmlApiSerializationHelper<T>
    {
        public Type ApiType;

        public XmlApiSerializationHelper()
        {
            ApiType = typeof(T);
        }

        public T Read(string fileContent)
        {
            T result;
            using (TextReader textReader = new StringReader(fileContent))
            {
                var deserializer = new XmlSerializer(ApiType);
                result = (T)deserializer.Deserialize(textReader);
            }

            return result;
        }
    }
}