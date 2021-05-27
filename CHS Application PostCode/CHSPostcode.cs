
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xrm.Sdk;
//using Microsoft.Crm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System.ServiceModel;
using System.ServiceModel.Description;
//using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System.IO;

//using System.Activities;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Messages;

namespace CIG_CHS_PLugins
{
    public class CHSPostcode : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {

            IPluginExecutionContext context =
               (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory factory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = factory.CreateOrganizationService(context.UserId);

            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.Depth > 1)
            {
                // tracingService.Trace("Context depth > 1");
                return;
            }

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity chsPostcode = (Entity)context.InputParameters["Target"];
                if (chsPostcode != null && chsPostcode.LogicalName.ToLower() == "cig_chspostcodes")
                {
                    Guid postcodeID = chsPostcode.Id;
                    if (postcodeID != Guid.Empty)
                    {
                        string postcode = chsPostcode.Attributes.Contains("cig_postcode") ? chsPostcode.GetAttributeValue<string>("cig_postcode") : string.Empty;
                        if (postcode != string.Empty)
                        {
                            QueryExpression applicationsQuery = new QueryExpression("opportunity");
                            applicationsQuery.ColumnSet = new ColumnSet("cig_postcode,cig_inscheme");
                            applicationsQuery.Criteria.AddCondition("cig_postcode", ConditionOperator.Equal, postcode);
                            EntityCollection applicationQueryColl = service.RetrieveMultiple(applicationsQuery);
                            foreach(Entity application in applicationQueryColl.Entities)
                            {
                                Entity updateApplication = new Entity(application.LogicalName);
                                updateApplication.Id = application.Id;
                                updateApplication["cig_inscheme"] = true;
                                service.Update(updateApplication);
                            }
                          
                        }
                    }
                }
            }

        }
    }
}
