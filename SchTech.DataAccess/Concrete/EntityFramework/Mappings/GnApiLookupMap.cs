using SchTech.Entities.ConcreteTypes;
using System.Data.Entity.ModelConfiguration;

namespace SchTech.DataAccess.Concrete.EntityFramework.Mappings
{
    public class GnApiLookupMap : EntityTypeConfiguration<GN_Api_Lookup>
    {
        public GnApiLookupMap()
        {
            //mapped to EntityTypeConfiguration Adi_data
            ToTable(@"GN_Api_Lookup", @"dbo");
            //Primary key is set here
            HasKey(x => x.IngestUUID);

            //EntityTypeConfiguration properties are now mapped
            //to the db tables matching the property names.
            Property(x => x.Id).HasColumnName("id");
            Property(x => x.GN_TMSID).HasColumnName("GN_TMSID");
            Property(x => x.GN_connectorId).HasColumnName("GN_connectorId");
            Property(x => x.GnMapData).HasColumnName("GnMapData");
            Property(x => x.GnLayer1Data).HasColumnName("GnLayer1Data");
            Property(x => x.GnLayer2Data).HasColumnName("GnLayer2Data");
        }
    }
}
