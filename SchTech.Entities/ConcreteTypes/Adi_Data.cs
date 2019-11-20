using SchTech.Core.Entities;
using System;

namespace SchTech.Entities.ConcreteTypes
{
    public class Adi_Data : IEntity
    {
        public int Id { get; set; }
        public string TitlPaid { get; set; }
        public string OriginalAdi { get; set; }
        public int? VersionMajor { get; set; }
        public int? VersionMinor { get; set; }
        public string ProviderId { get; set; }
        public string TmsId { get; set; }
        public DateTime? ProcessedDateTime { get; set; }
        public string ContentTsFile { get; set; }
        public string ContentTsFilePaid { get; set; }
        public string ContentTsFileChecksum { get; set; }
        public string ContentTsFileSize { get; set; }
        public string PreviewFile { get; set; }
        public string PreviewFilePaid { get; set; }
        public string PreviewFileChecksum { get; set; }
        public string PreviewFileSize { get; set; }
        public string EnrichedAdi { get; set; }
        public DateTime? Enrichment_DateTime { get; set; }
        public string UpdateAdi { get; set; }
        public DateTime? Update_DateTime { get; set; }
        public string Licensing_Window_End { get; set; }
    }
}