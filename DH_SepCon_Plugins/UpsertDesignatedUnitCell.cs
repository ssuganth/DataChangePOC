using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DH_SepCon_Plugins
{
    public class UpsertDesignatedUnitCell : IPlugin
    {
        IOrganizationService _service;
        IPluginExecutionContext _context;

        public void Execute(IServiceProvider serviceProvider)
        {
            _context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            if (_context.InputParameters.Contains("Target") &&
                        _context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.  
                Entity entity = (Entity)_context.InputParameters["Target"];

                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                _service = serviceFactory.CreateOrganizationService(_context.UserId);
                ITracingService trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                try
                {
                    if (entity.LogicalName == "ssg_cellimport")
                    {
                        FilterExpression filter1 = new FilterExpression();

                        var ssg_name = entity.GetAttributeValue<String>("ssg_name");
                        var CorrectionalCenter = entity.GetAttributeValue<EntityReference>("ssg_businessunit");
                        ConditionExpression condition1 = new ConditionExpression();
                        condition1.AttributeName = "ssg_name";
                        condition1.Operator = ConditionOperator.Equal;
                        condition1.Values.Add(ssg_name);
                        filter1.Conditions.Add(condition1);

                        ConditionExpression condition2 = new ConditionExpression();
                        condition2.AttributeName = "ssg_businessunit";
                        condition2.Operator = ConditionOperator.Equal;
                        condition2.Values.Add(CorrectionalCenter.Id);
                        filter1.Conditions.Add(condition2);

                        
                        

                        QueryExpression query = new QueryExpression("ssg_cell");
                        query.ColumnSet.AddColumns("ssg_cellid");
                        query.Criteria.AddFilter(filter1);

                        EntityCollection ecCell = _service.RetrieveMultiple(query);

                        Entity cellToUpsert = new Entity("ssg_cell");
                        cellToUpsert.Attributes.Add("ssg_unitcode", entity.GetAttributeValue<String>("ssg_unitcode"));
                        cellToUpsert.Attributes.Add("ssg_cellcode", entity.GetAttributeValue<String>("ssg_cellcode"));                       
                        cellToUpsert.Attributes.Add("ssg_celltype", entity.GetAttributeValue<OptionSetValue>("ssg_celltype"));
                        cellToUpsert.Attributes.Add("ssg_businessunit", entity.GetAttributeValue<EntityReference>("ssg_businessunit"));
                        cellToUpsert.Attributes.Add("ssg_callbox", entity.GetAttributeValue<String>("ssg_callbox"));
                        cellToUpsert.Attributes.Add("ssg_camera", entity.GetAttributeValue<String>("ssg_camera"));
                        cellToUpsert.Attributes.Add("ssg_refreshed", true);
                        cellToUpsert.Attributes.Add("statecode", new OptionSetValue(0));
                        cellToUpsert.Attributes.Add("statuscode", new OptionSetValue(1));
                        if (entity.Contains("ssg_rosterprintpage"))
                            cellToUpsert.Attributes.Add("ssg_rosterprintpage", entity.GetAttributeValue<String>("ssg_rosterprintpage"));

                        if (ecCell.Entities.Count == 1)
                        {
                            cellToUpsert.Attributes.Add("ssg_cellid", ecCell.Entities[0].GetAttributeValue<Guid>("ssg_cellid"));

                            _service.Update(cellToUpsert);

                            Entity importCell = new Entity("ssg_cellimport");
                            importCell.Attributes.Add("ssg_importstatus", "updated");
                            importCell.Attributes.Add("ssg_cellimportid", entity.GetAttributeValue<Guid>("ssg_cellimportid"));
                            _service.Update(importCell);


                        }
                        else if (ecCell.Entities.Count == 0)
                        {

                            cellToUpsert.Attributes.Add("ssg_name", entity.GetAttributeValue<String>("ssg_name"));

                            _service.Create(cellToUpsert);

                            Entity importCell = new Entity("ssg_cellimport");
                            importCell.Attributes.Add("ssg_importstatus", "created");
                            importCell.Attributes.Add("ssg_cellimportid", entity.GetAttributeValue<Guid>("ssg_cellimportid"));
                            _service.Update(importCell);
                        }

                    }
                }
                catch(Exception e)
                {
                    Entity importCell = new Entity("ssg_cellimport");
                    importCell.Attributes.Add("ssg_importstatus", e.Message);
                    importCell.Attributes.Add("ssg_cellimportid", entity.GetAttributeValue<Guid>("ssg_cellimportid"));
                    _service.Update(importCell);
                }
                


            }
        }
    }
}

    