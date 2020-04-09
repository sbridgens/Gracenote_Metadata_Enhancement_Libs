using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace ADIWFE_GNTrackerClient
{
    class Program
    {
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs rargs)
        {
            // Ignore missing resources
            if (rargs.Name.Contains(".resources"))
                return null;

            // check for assemblies already loaded
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.FullName == rargs.Name);
            if (assembly != null)
                return assembly;

            // Try to load by filename - split out the filename of the full assembly name
            // and append the base path of the original assembly (ie. look in the same dir)
            var filename = rargs.Name.Split(',')[0] + ".dll".ToLower();

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

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var manager = new GnTrackingManager();

            var thread = new Thread(manager.Workflow_Start);
            thread.Start(manager);
            Thread.Sleep(0);
            //TestAdiSerialization();
            //TestDBData();
            //TestApiCall();
            Console.ReadLine();
        }
    }
}
