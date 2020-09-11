using SchTech.Entities.ConcreteTypes;
using System.Data.Entity.ModelConfiguration;

namespace SchTech.DataAccess.Concrete.EntityFramework.Mappings
{
    public class Layer1UpdateTrackingMap : EntityTypeConfiguration<Layer1UpdateTracking>
    {
        public Layer1UpdateTrackingMap()
        {
            //mapped to EntityTypeConfiguration Layer1UpdateTracking
            ToTable(@"Layer1UpdateTracking", @"dbo");
            //Primary key is set here
            HasKey(x => x.Id);

            Property(x => x.IngestUUID).HasColumnName("IngestUUID");
            Property(x => x.GN_Paid).HasColumnName("GN_Paid");
            Property(x => x.GN_TMSID).HasColumnName("GN_TMSID");
            Property(x => x.Layer1_UpdateId).HasColumnName("Layer1_UpdateId");
            Property(x => x.Layer1_UpdateDate).HasColumnName("Layer1_UpdateDate");
            Property(x => x.Layer1_NextUpdateId).HasColumnName("Layer1_NextUpdateId");
            Property(x => x.Layer1_MaxUpdateId).HasColumnName("Layer1_MaxUpdateId");
            Property(x => x.Layer1_RootId).HasColumnName("Layer1_RootId");
            Property(x => x.UpdatesChecked).HasColumnName("UpdatesChecked");
            Property(x => x.RequiresEnrichment).HasColumnName("RequiresEnrichment");
        }
    }
}