using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Localization;

namespace Nop.Web.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class LocalizationController : BaseApiController
    {
        #region Fields

        private readonly ILanguageService _languageService;
        private readonly ILanguageModelFactory _languageModelFactory;

        #endregion

        #region Ctor

        public LocalizationController(
            ILanguageService languageService,
            ILanguageModelFactory languageModelFactory)
        {
            _languageService = languageService;
            _languageModelFactory = languageModelFactory;
        }

        #endregion

        #region Methods

        [HttpGet]
        [Route("{languageId}")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(List<LocaleResourceModel>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<LocaleResourceModel>>> GetLocaleResourceByIdAsync(int languageId)
        {
            var result = new List<LocaleResourceModel>();
            var language = _languageService.GetLanguageById(languageId);
            result = _languageModelFactory.PrepareLocaleResourceListModel(new LocaleResourceSearchModel(){ Length = int.MaxValue}, language).Data.Where(v => v.ResourceName.Contains("mobile")).ToList();
            return result;
        }

        #endregion
    }
}