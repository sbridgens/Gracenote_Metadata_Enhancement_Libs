using SchTech.Entities.ConcreteTypes;
using System.Data.Entity.ModelConfiguration;

namespace SchTech.DataAccess.Concrete.EntityFramework.Mappings
{
    public class AdiEnrichmentMap : EntityTypeConfiguration<Adi_Data>
    {
        public AdiEnrichmentMap()
        {
            //mapped to EntityTypeConfiguration Adi_data
            ToTable(@"Adi_Data", @"dbo");
            //Primary key is set here
            HasKey(x => x.Id);

            //EntityTypeConfiguration properties are now mapped
            //to the db tables matching the property names.
            Property(x => x.IngestUUID).HasColumnName("IngestUUID");
            Property(x => x.TitlPaid).HasColumnName("TITLPAID");
            Property(x => x.OriginalAdi).HasColumnName("OriginalADI");
            Property(x => x.VersionMajor).HasColumnName("VersionMajor");
            Property(x => x.VersionMinor).HasColumnName("VersionMinor");
            Property(x => x.ProviderId).HasColumnName("ProviderId");
            Property(x => x.TmsId).HasColumnName("TMSID");
            Property(x => x.ProcessedDateTime).HasColumnName("ProcessedDateTime");
            Property(x => x.ContentTsFile).HasColumnName("ContentTSFile");
            Property(x => x.ContentTsFilePaid).HasColumnName("ContentTsFilePaid");
            Property(x => x.ContentTsFileChecksum).HasColumnName("ContentTSFileChecksum");
            Property(x => x.ContentTsFileSize).HasColumnName("ContentTSFileSize");
            Property(x => x.PreviewFile).HasColumnName("PreviewFile");
            Property(x => x.PreviewFilePaid).HasColumnName("PreviewFilePaid");
            Property(x => x.PreviewFileChecksum).HasColumnName("PreviewFileChecksum");
            Property(x => x.PreviewFileSize).HasColumnName("PreviewFileSize");
            Property(x => x.EnrichedAdi).HasColumnName("EnrichedADI");
            Property(x => x.Enrichment_DateTime).HasColumnName("Enrichment_DateTime");
            Property(x => x.UpdateAdi).HasColumnName("UpdateAdi");
            Property(x => x.Update_DateTime).HasColumnName("Update_DateTime");
            Property(x => x.Licensing_Window_End).HasColumnName("Licensing_Window_End");
        }
    }
}