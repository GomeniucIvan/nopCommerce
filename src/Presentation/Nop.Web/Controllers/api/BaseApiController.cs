using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Controllers.api.Filters;

namespace Nop.Web.Controllers.api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [AuthorizeCustomer]
    [ApiController]
    public class BaseApiController : Controller
    {

    }
}