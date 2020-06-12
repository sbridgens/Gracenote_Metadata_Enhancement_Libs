using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNMappingSchema;


namespace SchTech.Api.Manager.Serialization
{
    public class UpdateTrackerSerializationHelper<T>
    {
        private readonly Type _apiType;

        public UpdateTrackerSerializationHelper()
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

        public static string SerializedObjectToString<T>(T serializedObject, bool isMapping)
        {
            
            var xmlSerializer = new XmlSerializer(serializedObject.GetType());

            using (var writer = new StringWriter())
            {
                //remove incorrectly placed namespace
                var xsn = new XmlSerializerNamespaces();
                xsn.Add("", "");
                xmlSerializer.Serialize(writer, serializedObject, xsn);

                var doc = new XmlDocument();
                doc.LoadXml(writer.ToString());
                var newDoc = PrepareDbDocument(doc, isMapping);
                return newDoc.InnerXml;
            }
        }

        private static XmlDocument PrepareDbDocument(XmlDocument apiDocument, bool isMapping)
        {
            var returnDoc = new XmlDocument();
            XmlNode rootElement = returnDoc.CreateElement("on");
            XmlElement childElement = isMapping ? returnDoc.CreateElement("programMappings") : returnDoc.CreateElement("programs");

            returnDoc.AppendChild(rootElement);
            returnDoc.DocumentElement?.AppendChild(childElement);


            var xsn = new XmlSerializerNamespaces();
            xsn.Add("xmlns"," http://www.w3.org/2001/XMLSchema-instance");
            if (apiDocument.DocumentElement != null)
            {
                var importedNode = returnDoc.ImportNode(apiDocument.DocumentElement, true);
                childElement.AppendChild(importedNode);
            }

            return returnDoc;
        }
    }
}
