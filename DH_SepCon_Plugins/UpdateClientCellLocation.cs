using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DH_SepCon_Plugins
{
    public class UpdateClientCellLocation:IPlugin
    {
        IOrganizationService _service;
        IPluginExecutionContext _context;
        EntityReference _preUnitCell = null;
        EntityReference _CorrectionalCenter = null;

        public void Execute(IServiceProvider serviceProvider)
        {
            _context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            if (_context.InputParameters.Contains("Target") &&
                        _context.InputParameters["Target"] is Entity && _context.Depth==1)
            {
                // Obtain the target entity from the input parameters.  
                Entity entity = (Entity)_context.InputParameters["Target"];
                
                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                _service = serviceFactory.CreateOrganizationService(_context.UserId);
                ITracingService trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                //Fetching Client's Correctional Center, so that the client can be mapped to a Designated Unit Cell in the same Correctional Center
                _CorrectionalCenter = entity.GetAttributeValue<EntityReference>("ssg_correctionalcentre");
                var sCellLocation =entity.Contains("ssg_celllocation")? entity.GetAttributeValue<String>("ssg_celllocation"):"";
                if (_context.PreEntityImages.Contains("PreImage") && _context.PreEntityImages["PreImage"] is Entity)
                {
                    Entity preMessageImage = (Entity)_context.PreEntityImages["PreImage"];
                    _preUnitCell = preMessageImage.GetAttributeValue<EntityReference>("ssg_cellid");
                    if (_CorrectionalCenter == null && preMessageImage.Contains("ssg_correctionalcentre"))
                        _CorrectionalCenter = (EntityReference)preMessageImage["ssg_correctionalcentre"];
                    if(sCellLocation == null &&sCellLocation!= String.Empty && sCellLocation!="" && preMessageImage.Contains("ssg_celllocation") && !entity.Contains("ssg_celllocation"))
                    {
                        sCellLocation = preMessageImage.GetAttributeValue<String>("ssg_celllocation") ;
                    }

                }

                

                //If Correctional Center not is blank, verify Cell Location
                if (_CorrectionalCenter != null)
                {
                    trace.Trace("Correctional Center has value");
                    try
                    {
                        if (entity.LogicalName == "contact")
                        {
                            //Check if Client's Cell Location is empty in the context
                            if (sCellLocation != String.Empty && sCellLocation != null)
                            {
                                trace.Trace("has cell location " + sCellLocation);

                                FilterExpression filter1 = new FilterExpression();
                                ConditionExpression condition1 = new ConditionExpression();
                                condition1.AttributeName = "ssg_name";
                                condition1.Operator = ConditionOperator.Equal;
                                condition1.Values.Add(sCellLocation);
                                filter1.Conditions.Add(condition1);

                                //Include Correctional Center filter only if it has value
                                if (_CorrectionalCenter != null)
                                {
                                    ConditionExpression condition2 = new ConditionExpression();
                                    condition2.AttributeName = "ssg_businessunit";
                                    condition2.Operator = ConditionOperator.Equal;
                                    condition2.Values.Add(_CorrectionalCenter.Id);
                                    filter1.Conditions.Add(condition2);
                                }

                                QueryExpression query = new QueryExpression("ssg_cell");
                                query.ColumnSet.AddColumns("ssg_cellid");
                                query.Criteria.AddFilter(filter1);

                                EntityCollection ecCell = _service.RetrieveMultiple(query);


                                // If designated unit cell is retrieved, update the designated unit cell with client and update client with unit cell and client cell changed flag as true
                                if (ecCell.Entities.Count > 0)
                                {
                                    Entity updateUnitCell = new Entity("ssg_cell");
                                    updateUnitCell.Attributes.Add("ssg_cellid", ecCell.Entities[0].GetAttributeValue<Guid>("ssg_cellid"));
                                    updateUnitCell.Attributes.Add("ssg_clientid", new EntityReference("contact", entity.Id));
                                    updateUnitCell.Attributes.Add("ssg_celllocationupdated", true);
                                    _service.Update(updateUnitCell);

                                    Entity updateClient = new Entity("contact");
                                    updateClient.Attributes.Add("ssg_cellid", new EntityReference("ssg_cell", ecCell.Entities[0].GetAttributeValue<Guid>("ssg_cellid")));
                                    updateClient.Attributes.Add("contactid", entity.GetAttributeValue<Guid>("contactid"));
                                    updateClient.Attributes.Add("ssg_clientcellchanged", new OptionSetValue(867670000));
                                    updateClient.Attributes.Add("ssg_regeneratedt", new OptionSetValue(867670000));
                                    updateClient.Attributes.Add("ssg_clonedt", new OptionSetValue(867670000));
                                    updateClient.Attributes.Add("ssg_deactivatedt", new OptionSetValue(867670001));
                                    _service.Update(updateClient);

                                }
                                //If designedated unit cell is not found, update client record with client cell change flag as true to verify the change in daily tracking record
                                else
                                {


                                    EntityReference erCTOC = null;
                                    Entity updateClient = new Entity("contact");
                                    updateClient.Attributes.Add("contactid", entity.GetAttributeValue<Guid>("contactid"));

                                    if (_context.PreEntityImages.Contains("PreImage") && _context.PreEntityImages["PreImage"] is Entity)
                                    {
                                        Entity preMessageImage = (Entity)_context.PreEntityImages["PreImage"];
                                        
                                        if (preMessageImage.Contains("ssg_lastcontinuousperiodofseparateconfinement"))
                                            erCTOC = preMessageImage.GetAttributeValue<EntityReference>("ssg_lastcontinuousperiodofseparateconfinement");
                                    }

                                    if (erCTOC != null)
                                    {
                                        var enPreCTOC = _service.Retrieve("ssg_separateconfinementperiod", erCTOC.Id, new ColumnSet("statecode"));

                                        if (enPreCTOC.GetAttributeValue<OptionSetValue>("statecode").Value == 0)
                                        {
                                            updateClient.Attributes.Add("ssg_regeneratedt", new OptionSetValue(867670000));
                                            updateClient.Attributes.Add("ssg_clonedt", new OptionSetValue(867670000));
                                        }
                                        else if (enPreCTOC.GetAttributeValue<OptionSetValue>("statecode").Value == 1)
                                            updateClient.Attributes.Add("ssg_deactivatedt", new OptionSetValue(867670000));
                                    }

                                    trace.Trace("Designed Unit cell for cell location " + sCellLocation + " is not found");
                                    
                                    updateClient.Attributes.Add("ssg_cellid", null);
                                    updateClient.Attributes.Add("ssg_clientcellchanged", new OptionSetValue(867670000));
                                    _service.Update(updateClient);

                                }

                                UpdatePreviousUnitCell(_service, _context,entity);
                            }
                            else //If Client's Cell Location is empty, Fetch the PreImage 
                            {
                                trace.Trace("Client Cell locaiton is empty");
                                EntityReference erUnitCell = null;
                                EntityReference erCTOC = null;
                                Entity updateClient = new Entity("contact");
                                updateClient.Attributes.Add("contactid", entity.GetAttributeValue<Guid>("contactid"));

                                if (_context.PreEntityImages.Contains("PreImage") && _context.PreEntityImages["PreImage"] is Entity)
                                {
                                    Entity preMessageImage = (Entity)_context.PreEntityImages["PreImage"];
                                    if(preMessageImage.Contains("ssg_cellid"))
                                        erUnitCell = preMessageImage.GetAttributeValue<EntityReference>("ssg_cellid");
                                    if(preMessageImage.Contains("ssg_lastcontinuousperiodofseparateconfinement"))
                                        erCTOC = preMessageImage.GetAttributeValue<EntityReference>("ssg_lastcontinuousperiodofseparateconfinement");
                                }
                                trace.Trace("cell location is null");

                                
                                //Clear the Designated Unit Cell's associate Client and update the Cell Location updated flag
                                if (erUnitCell != null)
                                {
                                    var enPreUnitCell = _service.Retrieve("ssg_cell", erUnitCell.Id, new ColumnSet("ssg_clientid"));

                                    Entity updateUnitCell = new Entity("ssg_cell");
                                    updateUnitCell.Attributes.Add("ssg_cellid", erUnitCell.Id);

                                    //if the previous unit cell still has the reference to the current client, then clear the client association
                                    if(enPreUnitCell!= null && enPreUnitCell.Contains("ssg_clientid") && enPreUnitCell.GetAttributeValue<EntityReference>("ssg_clientid").Id == entity.Id)
                                        updateUnitCell.Attributes.Add("ssg_clientid", null);
                                    updateUnitCell.Attributes.Add("ssg_celllocationupdated", true);
                                    _service.Update(updateUnitCell);

                                    updateClient.Attributes.Add("ssg_cellid", null);
                                }
                                if(erCTOC!= null)
                                {
                                    var enPreCTOC = _service.Retrieve("ssg_separateconfinementperiod", erCTOC.Id, new ColumnSet("statecode"));

                                    if (enPreCTOC.GetAttributeValue<OptionSetValue>("statecode").Value == 0)
                                    {
                                        updateClient.Attributes.Add("ssg_regeneratedt", new OptionSetValue(867670000));
                                        updateClient.Attributes.Add("ssg_clonedt", new OptionSetValue(867670000));
                                    }
                                    else if (enPreCTOC.GetAttributeValue<OptionSetValue>("statecode").Value == 1)
                                        updateClient.Attributes.Add("ssg_deactivatedt", new OptionSetValue(867670000));
                                }


                                
                                
                                updateClient.Attributes.Add("ssg_clientcellchanged", new OptionSetValue(867670000));
                                _service.Update(updateClient);
                            }


                        }
                    }
                    catch (Exception e)
                    {
                        throw new InvalidPluginExecutionException("Update Client Cell Location: " + e.Message);
                    }

                }
                else if (entity.Contains("ssg_correctionalcentre") && entity.GetAttributeValue<EntityReference>("ssg_correctionalcentre") == null)//If Correctional Center is blank, Client cannot be mapped to Designated Unit Cell
                {
                    trace.Trace("Correctional Center doesn't has value");
                    EntityReference erUnitCell = null;

                    EntityReference erCTOC = null;
                    Entity updateClient = new Entity("contact");
                    updateClient.Attributes.Add("contactid", entity.GetAttributeValue<Guid>("contactid"));

                    //Fetch Cell and CTOC from PreImage
                    if (_context.PreEntityImages.Contains("PreImage") && _context.PreEntityImages["PreImage"] is Entity)
                    {
                        Entity preMessageImage = (Entity)_context.PreEntityImages["PreImage"];
                        if (preMessageImage.Contains("ssg_cellid"))
                            erUnitCell = preMessageImage.GetAttributeValue<EntityReference>("ssg_cellid");
                        if (preMessageImage.Contains("ssg_lastcontinuousperiodofseparateconfinement"))
                            erCTOC = preMessageImage.GetAttributeValue<EntityReference>("ssg_lastcontinuousperiodofseparateconfinement");

                    }
                    
                    //Clear the Designated Unit Cell's associate Client and update the Cell Location updated flag
                    if (erUnitCell != null)
                    {
                        Entity updateUnitCell = new Entity("ssg_cell");
                        updateUnitCell.Attributes.Add("ssg_cellid", erUnitCell.Id);
                        updateUnitCell.Attributes.Add("ssg_clientid", null);
                        updateUnitCell.Attributes.Add("ssg_celllocationupdated", true);
                        _service.Update(updateUnitCell);

                        updateClient.Attributes.Add("ssg_cellid", null);
                        trace.Trace("update cell location as null");
                    }
                    if (erCTOC != null)
                    {
                        var enPreCTOC = _service.Retrieve("ssg_separateconfinementperiod", erCTOC.Id, new ColumnSet("statecode"));

                        if (enPreCTOC.GetAttributeValue<OptionSetValue>("statecode").Value == 0)
                        {
                            updateClient.Attributes.Add("ssg_regeneratedt", new OptionSetValue(867670000));
                            updateClient.Attributes.Add("ssg_clonedt", new OptionSetValue(867670000));
                        }
                        else if (enPreCTOC.GetAttributeValue<OptionSetValue>("statecode").Value == 1)
                            updateClient.Attributes.Add("ssg_deactivatedt", new OptionSetValue(867670000));
                    }


                    
                    updateClient.Attributes.Add("ssg_clientcellchanged", new OptionSetValue(867670000));
                    _service.Update(updateClient);

                    trace.Trace("Client is not part of Custody Center");
                    throw new InvalidPluginExecutionException("Client Cell location is modified. But Client is not part of Custody Center");
                }
                

            }
        }

        public void UpdatePreviousUnitCell(IOrganizationService service, IPluginExecutionContext context, Entity entity)
        {
            EntityReference erUnitCell = null;
            if (_context.PreEntityImages.Contains("PreImage") && _context.PreEntityImages["PreImage"] is Entity)
            {
                Entity preMessageImage = (Entity)_context.PreEntityImages["PreImage"];
                erUnitCell = preMessageImage.GetAttributeValue<EntityReference>("ssg_cellid");

            }
            if(erUnitCell!=null)
            {
                var enPreUnitCell = _service.Retrieve("ssg_cell", erUnitCell.Id, new ColumnSet("ssg_clientid"));

                Entity updateUnitCell = new Entity("ssg_cell");

                //if the previous unit cell still has the reference to the current client, then clear the client association
                if (enPreUnitCell != null && enPreUnitCell.Contains("ssg_clientid") && enPreUnitCell.GetAttributeValue<EntityReference>("ssg_clientid").Id == entity.Id)
                    updateUnitCell.Attributes.Add("ssg_clientid", null);

                
                updateUnitCell.Attributes.Add("ssg_cellid", erUnitCell.Id);
                updateUnitCell.Attributes.Add("ssg_celllocationupdated", true);
                _service.Update(updateUnitCell);
            }
        }
    }
}
