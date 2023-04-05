using Core.Controllers;
using Idata.Data;
using Idata.Data.Entities.Ireport;
using Iprofile.Helpers;
using Ireport.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Serialization;

namespace Ireport.Controllers
{
    //[Authorize]
    [Route("api/ireport/v1/report-types")]
    [ApiController]
    public class ReportTypeController : ControllerBase<ReportType>
    {
        public ReportTypeController(IReportTypeRepository repositoryBase, IHttpContextAccessor currentContext) : base( repositoryBase, AuthHelper.AuthUser(currentContext))
        {

        }
    }
}
