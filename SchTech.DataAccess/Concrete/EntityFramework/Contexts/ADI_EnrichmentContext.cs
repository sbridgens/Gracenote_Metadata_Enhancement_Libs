using Microsoft.EntityFrameworkCore;
using SchTech.Configuration.Manager.Parameters;
using SchTech.DataAccess.Concrete.EntityFramework.Mappings;
using SchTech.Entities.ConcreteTypes;
using System;
using System.Data.Entity;
using DbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace SchTech.DataAccess.Concrete.EntityFramework.Contexts
{
    public class ADI_EnrichmentContext : DbContext
    {
        private DbModelBuilder _modelBuilder;

        public ADI_EnrichmentContext()
        {
        }


        public ADI_EnrichmentContext(DbContextOptions<ADI_EnrichmentContext> options)
            : base(options)
        {
        }

        public virtual Microsoft.EntityFrameworkCore.DbSet<Adi_Data> Adi_Data { get; set; }
        public virtual Microsoft.EntityFrameworkCore.DbSet<GN_ImageLookup> GN_ImageLookup { get; set; }
        public virtual Microsoft.EntityFrameworkCore.DbSet<GN_Mapping_Data> GN_Mapping_Data { get; set; }
        public virtual Microsoft.EntityFrameworkCore.DbSet<CategoryMapping> CategoryMapping { get; set; }
        public virtual Microsoft.EntityFrameworkCore.DbSet<GnProgramTypeLookup> GnProgramTypeLookup { get; set; }
        public virtual Microsoft.EntityFrameworkCore.DbSet<MappingsUpdateTracking> MappingsUpdateTracking { get; set; }
        public virtual Microsoft.EntityFrameworkCore.DbSet<Layer1UpdateTracking> Layer1UpdateTracking { get; set; }
        public virtual Microsoft.EntityFrameworkCore.DbSet<Layer2UpdateTracking> Layer2UpdateTracking { get; set; }
        public virtual Microsoft.EntityFrameworkCore.DbSet<LatestUpdateIds> LatestUpdateIds { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
                return;

            optionsBuilder.UseSqlServer($"Server={DBConnectionProperties.DbServerOrIp},1433;" +
                                        $"Database={DBConnectionProperties.DatabaseName};" +
                                        $"Trusted_Connection={DBConnectionProperties.IntegratedSecurity};" +
                                        "MultipleActiveResultSets=True;",
                opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds));

            if (_modelBuilder != null)
                return;
            _modelBuilder = new DbModelBuilder();
            _modelBuilder.Configurations.Add(new AdiEnrichmentMap());
            _modelBuilder.Configurations.Add(new GnImageLookupMap());
            _modelBuilder.Configurations.Add(new GnMappingDataMap());
            _modelBuilder.Configurations.Add(new CategoryMappingMap());
            _modelBuilder.Configurations.Add(new GnProgramTypeLookupMap());
            _modelBuilder.Configurations.Add(new MappingsUpdateTrackingMap());
            _modelBuilder.Configurations.Add(new Layer1UpdateTrackingMap());
            _modelBuilder.Configurations.Add(new Layer2UpdateTrackingMap());
            _modelBuilder.Configurations.Add(new LatestUpdateIdsMap());
        }

        /// <summary>
        ///     SCH Tech added to allow update of entity based on property name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        public void UpdateEntity<T>(int id, string propertyName, string propertyValue) where T : class
        {
            var entity = Set<T>().Find(id);
            var prop = typeof(T).GetProperty(propertyName);
            if (prop != null)
            {
                var val = Convert.ChangeType(propertyValue, prop.PropertyType);
                prop.SetValue(entity, val);
            }

            SaveChanges();
        }

        /// <summary>
        ///     SCH Tech added to allow return of data based on property name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public string GetEntity<T>(int id, string propertyName) where T : class
        {
            var entity = Set<T>().Find(id);
            var prop = typeof(T).GetProperty(propertyName);

            return prop != null ? prop.GetValue(entity, null).ToString() : string.Empty;
        }
    }
}