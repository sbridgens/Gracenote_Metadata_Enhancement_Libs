using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;
using SchTech.Configuration.Manager.Schema.GNUpdateTracker;
using SchTech.DataAccess.Concrete.EntityFramework;

namespace ADIWFE_GNTrackerClient
{
        public class GnTrackerController
        {
            /// <summary>
            ///     Initialize Log4net
            /// </summary>
            private static readonly ILog Log = LogManager.GetLogger(typeof(GnTrackerController));

            public static bool IsRunning { get; set; }

            private GnTrackingOperations GnTrackingOperations { get; set; }
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

            public GnTrackerController()
            {
                GnTrackingOperations = new GnTrackingOperations();
            }

            public void Workflow_Start(object objdata)
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                Log.Info("************* Service Starting **************");

                IsRunning = GnTrackingOperations.LoadAppConfig();

                if (IsRunning)
                {

                    Log.Info("Service Started Successfully");

                    GnTrackingOperations.InitialiseWorkflowOperations();
                    StartTrackingEngine();
                }
                else
                {
                    Log.Error("Service Stopping due to error or manual stop.");
                }
            }

            private void StartTrackingEngine()
            {
                var canExecute = DateTime.Now;
                var dtNow = DateTime.Now.AddMinutes(1);

                while (IsRunning)
                {
                    try
                    {
                        if(!EfAdiEnrichmentDal.IsWorkflowProcessing && canExecute <= dtNow)
                        {
                            EfAdiEnrichmentDal.IsWorkflowProcessing = false;
                            GnTrackingOperations.StartOperations();
                            canExecute = DateTime.Now.AddMinutes(Convert.ToInt32(GN_UpdateTracker_Config.PollIntervalInMinutes));
                            Log.Info($"Next Poll will occur at: {canExecute}");
                        }

                        Thread.Sleep(30000);
                        dtNow = DateTime.Now;
                    }
                    catch (Exception saeEx)
                    {
                        GnTrackingOperations.LogError("StartEnhancementEngine",
                            "Error Encountered During Poll Workflow Operations",
                            saeEx);
                    }
                }

            }
        }
    }
