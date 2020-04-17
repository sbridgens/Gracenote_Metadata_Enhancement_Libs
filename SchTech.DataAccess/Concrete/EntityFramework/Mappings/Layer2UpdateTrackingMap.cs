using SchTech.Entities.ConcreteTypes;
using System.Data.Entity.ModelConfiguration;

namespace SchTech.DataAccess.Concrete.EntityFramework.Mappings
{
    public class Layer2UpdateTrackingMap : EntityTypeConfiguration<Layer2UpdateTracking>
    {
        public Layer2UpdateTrackingMap()
        {
            //mapped to EntityTypeConfiguration Layer1UpdateTracking
            ToTable(@"Layer1UpdateTracking", @"dbo");
            //Primary key is set here
            HasKey(x => x.Id);
            
            Property(x => x.IngestUUID).HasColumnName("IngestUUID");
            Property(x => x.GN_Paid).HasColumnName("GN_Paid");
            Property(x => x.GN_connectorId).HasColumnName("GN_connectorId");
            Property(x => x.Layer2_UpdateId).HasColumnName("Layer2_UpdateId");
            Property(x => x.Layer2_UpdateDate).HasColumnName("Layer2_UpdateDate");
            Property(x => x.Layer2_NextUpdateId).HasColumnName("Layer2_NextUpdateId");
            Property(x => x.Layer2_MaxUpdateId).HasColumnName("Layer2_MaxUpdateId");
            Property(x => x.Layer2_RootId).HasColumnName("Layer2_RootId");
            Property(x => x.UpdatesChecked).HasColumnName("UpdatesChecked");
            Property(x => x.RequiresEnrichment).HasColumnName("RequiresEnrichment");
        }
    }
}
