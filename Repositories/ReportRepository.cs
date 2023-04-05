using Core;
using Core.Repositories;
using Ireport.Data;
using Microsoft.Extensions.DependencyInjection;
using Ireport.Repositories.Interfaces;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using Newtonsoft.Json;
using Core.Transformers;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Azure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Data.SqlClient;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Razor.Templating.Core;
using jsreport.Local;
using System.Runtime.InteropServices;
using jsreport.Binary;
using jsreport.Types;
using jsreport.Client;
using Core.Storage.Interfaces;
using Iprofile.Helpers;
using Setup.Data;
using TypeSupport.Extensions;
using System.Collections.Generic;
using Ireport.Helpers;
using Core.Exceptions;
using Idata.Data;
using Idata.Data.Entities.Ireport;
using Ihelpers.Helpers;
using Idata.Data.Entities.Ireport;
using Idata.Data.Entities.Iprofile;
using Idata.Data.Entities.Ireport;
using Idata.Data.Entities.Isite;
using Idata.Data.Entities.Page;
using Idata.Data.Entities.Ramp;
using Idata.Data.Entities.Setup;
using Idata.Entities.Core;
using Idata.Entities.Icomment;
using Idata.Entities.Idhl;
namespace Ireport.Repositories
{
    public class ReportRepository : RepositoryBase<Idata.Data.Entities.Ireport.Report>, IReportRepository
    {
        IReportTypeRepository _reportTypeRepository;

		IStorageBase _storageBase;

		public ReportRepository(IStorageBase storageBase)
        {

			_storageBase = storageBase;
        }


        public async Task<dynamic> GetReportJson(UrlRequestBase request)
        {
			try
			{

                // This code is extracting a report from a request and preparing a new request to get the report data

                // Variables are being defined to store the include and setting properties from the original request
                var reportIncludes = request.include;
                var reportSettings = request.setting;

                // The include and setting properties on the request are being set to specific values
                request.include = "reportType";
                request.setting = string.Empty;

                // The request is parsed
                await request.Parse();

                // The filter for the report is being obtained from the request
                request.criteria = request.GetFilter("reportId");

                // A report is obtained from the base class
                var report = await base.GetItem(request);

                // A repository is obtained using the report entity type
                dynamic Repository = Factory.RepositoryFactory.GetRepository(report.entity);

                // A new report request is being created
                var reportRequest = new UrlRequestBase();

                // The filter, page, take, and include properties from the original request are being copied to the new report request
                reportRequest.filter = request.filter;
                reportRequest.page = request.page;
                reportRequest.take = request.take;
                reportRequest.include = reportIncludes;

                // Permissions check is skipped for the new report request
                reportRequest.doNotCheckPermissions();

                // The new report request is parsed
                await reportRequest.Parse();

                // Results are obtained from the repository using the new report request
                dynamic results = await Repository.GetItemsBy(reportRequest);

                // Variables to store metadata and response data are being defined
                dynamic meta = null;
                Dictionary<string, dynamic?> response = new Dictionary<string, dynamic?>();
                object returnData = null;


                if (results.Count > 0)
				{

					bool isSP = !string.IsNullOrEmpty(report.reportType.procedure_name);
				
					meta = await ResponseBase.GetMeta(results);

					if (!isSP)
					{

						var transformedItems = await TransformerBase.TransformCollection(results);

						// string? entityName = ((string)report.entity).Split('.').LastOrDefault();

						Dictionary<string, string>? pathFields = null;
						//Extract columns to be filtered
						if (report.jsonColumns.HasValues)
						{
							pathFields = new Dictionary<string, string>();

							foreach (var column in report.jsonColumns)
							{
								pathFields.Add(column.ToString().ToCamelCase(), string.Empty);
							}
						}


						string result = await TransformerBase.GetReportJson(transformedItems, pathFields);

						returnData = JArray.Parse(result);
					}
					else
					{
                        //Assign the procedure name from the report type
                        string procedureName = report.reportType.procedure_name;

                        //Check if the stored procedure exists in the database
                        bool procedureExists = await Ihelpers.Helpers.EntityFrameworkCoreHelper.StoredProcedureExists((DatabaseFacade)_dataContext.Database, $"{procedureName}");

                        //If the stored procedure exists
                        if (procedureExists)
                        {
                            //Convert the results to a list of work orders
                            var typeResults = (Ihelpers.Helpers.PaginatedList<WorkOrder>)results;

                            //Create a semicolon-separated string of IDs from the list of work orders
                            string idList = string.Join(';', typeResults.Select(c => c.id));

                            //Format the sort string from the report
                            string jsonSortString = report.sort?.Replace("{", string.Empty).Replace("}", string.Empty).Replace("': '", " ").Replace("'", string.Empty);

                            //Create two parameters for the stored procedure: "id_list" and "sort"
                            SqlParameter param = new SqlParameter("id_list", idList);
                            SqlParameter param2 = new SqlParameter("sort", jsonSortString);

                            //Assign the stored procedure name to a string
                            string sql = $"{procedureName}";

                            //Execute the stored procedure and convert the result to a dictionary
                            Dictionary<string, List<string>> csvDict = await EntityFrameworkCoreHelper.FromSqlQueryToDictionary((DatabaseFacade)_dataContext.Database, sql, System.Data.CommandType.StoredProcedure, param, param2).SingleAsync();

                            //Convert the dictionary to a JSON report, or an empty object if the dictionary is null
                            returnData = csvDict != null ? csvDict.ToJsonReport(report.jsonColumns) : new();
                        }
                    }

				}

				response = await ResponseBase.Response(returnData ?? new JArray(), meta ?? JObjectHelper.GetDefaultMeta());

				return response;

			}
			catch (Exception ex)
			{

				//ExceptionBase.HandleException(ex, $"Error Creating Report Json", $"ExceptionMessage = {ex.Message}   trace received: URL | " + JsonConvert.SerializeObject(request).Trim().Replace("\"", "'"));


				//Loguear el error
				//Atrapar el error y devolver el error :"este reporte esta temporalmente deshabilitado" al usuario
				return null;
            }

        }


