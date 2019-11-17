namespace SchTech.Api.Manager.GracenoteOnApi.Schema.GNMappingSchema
{
    public class GnOnApiProgramMappingSchema
    {
        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public partial class on
        {

            private header headerField;

            private onProgramMappings programMappingsField;

            private decimal schemaVersionField;

            public on()
            {
                this.schemaVersionField = ((decimal)(3.0m));
            }

            /// <remarks/>
            public header header
            {
                get
                {
                    return this.headerField;
                }
                set
                {
                    this.headerField = value;
                }
            }

            /// <remarks/>
            public onProgramMappings programMappings
            {
                get
                {
                    return this.programMappingsField;
                }
                set
                {
                    this.programMappingsField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            [System.ComponentModel.DefaultValueAttribute(typeof(decimal), "3.0")]
            public decimal schemaVersion
            {
                get
                {
                    return this.schemaVersionField;
                }
                set
                {
                    this.schemaVersionField = value;
                }
            }
        }

        ///Version 3.0
        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public partial class header
        {

            private string contentField;

            private System.DateTime createdField;

            private string copyrightField;

            private headerRequestParameter[] requestParametersField;

            private headerStreamData streamDataField;

            /// <remarks/>
            public string content
            {
                get
                {
                    return this.contentField;
                }
                set
                {
                    this.contentField = value;
                }
            }

            /// <remarks/>
            public System.DateTime created
            {
                get
                {
                    return this.createdField;
                }
                set
                {
                    this.createdField = value;
                }
            }

            /// <remarks/>
            public string copyright
            {
                get
                {
                    return this.copyrightField;
                }
                set
                {
                    this.copyrightField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayItemAttribute("requestParameter", IsNullable = false)]
            public headerRequestParameter[] requestParameters
            {
                get
                {
                    return this.requestParametersField;
                }
                set
                {
                    this.requestParametersField = value;
                }
            }

            /// <remarks/>
            public headerStreamData streamData
            {
                get
                {
                    return this.streamDataField;
                }
                set
                {
                    this.streamDataField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class headerRequestParameter
        {

            private string nameField;

            private string valueField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string name
            {
                get
                {
                    return this.nameField;
                }
                set
                {
                    this.nameField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlTextAttribute()]
            public string Value
            {
                get
                {
                    return this.valueField;
                }
                set
                {
                    this.valueField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class headerStreamData
        {

            private long nextUpdateIdField;

            private bool nextUpdateIdFieldSpecified;

            private long maxUpdateIdField;

            private bool maxUpdateIdFieldSpecified;

            /// <remarks/>
            public long nextUpdateId
            {
                get
                {
                    return this.nextUpdateIdField;
                }
                set
                {
                    this.nextUpdateIdField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlIgnoreAttribute()]
            public bool nextUpdateIdSpecified
            {
                get
                {
                    return this.nextUpdateIdFieldSpecified;
                }
                set
                {
                    this.nextUpdateIdFieldSpecified = value;
                }
            }

            /// <remarks/>
            public long maxUpdateId
            {
                get
                {
                    return this.maxUpdateIdField;
                }
                set
                {
                    this.maxUpdateIdField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlIgnoreAttribute()]
            public bool maxUpdateIdSpecified
            {
                get
                {
                    return this.maxUpdateIdFieldSpecified;
                }
                set
                {
                    this.maxUpdateIdFieldSpecified = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public partial class requestParameters
        {

            private requestParametersRequestParameter[] requestParameterField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("requestParameter")]
            public requestParametersRequestParameter[] requestParameter
            {
                get
                {
                    return this.requestParameterField;
                }
                set
                {
                    this.requestParameterField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class requestParametersRequestParameter
        {

            private string nameField;

            private string valueField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string name
            {
                get
                {
                    return this.nameField;
                }
                set
                {
                    this.nameField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlTextAttribute()]
            public string Value
            {
                get
                {
                    return this.valueField;
                }
                set
                {
                    this.valueField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class onProgramMappings
        {

            private onProgramMappingsProgramMapping[] programMappingField;

            private string typeField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("programMapping")]
            public onProgramMappingsProgramMapping[] programMapping
            {
                get
                {
                    return this.programMappingField;
                }
                set
                {
                    this.programMappingField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string type
            {
                get
                {
                    return this.typeField;
                }
                set
                {
                    this.typeField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class onProgramMappingsProgramMapping
        {

            private string initialMappingDateField;

            private string creationDateField;

            private bool creationDateFieldSpecified;

            private onProgramMappingsProgramMappingStatus statusField;

            private bool statusFieldSpecified;

            private onProgramMappingsProgramMappingID[] idField;

            private onProgramMappingsProgramMappingLink[] linkField;

            private onProgramMappingsProgramMappingAvailability availabilityField;

            private onProgramMappingsProgramMappingMessage messageField;

            private string catalogNameField;

            private string programMappingIdField;

            private string updateIdField;

            private System.DateTime updateDateField;

            private bool deletedField;

            private bool deletedFieldSpecified;

            /// <remarks/>
            public string initialMappingDate
            {
                get
                {
                    return this.initialMappingDateField;
                }
                set
                {
                    this.initialMappingDateField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
            public string creationDate
            {
                get
                {
                    return this.creationDateField;
                }
                set
                {
                    this.creationDateField = value;
                }
            }


            /// <remarks/>
            [System.Xml.Serialization.XmlIgnoreAttribute()]
            public bool creationDateSpecified
            {
                get
                {
                    return this.creationDateFieldSpecified;
                }
                set
                {
                    this.creationDateFieldSpecified = value;
                }
            }

            /// <remarks/>
            public onProgramMappingsProgramMappingStatus status
            {
                get
                {
                    return this.statusField;
                }
                set
                {
                    this.statusField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlIgnoreAttribute()]
            public bool statusSpecified
            {
                get
                {
                    return this.statusFieldSpecified;
                }
                set
                {
                    this.statusFieldSpecified = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("id")]
            public onProgramMappingsProgramMappingID[] id
            {
                get
                {
                    return this.idField;
                }
                set
                {
                    this.idField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("link")]
            public onProgramMappingsProgramMappingLink[] link
            {
                get
                {
                    return this.linkField;
                }
                set
                {
                    this.linkField = value;
                }
            }

            /// <remarks/>
            public onProgramMappingsProgramMappingAvailability availability
            {
                get
                {
                    return this.availabilityField;
                }
                set
                {
                    this.availabilityField = value;
                }
            }

            /// <remarks/>
            public onProgramMappingsProgramMappingMessage message
            {
                get
                {
                    return this.messageField;
                }
                set
                {
                    this.messageField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
            public string catalogName
            {
                get
                {
                    return this.catalogNameField;
                }
                set
                {
                    this.catalogNameField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string programMappingId
            {
                get
                {
                    return this.programMappingIdField;
                }
                set
                {
                    this.programMappingIdField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
            public string updateId
            {
                get
                {
                    return this.updateIdField;
                }
                set
                {
                    this.updateIdField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public System.DateTime updateDate
            {
                get
                {
                    return this.updateDateField;
                }
                set
                {
                    this.updateDateField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public bool deleted
            {
                get
                {
                    return this.deletedField;
                }
                set
                {
                    this.deletedField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlIgnoreAttribute()]
            public bool deletedSpecified
            {
                get
                {
                    return this.deletedFieldSpecified;
                }
                set
                {
                    this.deletedFieldSpecified = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
        [System.SerializableAttribute()]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public enum onProgramMappingsProgramMappingStatus
        {

            /// <remarks/>
            ToBeMapped,

            /// <remarks/>
            Mapped,

            /// <remarks/>
            Unmappable,
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class onProgramMappingsProgramMappingID
        {

            private string typeField;

            private string valueField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(DataType = "token")]
            public string type
            {
                get
                {
                    return this.typeField;
                }
                set
                {
                    this.typeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlTextAttribute(DataType = "token")]
            public string Value
            {
                get
                {
                    return this.valueField;
                }
                set
                {
                    this.valueField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class onProgramMappingsProgramMappingLink
        {

            private string idTypeField;

            private string valueField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(DataType = "token")]
            public string idType
            {
                get
                {
                    return this.idTypeField;
                }
                set
                {
                    this.idTypeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlTextAttribute()]
            public string Value
            {
                get
                {
                    return this.valueField;
                }
                set
                {
                    this.valueField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class onProgramMappingsProgramMappingAvailability
        {

            private System.DateTime startField;

            private bool startFieldSpecified;

            private System.DateTime endField;

            private bool endFieldSpecified;

            /// <remarks/>
            public System.DateTime start
            {
                get
                {
                    return this.startField;
                }
                set
                {
                    this.startField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlIgnoreAttribute()]
            public bool startSpecified
            {
                get
                {
                    return this.startFieldSpecified;
                }
                set
                {
                    this.startFieldSpecified = value;
                }
            }

            /// <remarks/>
            public System.DateTime end
            {
                get
                {
                    return this.endField;
                }
                set
                {
                    this.endField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlIgnoreAttribute()]
            public bool endSpecified
            {
                get
                {
                    return this.endFieldSpecified;
                }
                set
                {
                    this.endFieldSpecified = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class onProgramMappingsProgramMappingMessage
        {

            private string[] detailField;

            private string reasonField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("detail", DataType = "token")]
            public string[] detail
            {
                get
                {
                    return this.detailField;
                }
                set
                {
                    this.detailField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string reason
            {
                get
                {
                    return this.reasonField;
                }
                set
                {
                    this.reasonField = value;
                }
            }
        }
    }
}
