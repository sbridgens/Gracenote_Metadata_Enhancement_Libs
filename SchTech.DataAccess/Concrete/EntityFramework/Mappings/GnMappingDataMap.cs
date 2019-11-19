using System.Data.Entity.ModelConfiguration;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Concrete.EntityFramework.Mappings
{
    public class GnMappingDataMap : EntityTypeConfiguration<GN_Mapping_Data>
    {
        public GnMappingDataMap()
        {
            //mapped to EntityTypeConfiguration Adi_data
            ToTable(@"GN_Mapping_Data", @"dbo");
            //Primary key is set here
            HasKey(x => x.Id);


            //EntityTypeConfiguration properties are now mapped
            //to the db tables matching the property names.
            Property(x => x.GN_Availability_End).HasColumnName("GN_Availability_End");
            Property(x => x.GN_Availability_Start).HasColumnName("GN_Availability_Start");
            Property(x => x.GN_connectorId).HasColumnName("GN_connectorId");
            Property(x => x.GN_creationDate).HasColumnName("GN_creationDate");
            Property(x => x.GN_EpisodeNumber).HasColumnName("GN_EpisodeNumber");
            Property(x => x.GN_EpisodeTitle).HasColumnName("GN_EpisodeTitle");
            Property(x => x.GN_Images).HasColumnName("GN_Images");
            Property(x => x.GN_Paid).HasColumnName("GN_Paid");
            Property(x => x.GN_Pid).HasColumnName("GN_Pid");
            Property(x => x.GN_programMappingId).HasColumnName("GN_programMappingId");
            Property(x => x.GN_ProviderId).HasColumnName("GN_ProviderId");
            Property(x => x.GN_RootID).HasColumnName("GN_RootID");
            Property(x => x.GN_SeasonId).HasColumnName("GN_SeasonId");
            Property(x => x.GN_SeasonNumber).HasColumnName("GN_SeasonNumber");
            Property(x => x.GN_SeriesId).HasColumnName("GN_SeriesId");
            Property(x => x.GN_SeriesTitle).HasColumnName("GN_SeriesTitle");
            Property(x => x.GN_Status).HasColumnName("GN_Status");
            Property(x => x.GN_TMSID).HasColumnName("GN_TMSID");
            Property(x => x.GN_updateId).HasColumnName("GN_updateId");
        }
    }
}