		public async Task<dynamic> GetReportFile(UrlRequestBase request, BodyRequestBase bodyRequest)
		{
			try
			{
				await bodyRequest.Parse();

				//Get the user that sent request
				request.currentContextUser = AuthHelper.AuthUser(request.getCurrentContextToken());

				request.currentContextUser.Initialize();


				//Extract report from request
				var reportIncludes = request.include;
				var reportSettings = request.setting;

				//Modify request to include only reportType
				request.include = "reportType";
				request.setting = string.Empty;
				request.filter = bodyRequest._filter;
				await request.Parse();

				//Get report based on reportId filter
				request.criteria = request.GetFilter("reportId");
				var report = await base.GetItem(request);
				
				//Get repository for report's entity
				dynamic Repository = Factory.RepositoryFactory.GetRepository(report.entity);

				//Create new request for report data
				var reportRequest = new UrlRequestBase();
				reportRequest.filter = request.filter;
				reportRequest.page = request.page;
				reportRequest.take = request.take;
				reportRequest.include = reportIncludes;
				reportRequest.doNotCheckPermissions();
				await reportRequest.Parse();

				//Get report data from repository
				dynamic results = await Repository.GetItemsBy(reportRequest);

				if (results.Count > 0)
				{
					//CHeck if stored procedure exists
					bool existSP = await _dataContext.Database.StoredProcedureExists(report.reportType.procedure_name);



					//If SP exists then proceed to consume it with results id

					if (existSP)
                    {

                        // Extract list of ids from results
                        string id_list = string.Join(';', Ihelpers.Helpers.TypeHelper.GetIds(results));
                        var jsonSortString = report.sort?.Replace("{", string.Empty).Replace("}", string.Empty).Replace("': '", " ").Replace("'", string.Empty);

                        // Create SP parameter with id list
                        SqlParameter param = new SqlParameter("id_list", id_list);
                        SqlParameter param2 = new SqlParameter("sort", jsonSortString);
                        // Execute stored procedure to get raw data
                        string sql = $"{report.reportType.procedure_name}";

						var rawSPData = await _dataContext.Database.FromSqlQueryToDictionary(sql, System.Data.CommandType.StoredProcedure, param, param2).SingleAsync();


                        //Initialice csv string, first line is for title
                        string csvString = $"report {report.reportType.name} \r\n";


						//Convert to csv and append the SP data result to csv string
						csvString += string.Join(Environment.NewLine, rawSPData.ToCsv('|', report.jsonColumns).MatchingReportTypeHeaders(reportTypeColumns: report.reportType.jsonColumns));


                        //Get report format
                        string? reportFormat = JObjectHelper.GetJObjectValue<string>(bodyRequest.exportParams, "fileFormat") ?? "xlsx";

						//Initialize jsReport Params
						var html = await RazorTemplateEngine.RenderAsync("~/CSVReport/CSVReport.cshtml", csvString);
						
						string? url = Ihelpers.Helpers.ConfigurationHelper.GetConfig<string>("JsReportingServices:Url");

						string? userName = Ihelpers.Helpers.ConfigurationHelper.GetConfig<string>("JsReportingServices:UserName");

						string? password = Ihelpers.Helpers.ConfigurationHelper.GetConfig<string>("JsReportingServices:Password");

						string? filename = $"report{report.reportType.name.Replace(" ", "_").Trim()}{report.id}_{request.currentContextUser.id}.{reportFormat}";


						//Perform jsReport render based on report format 


						switch (reportFormat)
                        {
                            case "csv":
                            case "xlsx":
								//Extrat the table from rendered HTML to pass it to jsreport to generate excel file
								string table = html.Split("<body>").Last().Split("</body>").First().Trim();

								//use local jsreporting because cloud one gives timeout error (licence)
								var rs = new LocalReporting()
								.KillRunningJsReportProcesses()
								.UseBinary(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? JsReportBinary.GetBinary() : jsreport.Binary.Linux.JsReportBinary.GetBinary())
								.Configure(cfg => cfg.AllowedLocalFilesAccess().FileSystemStore().BaseUrlAsWorkingDirectory())
								.AsUtility()
								.Create();

								//Render the csv | xlsx using jsReport
								var generatedExcel = await rs.RenderAsync(new RenderRequest
								{
									Template = new Template
									{
										Recipe = Recipe.HtmlToXlsx,
										Engine = Engine.JsRender,
										Content = table,

									},
									Options = new RenderOptions
									{
										Timeout = 10000000
									}
								});



								//Upload the file to storage handler
								await _storageBase.CreateFile(filename, generatedExcel.Content);


                                break;

                            case "pdf":

								//Initialize render settings and render the HTML to a pdf
								var pdfRS = new ReportingService(url, userName, password);
								var generatedPdf = await pdfRS.RenderAsync(new RenderRequest
								{
									Template = new Template
									{
										Recipe = Recipe.ChromePdf,

										Engine = Engine.None,
										Content = html,
										Chrome = new Chrome
										{
											MarginTop = "10",
											MarginBottom = "10",
											MarginLeft = "50",
											MarginRight = "50",
											Landscape = true,
											
										}

									}
								});


								//Upload the file to storage handler
								await _storageBase.CreateFile(filename, generatedPdf.Content);
								
                                break;
                        }

					}


				}



				return "";

			}
			catch (Exception ex)
			{

				ExceptionBase.HandleException(ex, $"Error Creating Report File", $"ExceptionMessage = {ex.Message}   trace received: BODY | " + JsonConvert.SerializeObject(bodyRequest._attributes).Trim().Replace("\"", "'") + " |  URL | " + JsonConvert.SerializeObject(request).Trim().Replace("\"", "'"));
			
				return "";
			}

		}


