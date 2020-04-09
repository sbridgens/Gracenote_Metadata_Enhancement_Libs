using System;
using log4net;
using SchTech.Business.Manager.Abstract.EntityFramework;
using SchTech.Business.Manager.Concrete.EntityFramework;
using SchTech.DataAccess.Concrete.EntityFramework;
using SchTech.Entities.ConcreteTypes;


namespace SchTech.Business.Manager.Concrete
{
    public class ProgramTypes
    {
        /// <summary>
        ///     Initialize Log4net
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProgramTypes));

        public static void SetProgramType(string progType, string progSubType)
        {
            try
            {
                Log.Info("Setting Program Types for Package.");
                IGnProgramTypeLookupService programTypeLookupService = 
                    new GnProgramTypeLookupManager(new EfGnProgramTypeLookupDal());

                var lookupValue = programTypeLookupService.Get(
                    t => string.Equals(t.GnProgramType.ToLower(), progType.ToLower(),
                             StringComparison.OrdinalIgnoreCase) &&
                         string.Equals(t.GnProgramSubType.ToLower(), progSubType.ToLower(),
                             StringComparison.OrdinalIgnoreCase
                         )).LgiProgramTypeId;
                
                //set all 3 flags to ensure static flags are set correctly per package.
                EnrichmentWorkflowEntities.IsMoviePackage = false;
                EnrichmentWorkflowEntities.IsEpisodeSeries = false;
                EnrichmentWorkflowEntities.PackageIsAOneOffSpecial = false;

                switch (lookupValue)
                {
                    case 0:
                        Log.Info("Program is of type Movie");
                        EnrichmentWorkflowEntities.IsMoviePackage = true;
                        break;
                    case 1:
                    case 2:
                        Log.Info(lookupValue == 1
                            ? "Program is of type Episode"
                            : "Movie is a Series/Show asset.");
                        EnrichmentWorkflowEntities.IsEpisodeSeries = true;
                        break;
                    case 99:
                        EnrichmentWorkflowEntities.PackageIsAOneOffSpecial = true;
                        Log.Info("Program is of type Special.");
                        break;
                }
            }
            catch (Exception gptException)
            {
                Log.Error($"Error Encountered Setting Program Type: {gptException.Message}");
            }
        }


    }
}
