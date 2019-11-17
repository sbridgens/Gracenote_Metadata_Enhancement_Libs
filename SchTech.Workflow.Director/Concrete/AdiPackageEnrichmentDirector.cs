using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
