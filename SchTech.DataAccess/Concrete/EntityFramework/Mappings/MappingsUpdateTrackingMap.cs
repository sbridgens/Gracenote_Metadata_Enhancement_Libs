using SchTech.Entities.ConcreteTypes;
using System.Data.Entity.ModelConfiguration;

namespace SchTech.DataAccess.Concrete.EntityFramework.Mappings
{
    public class MappingsUpdateTrackingMap : EntityTypeConfiguration<MappingsUpdateTracking>
    {
        public MappingsUpdateTrackingMap()
        {
            //mapped to EntityTypeConfiguration MappingsUpdateTracking
            ToTable(@"MappingsUpdateTracking", @"dbo");
            //Primary key is set here
            HasKey(x => x.Id);

            Property(x => x.IngestUUID).HasColumnName("IngestUUID");
            Property(x => x.GN_ProviderId).HasColumnName("GN_ProviderId");
            Property(x => x.Mapping_UpdateId).HasColumnName("Mapping_UpdateId");
            Property(x => x.Mapping_UpdateDate).HasColumnName("Mapping_UpdateDate");
            Property(x => x.Mapping_NextUpdateId).HasColumnName("Mapping_NextUpdateId");
            Property(x => x.Mapping_MaxUpdateId).HasColumnName("Mapping_MaxUpdateId");
            Property(x => x.Mapping_RootId).HasColumnName("Mapping_RootId");
            Property(x => x.UpdatesChecked).HasColumnName("UpdatesChecked");
            Property(x => x.RequiresEnrichment).HasColumnName("RequiresEnrichment");
        }
    }
}