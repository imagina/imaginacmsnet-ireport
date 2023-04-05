using Core;
using Core.Controllers;
using Core.Exceptions;
using Ireport.Repositories.Interfaces;
using Iprofile.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Xml.Serialization;
using Idata.Data;
using Idata.Data.Entities.Ireport;

namespace Ireport.Controllers
{
    //[Authorize]
    [Route("api/ireport/v1/folders")]
    [ApiController]
    public class FolderController : ControllerBase<Folder>
    {
        public FolderController(IFolderRepository repositoryBase, IHttpContextAccessor currentContext) : base(repositoryBase, AuthHelper.AuthUser(currentContext))
        {
    
        }


        [HttpPost("order-position")]
        public virtual async Task<IActionResult> OrderPositions([FromQuery] UrlRequestBase? urlRequestBase, [FromBody] BodyRequestBase? bodyRequestBase)
        {
            int status = 200;
            object? response = null;

            try
            {

                Folder pos = new Folder();

                string helper = JsonConvert.SerializeObject(pos);

                urlRequestBase.doNotCheckPermissions();

                await urlRequestBase.Parse(this);

                await bodyRequestBase.Parse();

                response = await _repositoryBase.UpdateOrdering(urlRequestBase, bodyRequestBase);


            }
            catch (ExceptionBase ex)
            {
                return StatusCode(ex.CodeResult, ex.CreateResponseFromException());
            }

            return StatusCode(200);
        }

    }
}
