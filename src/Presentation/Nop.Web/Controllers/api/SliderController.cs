using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Models.Sliders;

namespace Nop.Web.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class SliderController : BaseApiController
    {
        #region Fields

        //TODO change to public model factory
        private readonly ISliderModelFactory _sliderModelFactory;

        #endregion

        #region Ctor

        public SliderController(
            ISliderModelFactory sliderModelFactory)
        {
            _sliderModelFactory = sliderModelFactory;
        }

        #endregion

        #region Methods

        [HttpGet]
        [Route("all")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(List<SliderModel>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<SliderModel>>> GetAllSliders()
        {
            var result = new List<SliderModel>();
            await Task.Run(() =>
            {
                result = _sliderModelFactory.PrepareSliderList();
            });
            return result;
        }

        #endregion
    }
}