using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SchTech.Api.Manager.GracenoteOnApi.Concrete;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNMappingSchema;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using SchTech.Api.Manager.Serialization;
using SchTech.Business.Manager.Abstract.EntityFramework;
using SchTech.Business.Manager.Concrete.EntityFramework;
using SchTech.Configuration.Manager.Concrete;
using SchTech.Configuration.Manager.Schema.ADIWFE;
using SchTech.DataAccess.Abstract;
using SchTech.DataAccess.Concrete.EntityFramework;
using SchTech.File.Manager.Concrete.Serialization;
using SchTech.Web.Manager.Concrete;


namespace ADIWFE_TestClient
{
    class Program
    {
        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs rargs)
        {
            // Ignore missing resources
            if (rargs.Name.Contains(".resources"))
                return null;

            // check for assemblies already loaded
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.FullName == rargs.Name);
            if (assembly != null)
                return assembly;

            // Try to load by filename - split out the filename of the full assembly name
            // and append the base path of the original assembly (ie. look in the same dir)
            string filename = rargs.Name.Split(',')[0] + ".dll".ToLower();

            string asmFile = Path.Combine(@".\", "mslib", filename);

            try
            {
                return Assembly.LoadFrom(asmFile);
            }
            catch (Exception)
            {
                return null;
            }
        }
        

        static void Main(string[] args)
        {

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            AdiWfManager manager = new AdiWfManager();

            Thread thread = new Thread(manager.Workflow_Start);
            thread.Start(manager);
            Thread.Sleep(0);
            //TestAdiSerialization();
            //TestDBData();
            //TestApiCall();
            Console.ReadLine();
        }

        public static void TestApiCall()
        {
            var url =
                $"http://on-api.gracenote.com/v3/ProgramMappings?" +
                $"limit=10&providerId=VMMTITL0000000001778678&api_key={ADIWF_Config.ApiKey}";


            var webclient = new WebClientManager();
            var webData = webclient.HttpGetRequest(url);

            Console.WriteLine(webData);
            GraceNoteApiManager gnMappingData;
            gnMappingData = new GraceNoteApiManager();
            XmlApiSerializationHelper<GnOnApiProgramMappingSchema.@on> serializeData;
            serializeData = new XmlApiSerializationHelper<GnOnApiProgramMappingSchema.@on>();

            GnOnApiProgramMappingSchema.@on programData;
            programData = serializeData.Read(webData) ?? throw new ArgumentNullException("serializeData.Read(webData)");
            gnMappingData.GraceNoteMappingData = programData.programMappings
                                                            .programMapping
                                                            .FirstOrDefault(p => p.link.Any
                                                            (
                                                                i => i.Value == "VMMTITL0000000001778678" &&
                                                                     p.status.ToString().ToLower() == 
                                                                        GnOnApiProgramMappingSchema
                                                                            .onProgramMappingsProgramMappingStatus.
                                                                            Mapped.ToString().ToLower())
                                                            );

            if (gnMappingData.GraceNoteMappingData != null)
            {
                var TMSID = gnMappingData.GraceNoteMappingData.id.Where(t => t.type.Equals("TMSId"))
                    .Select(r => r.Value).FirstOrDefault();
                var ROOTID = gnMappingData.GraceNoteMappingData.id.Where(t => t.type.Equals("rootId"))
                    .Select(r => r.Value).FirstOrDefault();

                Console.WriteLine($"\r\n{TMSID}" +
                                  $"\r\n{ROOTID}");
            }
        }

        public static void TestAdiSerialization()
        {
            var ADI_FILE = new ADI();
            var xmlSerializer = new XmlSerializationManager<ADI>();
            ADI_FILE = xmlSerializer.Read(
                File.ReadAllText(
                    @"D:\Horizon4_TestDir\Input\EpisodeContainsPreview\ADI.xml"));

            xmlSerializer.Save(@"D:\Horizon4_TestDir\Input\EpisodeContainsPreview\ADITEST.xml", ADI_FILE);
        }

        public static void TestDBData()
        {
            var xmlSerializer = new ConfigSerializationHelper();
            xmlSerializer.LoadConfigurationFile(Properties.Settings.Default.XmlConfigFile);

            IAdiEnrichmentService adiEnrichment =
                new AdiEnrichmentManager(new EfAdiEnrichmentDal());


            var gnImageLookupService =
                new GnImageLookupManager(new EfGnImageLookupDal());

            IGnMappingDataService gnMappingDataService =
                new GnMappingDataManager(new EfGnMappingDataDal());


            Console.WriteLine("Updating Version Major");
            var adiRow = adiEnrichment.Get(p => p.TitlPaid== "TITL0000000001778678");

            adiRow.VersionMajor = 1;

            adiEnrichment.Update(adiRow);

            foreach (var adidata in adiEnrichment.GetList(p => p.TitlPaid=="TITL0000000001778678"))
            {
                Console.WriteLine($"{adidata.Id}\r\n" +
                                  $"{adidata.TitlPaid}\r\n" +
                                  $"{adidata.ContentTsFile}\r\n" +
                                  $"{adidata.ContentTsFileChecksum}\r\n" +
                                  $"{adidata.ContentTsFilePaid}\r\n" +
                                  $"{adidata.ContentTsFileSize}\r\n" +
                                  $"{adidata.Licensing_Window_End}\r\n" +
                                  $"{adidata.Enrichment_DateTime}\r\n");
            }

            Console.WriteLine("Loading Gnlookup Data\r\n");

            foreach (var mapping in gnMappingDataService.GetList(i => i.GN_Paid== "TITL0000000001778678"))
            {
                Console.WriteLine($"{mapping.Id}\r\n" +
                                  $"{mapping.GN_Paid}\r\n" +
                                  $"{mapping.GN_Availability_Start}\r\n" +
                                  $"{mapping.GN_Availability_End}\r\n" +
                                  $"{mapping.GN_EpisodeTitle}\r\n" +
                                  $"{mapping.GN_TMSID}\r\n");

            }

            Console.WriteLine("Loading GnMapping data\r\n");

            foreach (var lookup in gnImageLookupService.GetList())
            {
                Console.WriteLine($"{lookup.Id}\r\n" +
                                   $"{lookup.Image_AdiOrder}\r\n" +
                                   $"{lookup.Image_Lookup}\r\n" +
                                   $"{lookup.Image_Mapping}\r\n" +
                                   $"{lookup.Mapping_Config}\r\n");
            }
        }
    }
}
