using SchTech.Business.Manager.Concrete.CustomerBusinessLogic.VirginMedia;

namespace SchTech.Workflow.Director.Concrete
{
    public class AdiPackageEnrichmentDirector
    {
        private EnrichmentWorkflowManager _workflowManager;

        public AdiPackageEnrichmentDirector()
        {
            _workflowManager = new EnrichmentWorkflowManager();
        }
    }
}