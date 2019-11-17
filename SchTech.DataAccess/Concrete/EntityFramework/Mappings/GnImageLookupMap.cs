using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Concrete.EntityFramework.Mappings
{
    public class GnImageLookupMap : EntityTypeConfiguration<GN_ImageLookup>
    {
        public GnImageLookupMap()
        {
            //mapped to EntityTypeConfiguration Adi_data
            ToTable(@"GN_ImageLookup", @"dbo");
            //Primary key is set here
            HasKey(x => x.Id);

            
            //EntityTypeConfiguration properties are now mapped
            //to the db tables matching the property names.
            Property(x => x.Image_Lookup).HasColumnName("Image_Lookup");
            Property(x => x.Image_Mapping).HasColumnName("Image_Mapping");
            Property(x => x.Image_AdiOrder).HasColumnName("Image_AdiOrder");
            Property(x => x.Mapping_Config).HasColumnName("Mapping_Config");
        }
    }
}
