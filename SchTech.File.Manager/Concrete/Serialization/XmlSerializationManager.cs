using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using ICSharpCode.SharpZipLib.Zip;

namespace SchTech.File.Manager.Concrete.Serialization
{
    public class XmlSerializationManager<T>
    {
        private readonly Type _type;
         

        public XmlSerializationManager()
        {
            _type = typeof(T);
        }

        public void Save(string path, object obj)
        {
            using (TextWriter textWriter = new StreamWriter(path))
            {
                var serializer = new XmlSerializer(_type);
                serializer.Serialize(textWriter, obj);
                textWriter.Close();
            }
        }

        public T Read(string fileContent)
        {
            T result;
            using (TextReader textReader = new StringReader(fileContent))
            {
                using (var reader = new XmlTextReader(textReader))
                {
                    reader.Namespaces = false;
                    var serializer = new XmlSerializer(_type);
                    result = (T)serializer.Deserialize(reader);
                }
            }

            return result;
        }

        public static string SerializedObjectToString<T>(T serializedObject)
        {
            var xmlSerializer = new XmlSerializer(serializedObject.GetType());

            using (var writer = new StringWriter())
            {
                xmlSerializer.Serialize(writer, serializedObject);
                return writer.ToString();
            }
        }
    }
}