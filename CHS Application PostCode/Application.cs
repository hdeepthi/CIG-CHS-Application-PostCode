
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
    public class Application : IPlugin
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
            tracingService.Trace("Context depth > 1");

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity application = (Entity)context.InputParameters["Target"];
                if (application != null && application.LogicalName.ToLower() == "opportunity")
                {
                    Guid appID = application.Id;
                    if (appID != Guid.Empty)
                    {
                        string postcode = application.Attributes.Contains("cig_postcode") ? application.GetAttributeValue<string>("cig_postcode") : string.Empty;
                        if (postcode != string.Empty)
                        {
                            QueryExpression chsPostcodeQuery = new QueryExpression("cig_chspostcodes");
                            chsPostcodeQuery.ColumnSet = new ColumnSet("cig_postcode", "cig_council");
                            chsPostcodeQuery.Criteria.AddCondition("cig_postcode", ConditionOperator.Equal, postcode);
                            EntityCollection chsPostcodeQueryColl = service.RetrieveMultiple(chsPostcodeQuery);
                            Entity updateApplication = new Entity(application.LogicalName);
                            updateApplication.Id = appID;
                            if (chsPostcodeQueryColl.Entities.Count ==1)
                            {
                                string schemename= chsPostcodeQueryColl.Entities[0].FormattedValues["cig_council"];
                                updateApplication["cig_inscheme"] = true;
                                updateApplication["cig_schemename"] = schemename;
                            }
                            else
                            {
                                updateApplication["cig_inscheme"] = false;
                            }
                            service.Update(updateApplication);
                        }
                    }
                }
            }
        }
    }
}
