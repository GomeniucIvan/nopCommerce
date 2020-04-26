using System.Collections.Generic;
using Nop.Web.Models.Sliders;

namespace Nop.Web.Factories
{
    public interface ISliderModelFactory
    {
        /// <summary>
        /// Prepare slider list
        /// </summary>
        /// <returns>Slider list</returns>
        List<SliderModel> PrepareSliderList();
    }
}