		public override async Task SyncRelations(object? input, dynamic relations, dynamic dataContext)
		{
			//First clear current relations 
			var model = (Idata.Data.Entities.Ireport.Report?)input;

			//List for storing relations given in front.
			List<long?> relationIds;

			
			

			//new dbContext is necesary for avoid strict object tracking in previous context
			//Already implements IDisposable so when finish invoke gets disposed

			using (var db = new IdataContext())
			{
				//get the model again
				var internalModel = await db.Reports.Where(rol => rol.id == model.id).FirstOrDefaultAsync();

				//Asign relations
				if (relations.ContainsKey("users"))
				{
                    //clear all current relations

                    model.users?.Clear();


                    await dataContext.SaveChangesAsync(CancellationToken.None);

                    relationIds = relations["users"];

					internalModel.users = _dataContext.Users.Where(dep => relationIds.Contains(dep.id)).ToList();
				}

				if (relations.ContainsKey("roles"))
				{
                    //clear all current relations

                    model.roles?.Clear();

                    await dataContext.SaveChangesAsync(CancellationToken.None);

                    relationIds = relations["roles"];

					internalModel.roles = _dataContext.Roles.Where(dep => relationIds.Contains(dep.id)).ToList();
				}
				//save the changes to the model
				await db.SaveChangesAsync(CancellationToken.None);

			}


		}


		public override void CustomFilters(ref IQueryable<Idata.Data.Entities.Ireport.Report> query, ref UrlRequestBase? requestBase)
		{

			if (!requestBase.currentContextUser.HasAccess("ireport.folders.index-all"))
			{
				var userRoles = (List<Role>)requestBase.currentContextUser.roles;

				var user = (User)requestBase.currentContextUser;

				query = query.Include("roles");
				query = query.Include("users");

				//Get the reports by users assigned and also get by roles of the current user roles
				query = query.Where(f => f.roles.Count() == 0 || f.roles.Any(r => userRoles.Contains(r)));
				query = query.Where(f => f.users.Count() == 0 || f.users.Contains(user));
			}

		}
	}
}
