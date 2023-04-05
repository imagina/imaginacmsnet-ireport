using Azure;
using Core;
using Core.Controllers;
using Core.Exceptions;
using Core.Transformers;
using Hangfire;
using Idata.Data;
using Idata.Data.Entities.Ireport;
using Idata.Helpers;
using Ihelpers.Helpers;
using Iprofile.Helpers;
using Ireport.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using System.Xml.Serialization;

namespace Ireport.Controllers
{
    [Authorize]
    [Route("api/ireport/v1/reports")]
    [ApiController]
    public class ReportController : ControllerBase<Report>
    {
        IReportRepository _repositoryBase;
        IHttpContextAccessor _currentContext;
        IBackgroundJobClient _backgroundJobClient;

		public ReportController(IReportRepository repositoryBase, IHttpContextAccessor currentContext, IBackgroundJobClient backgroundJobClient) : base(repositoryBase, AuthHelper.AuthUser(currentContext))
        {
            _repositoryBase = repositoryBase;
            _currentContext = currentContext;
            _backgroundJobClient = backgroundJobClient;
        }

        [HttpGet("entities")]
        public virtual async Task<IActionResult> Objects([FromQuery] UrlRequestBase? urlRequestBase)
        {
            int status = 200;
            dynamic response = null;
            try
            {
                //parser
                urlRequestBase.doNotCheckPermissions();

                await urlRequestBase.Parse(this);

               

                response = await ReportableHelper.GetReportableClasses();

                //get meta before transform, because meta will be lost once object is transformed


            }
            catch (ExceptionBase ex)
            {
                return StatusCode(ex.CodeResult, ex.CreateResponseFromException());
            }
            //reponse


            return StatusCode(status, await ResponseBase.Response(response));
        }


        public override Task<IActionResult> Show(string? criteria, [FromQuery] UrlRequestBase? urlRequestBase)
        {
            urlRequestBase.doNotCheckPermissions();
            return base.Show(criteria, urlRequestBase);
        }

        [HttpGet("data")]
        public async Task<IActionResult> data([FromQuery] UrlRequestBase? urlRequestBase)
        {
            int status = 200;
            object? response = null;
            try
            {

                urlRequestBase.doNotCheckPermissions();
                //Parse is performed inside repository


                response = await _repositoryBase.GetReportJson(urlRequestBase);



            }
            catch (ExceptionBase ex)
            {
                return StatusCode(ex.CodeResult, ex.CreateResponseFromException());
            }
            return StatusCode(status, response);
           

        }

        [HttpPost("Export")]
		public override async Task<IActionResult> Export([FromQuery] UrlRequestBase? urlRequestBase, [FromBody] BodyRequestBase? bodyRequestBase)
		{
			int status = 200;
			object? response = null;
			try
			{

				urlRequestBase.doNotCheckPermissions();

                
                var currentContext = this.HttpContext;

				urlRequestBase.currentContextToken = currentContext.Request.Headers.Authorization;

                #if DEBUG
                _repositoryBase.GetReportFile(urlRequestBase, bodyRequestBase);
                #else

                    this._backgroundJobClient.Enqueue(() => _repositoryBase.GetReportFile(urlRequestBase, bodyRequestBase));
                    
                #endif
                

              

				return StatusCode(status);



			}
			catch (ExceptionBase ex)
			{
				return StatusCode(ex.CodeResult, ex.CreateResponseFromException());
			}
			return StatusCode(status, response);
		}


	}
}
