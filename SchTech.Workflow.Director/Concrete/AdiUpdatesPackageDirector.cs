using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
