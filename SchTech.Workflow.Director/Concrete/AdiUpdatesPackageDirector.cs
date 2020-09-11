using SchTech.Business.Manager.Concrete.CustomerBusinessLogic.VirginMedia;

namespace SchTech.Workflow.Director.Concrete
{
    public class AdiUpdatesPackageDirector
    {
        private EnrichmentWorkflowManager _workflowManager;

        public AdiUpdatesPackageDirector()
        {
            _workflowManager = new EnrichmentWorkflowManager();
        }
    }
}