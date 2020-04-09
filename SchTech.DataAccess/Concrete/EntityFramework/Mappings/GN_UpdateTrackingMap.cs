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
            
            Property(x => x.IngestUUID).HasColumnName("IngestUUID");
            Property(x => x.GN_ProviderId).HasColumnName("GN_ProviderId");

            Property(x => x.Mapping_UpdateId).HasColumnName("Mapping_UpdateId");
            Property(x => x.Mapping_UpdateDate).HasColumnName("Mapping_UpdateDate");
            Property(x => x.Mapping_NextUpdateId).HasColumnName("Mapping_NextUpdateId");
            Property(x => x.Mapping_MaxUpdateId).HasColumnName("Mapping_MaxUpdateId");
            Property(x => x.Mapping_RootId).HasColumnName("Mapping_RootId");

            Property(x => x.Layer1_UpdateId).HasColumnName("Layer1_UpdateId");
            Property(x => x.Layer1_UpdateDate).HasColumnName("Layer1_UpdateDate");
            Property(x => x.Layer1_NextUpdateId).HasColumnName("Layer1_NextUpdateId");
            Property(x => x.Layer1_MaxUpdateId).HasColumnName("Layer1_MaxUpdateId");
            Property(x => x.Layer1_RootId).HasColumnName("Layer1_RootId");

            Property(x => x.Layer2_UpdateId).HasColumnName("Layer2_UpdateId");
            Property(x => x.Layer2_UpdateDate).HasColumnName("Layer2_NextUpdateId");
            Property(x => x.Layer2_NextUpdateId).HasColumnName("Layer2_UpdateDate");
            Property(x => x.Layer2_MaxUpdateId).HasColumnName("Layer2_MaxUpdateId");
            Property(x => x.Layer2_RootId).HasColumnName("Layer2_RootId");
            Property(x => x.UpdatesChecked).HasColumnName("UpdatesChecked");
            Property(x => x.RequiresEnrichment).HasColumnName("RequiresEnrichment");
        }
    }
}