using SchTech.Entities.ConcreteTypes;
using System.Data.Entity.ModelConfiguration;

namespace SchTech.DataAccess.Concrete.EntityFramework.Mappings
{
    public class GN_UpdateTrackingMap : EntityTypeConfiguration<GN_UpdateTracking>
    {
        public GN_UpdateTrackingMap()
        {
            //mapped to EntityTypeConfiguration Adi_data
            ToTable(@"GN_UpdateTracking", @"dbo");
            //Primary key is set here
            HasKey(x => x.Id);
            
            Property(x => x.TrackingIngestGuid).HasColumnName("IngestUUID");
            Property(x => x.TrackingGnProviderId).HasColumnName("GN_ProviderId");

            Property(x => x.MappingUpdateId).HasColumnName("Mapping_UpdateId");
            Property(x => x.MappingUpdateDate).HasColumnName("Mapping_UpdateDate");
            Property(x => x.MappingNextUpdateId).HasColumnName("Mapping_NextUpdateId");
            Property(x => x.MappingMaxUpdateId).HasColumnName("Mapping_MaxUpdateId");
            Property(x => x.MappingRootId).HasColumnName("Mapping_RootId");

            Property(x => x.Layer1UpdateId).HasColumnName("Layer1_UpdateId");
            Property(x => x.Layer1UpdateDate).HasColumnName("Layer1_UpdateDate");
            Property(x => x.Layer1NextUpdateId).HasColumnName("Layer1_NextUpdateId");
            Property(x => x.Layer1MaxUpdateId).HasColumnName("Layer1_MaxUpdateId");
            Property(x => x.Layer1RootId).HasColumnName("Layer1_RootId");

            Property(x => x.Layer2UpdateId).HasColumnName("Layer2_UpdateId");
            Property(x => x.Layer2NextUpdateId).HasColumnName("Layer2_NextUpdateId");
            Property(x => x.Layer2UpdateDate).HasColumnName("Layer2_UpdateDate");
            Property(x => x.Layer2MaxUpdateId).HasColumnName("Layer2_MaxUpdateId");
            Property(x => x.Layer2RootId).HasColumnName("Layer2_RootId");
            Property(x => x.UpdatesChecked).HasColumnName("UpdatesChecked");
            Property(x => x.RequiresEnrichment).HasColumnName("RequiresEnrichment");
        }
    }
}