using System.Collections.Generic;
using System.Xml.Serialization;
using SchTech.Core.Entities;

namespace SchTech.DataAccess.Concrete
{
    public class GN_ImageLookup : IEntity
    {
        public int Id { get; set; }
        public string Image_Lookup { get; set; }
        public string Image_Mapping { get; set; }
        public int Image_AdiOrder { get; set; }
        public string Mapping_Config { get; set; }
    }

    [XmlRoot(ElementName = "Aspect")]
    public class Aspect
    {
        [XmlElement(ElementName = "Aspect_Width")]
        public string AspectWidth { get; set; }
        [XmlElement(ElementName = "Aspect_Height")]
        public string AspectHeight { get; set; }
        [XmlElement(ElementName = "Resize_Height")]
        public string ResizeHeight { get; set; }
    }

    [XmlRoot(ElementName = "AllowedAspects")]
    public class AllowedAspects
    {
        [XmlElement(ElementName = "Aspect")]
        public List<Aspect> Aspect { get; set; }
    }

    [XmlRoot(ElementName = "Image_Category")]
    public class Image_Category
    {
        [XmlElement(ElementName = "Image_Tier")]
        public List<string> ImageTier { get; set; }
        [XmlElement(ElementName = "AllowedAspects")]
        public AllowedAspects AllowedAspects { get; set; }
        [XmlAttribute(AttributeName = "Category_Name")]
        public string CategoryName { get; set; }
        [XmlAttribute(AttributeName = "Priority_Order")]
        public string PriorityOrder { get; set; }
    }

    [XmlRoot(ElementName = "Image_Identifier")]
    public class Image_Identifier
    {
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
    }


    [XmlRoot(ElementName = "ImageMapping")]
    public class ImageMapping
    {
        [XmlElement(ElementName = "Image_Qualifier")]
        public string ImageQualifier { get; set; }
        [XmlElement(ElementName = "ProgramType")]
        public List<string> ProgramType { get; set; }
        [XmlElement(ElementName = "Image_Identifier")]
        public List<Image_Identifier> ImageIdentifier { get; set; }
        [XmlElement(ElementName = "Image_Category")]
        public List<Image_Category> ImageCategory { get; set; }
        [XmlElement(ElementName = "IsLandscape")]
        public string IsLandscape { get; set; }
    }
}