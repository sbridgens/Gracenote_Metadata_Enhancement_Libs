using log4net;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using SchTech.Configuration.Manager.Schema.ADIWFE;
using SchTech.DataAccess.Concrete.EntityFramework;

namespace ADIWFE_TestLegacyGo
{
    public class AdiWfManager
    {
        /// <summary>
        ///     Initialize Log4net
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(AdiWfManager));

        public AdiWfManager()
        {
            AdiWfOperations = new AdiWfOperations();
        }

        private AdiWfOperations AdiWfOperations { get; }

        public static bool IsRunning { get; set; }


        /// <summary>
        ///     Function to resolve application assemblies that are contained in sub directories.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Ignore missing resources
            if (args.Name.Contains(".resources"))
                return null;

            // check for assemblies already loaded
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (assembly != null)
                return assembly;

            // Try to load by filename - split out the filename of the full assembly name
            // and append the base path of the original assembly (ie. look in the same dir)
            var filename = args.Name.Split(',')[0] + ".dll".ToLower();
            var asmFile = Path.Combine(@".\", "mslib", filename);

            try
            {
                return Assembly.LoadFrom(asmFile);
            }
            catch (Exception)
            {
                return null;
            }
        }


        public void Workflow_Start(object objdata)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            Log.Info("************* Service Starting **************");

            IsRunning = AdiWfOperations.LoadAppConfig();

            if (IsRunning)
            {
                Log.Info("Service Started Successfully");

                AdiWfOperations.InitialiseWorkflowOperations();
                StartEnhancementEngine();
            }
            else
            {
                Log.Error("Service Stopping due to error or manual stop.");
            }
        }

        private void StartEnhancementEngine()
        {
            while (IsRunning)
                try
                {
                    EfAdiEnrichmentDal.IsWorkflowProcessing = false;

                    AdiWfOperations.StartProcessing();
                    Thread.Sleep(Convert.ToInt32(ADIWF_Config.PollIntervalInSeconds) * 1000);

                }
                catch (Exception saeEx)
                {
                    AdiWfOperations.LogError("StartEnhancementEngine",
                        "Error Encountered During Poll Workflow Operations",
                        saeEx);
                }
        }
    }
}