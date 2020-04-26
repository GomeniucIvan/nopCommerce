using System;
using Nop.Core.Domain.Customers;
using Nop.Web.Controllers.api.Filters;
using Nop.Web.Framework.Controllers;

namespace Nop.Web.Controllers.api
{
    [AuthorizeCustomer]
    public class BaseApiController : BaseController
    {

    }
}