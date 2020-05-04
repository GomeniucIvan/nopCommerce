using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Common;

namespace Nop.Web.Controllers.api
{
    [Route("api/[controller]")]
    public class CommonController : BaseApiController
    {
        #region Fields

        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly ICommonModelFactory _commonModelFactory;

        #endregion

        #region Ctor

        public CommonController(
            ICatalogModelFactory catalogModelFactory,
            ICommonModelFactory commonModelFactory)
        {
            _catalogModelFactory = catalogModelFactory;
            _commonModelFactory = commonModelFactory;
        }

        #endregion

        #region Methods

        [HttpGet]
        [Route("categories")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(List<CategorySimpleModel>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<CategorySimpleModel>>> PrepareCategories()
        {
            var result = new List<CategorySimpleModel>();
            await Task.Run(() => result = _catalogModelFactory.PrepareCategorySimpleModels().Where(v => v.IncludeInTopMenu).ToList());
            return result;
        }

        [HttpGet]
        [Route("logo")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(LogoModel), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<LogoModel>> PrepareLogo()
        {
            var result = new LogoModel();
            await Task.Run(() => result =_commonModelFactory.PrepareLogoModel());
            result.LogoPath = "http://192.168.0.104:2129/Themes/DefaultClean/Content/images/logo.png";
            return result;
        }

        #endregion
    }
}