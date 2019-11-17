using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SchTech.File.Manager.Concrete.Serialization
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ADI
    {

        private ADIMetadata metadataField;

        private ADIAsset assetField;

        /// <remarks/>
        public ADIMetadata Metadata
        {
            get
            {
                return this.metadataField;
            }
            set
            {
                this.metadataField = value;
            }
        }

        /// <remarks/>
        public ADIAsset Asset
        {
            get
            {
                return this.assetField;
            }
            set
            {
                this.assetField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ADIMetadata
    {

        private ADIMetadataAMS aMSField;

        private List<ADIMetadataApp_Data> app_DataField;

        /// <remarks/>
        public ADIMetadataAMS AMS
        {
            get
            {
                return this.aMSField;
            }
            set
            {
                this.aMSField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("App_Data")]
        public List<ADIMetadataApp_Data> App_Data
        {
            get
            {
                return this.app_DataField;
            }
            set
            {
                this.app_DataField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ADIMetadataAMS
    {

        private string asset_ClassField;

        private string asset_IDField;

        private string asset_NameField;

        private System.DateTime creation_DateField;

        private string descriptionField;

        private string productField;

        private string providerField;

        private string provider_IDField;

        private string verbField;

        private int version_MajorField;

        private int version_MinorField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Asset_Class
        {
            get
            {
                return this.asset_ClassField;
            }
            set
            {
                this.asset_ClassField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Asset_ID
        {
            get
            {
                return this.asset_IDField;
            }
            set
            {
                this.asset_IDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Asset_Name
        {
            get
            {
                return this.asset_NameField;
            }
            set
            {
                this.asset_NameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime Creation_Date
        {
            get
            {
                return this.creation_DateField;
            }
            set
            {
                this.creation_DateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Product
        {
            get
            {
                return this.productField;
            }
            set
            {
                this.productField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Provider
        {
            get
            {
                return this.providerField;
            }
            set
            {
                this.providerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Provider_ID
        {
            get
            {
                return this.provider_IDField;
            }
            set
            {
                this.provider_IDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Verb
        {
            get
            {
                return this.verbField;
            }
            set
            {
                this.verbField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Version_Major
        {
            get
            {
                return this.version_MajorField;
            }
            set
            {
                this.version_MajorField = Convert.ToInt32(value);
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Version_Minor
        {
            get
            {
                return this.version_MinorField;
            }
            set
            {
                this.version_MinorField = Convert.ToInt32(value);
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ADIMetadataApp_Data
    {

        private string appField;

        private string nameField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string App
        {
            get
            {
                return this.appField;
            }
            set
            {
                this.appField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ADIAsset
    {

        private ADIAssetMetadata metadataField;

        private List<ADIAssetAsset> assetField;

        /// <remarks/>
        public ADIAssetMetadata Metadata
        {
            get
            {
                return this.metadataField;
            }
            set
            {
                this.metadataField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Asset")]
        public List<ADIAssetAsset> Asset
        {
            get
            {
                return this.assetField;
            }
            set
            {
                this.assetField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ADIAssetMetadata
    {

        private ADIAssetMetadataAMS aMSField;

        private List<ADIAssetMetadataApp_Data> app_DataField;

        /// <remarks/>
        public ADIAssetMetadataAMS AMS
        {
            get
            {
                return this.aMSField;
            }
            set
            {
                this.aMSField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("App_Data")]
        public List<ADIAssetMetadataApp_Data> App_Data
        {
            get
            {
                return this.app_DataField;
            }
            set
            {
                this.app_DataField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ADIAssetMetadataAMS
    {

        private string asset_ClassField;

        private string asset_IDField;

        private string asset_NameField;

        private System.DateTime creation_DateField;

        private string descriptionField;

        private string productField;

        private string providerField;

        private string provider_IDField;

        private string verbField;

        private int version_MajorField;

        private int version_MinorField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Asset_Class
        {
            get
            {
                return this.asset_ClassField;
            }
            set
            {
                this.asset_ClassField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Asset_ID
        {
            get
            {
                return this.asset_IDField;
            }
            set
            {
                this.asset_IDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Asset_Name
        {
            get
            {
                return this.asset_NameField;
            }
            set
            {
                this.asset_NameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime Creation_Date
        {
            get
            {
                return this.creation_DateField;
            }
            set
            {
                this.creation_DateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Product
        {
            get
            {
                return this.productField;
            }
            set
            {
                this.productField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Provider
        {
            get
            {
                return this.providerField;
            }
            set
            {
                this.providerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Provider_ID
        {
            get
            {
                return this.provider_IDField;
            }
            set
            {
                this.provider_IDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Verb
        {
            get
            {
                return this.verbField;
            }
            set
            {
                this.verbField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Version_Major
        {
            get
            {
                return this.version_MajorField;
            }
            set
            {
                this.version_MajorField = Convert.ToInt32(value);
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Version_Minor
        {
            get
            {
                return this.version_MinorField;
            }
            set
            {
                this.version_MinorField = Convert.ToInt32(value);
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ADIAssetMetadataApp_Data
    {

        private string appField;

        private string nameField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string App
        {
            get
            {
                return this.appField;
            }
            set
            {
                this.appField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ADIAssetAsset
    {

        private ADIAssetAssetMetadata metadataField;

        private ADIAssetAssetContent contentField;

        /// <remarks/>
        public ADIAssetAssetMetadata Metadata
        {
            get
            {
                return this.metadataField;
            }
            set
            {
                this.metadataField = value;
            }
        }

        /// <remarks/>
        public ADIAssetAssetContent Content
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
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ADIAssetAssetMetadata
    {

        private ADIAssetAssetMetadataAMS aMSField;

        private List<ADIAssetAssetMetadataApp_Data> app_DataField;

        /// <remarks/>
        public ADIAssetAssetMetadataAMS AMS
        {
            get
            {
                return this.aMSField;
            }
            set
            {
                this.aMSField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("App_Data")]
        public List<ADIAssetAssetMetadataApp_Data> App_Data
        {
            get
            {
                return this.app_DataField;
            }
            set
            {
                this.app_DataField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ADIAssetAssetMetadataAMS
    {

        private string asset_ClassField;

        private string asset_IDField;

        private string asset_NameField;

        private int version_MinorField;

        private int version_MajorField;

        private System.DateTime creation_DateField;

        private string descriptionField;

        private string provider_IDField;

        private string providerField;

        private string productField;

        private string verbField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Asset_Class
        {
            get
            {
                return this.asset_ClassField;
            }
            set
            {
                this.asset_ClassField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Asset_ID
        {
            get
            {
                return this.asset_IDField;
            }
            set
            {
                this.asset_IDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Asset_Name
        {
            get
            {
                return this.asset_NameField;
            }
            set
            {
                this.asset_NameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Version_Minor
        {
            get
            {
                return this.version_MinorField;
            }
            set
            {
                this.version_MinorField = Convert.ToInt32(value);
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Version_Major
        {
            get
            {
                return this.version_MajorField;
            }
            set
            {
                this.version_MajorField = Convert.ToInt32(value);
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime Creation_Date
        {
            get
            {
                return this.creation_DateField;
            }
            set
            {
                this.creation_DateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Provider_ID
        {
            get
            {
                return this.provider_IDField;
            }
            set
            {
                this.provider_IDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Provider
        {
            get
            {
                return this.providerField;
            }
            set
            {
                this.providerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Product
        {
            get
            {
                return this.productField;
            }
            set
            {
                this.productField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Verb
        {
            get
            {
                return this.verbField;
            }
            set
            {
                this.verbField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ADIAssetAssetMetadataApp_Data
    {

        private string appField;

        private string nameField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string App
        {
            get
            {
                return this.appField;
            }
            set
            {
                this.appField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ADIAssetAssetContent
    {

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
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


}