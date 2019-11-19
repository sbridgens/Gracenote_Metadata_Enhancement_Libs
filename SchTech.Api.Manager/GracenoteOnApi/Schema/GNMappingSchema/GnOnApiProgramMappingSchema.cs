using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace SchTech.Api.Manager.GracenoteOnApi.Schema.GNMappingSchema
{
    public class GnOnApiProgramMappingSchema
    {
        /// <remarks />
        [GeneratedCode("xsd", "4.6.1590.0")]
        [Serializable]
        [XmlType(AnonymousType = true)]
        public enum onProgramMappingsProgramMappingStatus
        {
            /// <remarks />
            ToBeMapped,

            /// <remarks />
            Mapped,

            /// <remarks />
            Unmappable
        }

        /// <remarks />
        [GeneratedCode("xsd", "4.6.1590.0")]
        [Serializable]
        [DebuggerStepThrough]
        [DesignerCategory("code")]
        [XmlType(AnonymousType = true)]
        [XmlRoot(Namespace = "", IsNullable = false)]
        public class on
        {
            private header headerField;

            private onProgramMappings programMappingsField;

            private decimal schemaVersionField;

            public on()
            {
                schemaVersionField = 3.0m;
            }

            /// <remarks />
            public header header
            {
                get => headerField;
                set => headerField = value;
            }

            /// <remarks />
            public onProgramMappings programMappings
            {
                get => programMappingsField;
                set => programMappingsField = value;
            }

            /// <remarks />
            [XmlAttribute]
            [DefaultValue(typeof(decimal), "3.0")]
            public decimal schemaVersion
            {
                get => schemaVersionField;
                set => schemaVersionField = value;
            }
        }

        /// Version 3.0
        /// <remarks />
        [GeneratedCode("xsd", "4.6.1590.0")]
        [Serializable]
        [DebuggerStepThrough]
        [DesignerCategory("code")]
        [XmlType(AnonymousType = true)]
        [XmlRoot(Namespace = "", IsNullable = false)]
        public class header
        {
            private string contentField;

            private string copyrightField;

            private DateTime createdField;

            private headerRequestParameter[] requestParametersField;

            private headerStreamData streamDataField;

            /// <remarks />
            public string content
            {
                get => contentField;
                set => contentField = value;
            }

            /// <remarks />
            public DateTime created
            {
                get => createdField;
                set => createdField = value;
            }

            /// <remarks />
            public string copyright
            {
                get => copyrightField;
                set => copyrightField = value;
            }

            /// <remarks />
            [XmlArrayItem("requestParameter", IsNullable = false)]
            public headerRequestParameter[] requestParameters
            {
                get => requestParametersField;
                set => requestParametersField = value;
            }

            /// <remarks />
            public headerStreamData streamData
            {
                get => streamDataField;
                set => streamDataField = value;
            }
        }

        /// <remarks />
        [GeneratedCode("xsd", "4.6.1590.0")]
        [Serializable]
        [DebuggerStepThrough]
        [DesignerCategory("code")]
        [XmlType(AnonymousType = true)]
        public class headerRequestParameter
        {
            private string nameField;

            private string valueField;

            /// <remarks />
            [XmlAttribute]
            public string name
            {
                get => nameField;
                set => nameField = value;
            }

            /// <remarks />
            [XmlText]
            public string Value
            {
                get => valueField;
                set => valueField = value;
            }
        }

        /// <remarks />
        [GeneratedCode("xsd", "4.6.1590.0")]
        [Serializable]
        [DebuggerStepThrough]
        [DesignerCategory("code")]
        [XmlType(AnonymousType = true)]
        public class headerStreamData
        {
            private long maxUpdateIdField;

            private bool maxUpdateIdFieldSpecified;

            private long nextUpdateIdField;

            private bool nextUpdateIdFieldSpecified;

            /// <remarks />
            public long nextUpdateId
            {
                get => nextUpdateIdField;
                set => nextUpdateIdField = value;
            }

            /// <remarks />
            [XmlIgnore]
            public bool nextUpdateIdSpecified
            {
                get => nextUpdateIdFieldSpecified;
                set => nextUpdateIdFieldSpecified = value;
            }

            /// <remarks />
            public long maxUpdateId
            {
                get => maxUpdateIdField;
                set => maxUpdateIdField = value;
            }

            /// <remarks />
            [XmlIgnore]
            public bool maxUpdateIdSpecified
            {
                get => maxUpdateIdFieldSpecified;
                set => maxUpdateIdFieldSpecified = value;
            }
        }

        /// <remarks />
        [GeneratedCode("xsd", "4.6.1590.0")]
        [Serializable]
        [DebuggerStepThrough]
        [DesignerCategory("code")]
        [XmlType(AnonymousType = true)]
        [XmlRoot(Namespace = "", IsNullable = false)]
        public class requestParameters
        {
            private requestParametersRequestParameter[] requestParameterField;

            /// <remarks />
            [XmlElement("requestParameter")]
            public requestParametersRequestParameter[] requestParameter
            {
                get => requestParameterField;
                set => requestParameterField = value;
            }
        }

        /// <remarks />
        [GeneratedCode("xsd", "4.6.1590.0")]
        [Serializable]
        [DebuggerStepThrough]
        [DesignerCategory("code")]
        [XmlType(AnonymousType = true)]
        public class requestParametersRequestParameter
        {
            private string nameField;

            private string valueField;

            /// <remarks />
            [XmlAttribute]
            public string name
            {
                get => nameField;
                set => nameField = value;
            }

            /// <remarks />
            [XmlText]
            public string Value
            {
                get => valueField;
                set => valueField = value;
            }
        }

        /// <remarks />
        [GeneratedCode("xsd", "4.6.1590.0")]
        [Serializable]
        [DebuggerStepThrough]
        [DesignerCategory("code")]
        [XmlType(AnonymousType = true)]
        public class onProgramMappings
        {
            private onProgramMappingsProgramMapping[] programMappingField;

            private string typeField;

            /// <remarks />
            [XmlElement("programMapping")]
            public onProgramMappingsProgramMapping[] programMapping
            {
                get => programMappingField;
                set => programMappingField = value;
            }

            /// <remarks />
            [XmlAttribute]
            public string type
            {
                get => typeField;
                set => typeField = value;
            }
        }

        /// <remarks />
        [GeneratedCode("xsd", "4.6.1590.0")]
        [Serializable]
        [DebuggerStepThrough]
        [DesignerCategory("code")]
        [XmlType(AnonymousType = true)]
        public class onProgramMappingsProgramMapping
        {
            private onProgramMappingsProgramMappingAvailability availabilityField;

            private string catalogNameField;

            private string creationDateField;

            private bool creationDateFieldSpecified;

            private bool deletedField;

            private bool deletedFieldSpecified;

            private onProgramMappingsProgramMappingID[] idField;

            private string initialMappingDateField;

            private onProgramMappingsProgramMappingLink[] linkField;

            private onProgramMappingsProgramMappingMessage messageField;

            private string programMappingIdField;

            private onProgramMappingsProgramMappingStatus statusField;

            private bool statusFieldSpecified;

            private DateTime updateDateField;

            private string updateIdField;

            /// <remarks />
            public string initialMappingDate
            {
                get => initialMappingDateField;
                set => initialMappingDateField = value;
            }

            /// <remarks />
            [XmlElement(DataType = "token")]
            public string creationDate
            {
                get => creationDateField;
                set => creationDateField = value;
            }


            /// <remarks />
            [XmlIgnore]
            public bool creationDateSpecified
            {
                get => creationDateFieldSpecified;
                set => creationDateFieldSpecified = value;
            }

            /// <remarks />
            public onProgramMappingsProgramMappingStatus status
            {
                get => statusField;
                set => statusField = value;
            }

            /// <remarks />
            [XmlIgnore]
            public bool statusSpecified
            {
                get => statusFieldSpecified;
                set => statusFieldSpecified = value;
            }

            /// <remarks />
            [XmlElement("id")]
            public onProgramMappingsProgramMappingID[] id
            {
                get => idField;
                set => idField = value;
            }

            /// <remarks />
            [XmlElement("link")]
            public onProgramMappingsProgramMappingLink[] link
            {
                get => linkField;
                set => linkField = value;
            }

            /// <remarks />
            public onProgramMappingsProgramMappingAvailability availability
            {
                get => availabilityField;
                set => availabilityField = value;
            }

            /// <remarks />
            public onProgramMappingsProgramMappingMessage message
            {
                get => messageField;
                set => messageField = value;
            }

            /// <remarks />
            [XmlElement(DataType = "token")]
            public string catalogName
            {
                get => catalogNameField;
                set => catalogNameField = value;
            }

            /// <remarks />
            [XmlAttribute]
            public string programMappingId
            {
                get => programMappingIdField;
                set => programMappingIdField = value;
            }

            /// <remarks />
            [XmlAttribute(DataType = "nonNegativeInteger")]
            public string updateId
            {
                get => updateIdField;
                set => updateIdField = value;
            }

            /// <remarks />
            [XmlAttribute]
            public DateTime updateDate
            {
                get => updateDateField;
                set => updateDateField = value;
            }

            /// <remarks />
            [XmlAttribute]
            public bool deleted
            {
                get => deletedField;
                set => deletedField = value;
            }

            /// <remarks />
            [XmlIgnore]
            public bool deletedSpecified
            {
                get => deletedFieldSpecified;
                set => deletedFieldSpecified = value;
            }
        }

        /// <remarks />
        [GeneratedCode("xsd", "4.6.1590.0")]
        [Serializable]
        [DebuggerStepThrough]
        [DesignerCategory("code")]
        [XmlType(AnonymousType = true)]
        public class onProgramMappingsProgramMappingID
        {
            private string typeField;

            private string valueField;

            /// <remarks />
            [XmlAttribute(DataType = "token")]
            public string type
            {
                get => typeField;
                set => typeField = value;
            }

            /// <remarks />
            [XmlText(DataType = "token")]
            public string Value
            {
                get => valueField;
                set => valueField = value;
            }
        }

        /// <remarks />
        [GeneratedCode("xsd", "4.6.1590.0")]
        [Serializable]
        [DebuggerStepThrough]
        [DesignerCategory("code")]
        [XmlType(AnonymousType = true)]
        public class onProgramMappingsProgramMappingLink
        {
            private string idTypeField;

            private string valueField;

            /// <remarks />
            [XmlAttribute(DataType = "token")]
            public string idType
            {
                get => idTypeField;
                set => idTypeField = value;
            }

            /// <remarks />
            [XmlText]
            public string Value
            {
                get => valueField;
                set => valueField = value;
            }
        }

        /// <remarks />
        [GeneratedCode("xsd", "4.6.1590.0")]
        [Serializable]
        [DebuggerStepThrough]
        [DesignerCategory("code")]
        [XmlType(AnonymousType = true)]
        public class onProgramMappingsProgramMappingAvailability
        {
            private DateTime endField;

            private bool endFieldSpecified;

            private DateTime startField;

            private bool startFieldSpecified;

            /// <remarks />
            public DateTime start
            {
                get => startField;
                set => startField = value;
            }

            /// <remarks />
            [XmlIgnore]
            public bool startSpecified
            {
                get => startFieldSpecified;
                set => startFieldSpecified = value;
            }

            /// <remarks />
            public DateTime end
            {
                get => endField;
                set => endField = value;
            }

            /// <remarks />
            [XmlIgnore]
            public bool endSpecified
            {
                get => endFieldSpecified;
                set => endFieldSpecified = value;
            }
        }

        /// <remarks />
        [GeneratedCode("xsd", "4.6.1590.0")]
        [Serializable]
        [DebuggerStepThrough]
        [DesignerCategory("code")]
        [XmlType(AnonymousType = true)]
        public class onProgramMappingsProgramMappingMessage
        {
            private string[] detailField;

            private string reasonField;

            /// <remarks />
            [XmlElement("detail", DataType = "token")]
            public string[] detail
            {
                get => detailField;
                set => detailField = value;
            }

            /// <remarks />
            [XmlAttribute]
            public string reason
            {
                get => reasonField;
                set => reasonField = value;
            }
        }
    }
}