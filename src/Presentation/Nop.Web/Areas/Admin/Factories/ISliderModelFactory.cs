using Nop.Core.Domain.Sliders;
using Nop.Web.Areas.Admin.Models.Sliders;

namespace Nop.Web.Areas.Admin.Factories
{
    public interface ISliderModelFactory
    {
        /// <summary>
        /// Prepare paged slider list model
        /// </summary>
        /// <param name="searchModel">Product search model</param>
        /// <returns>Slider list model</returns>
        SliderListModel PrepareSliderListModel(SliderSearchModel searchModel);

        /// <summary>
        /// Prepare slider model
        /// </summary>
        /// <param name="model">Slider model</param>
        /// <param name="slider">Slider</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Slider model</returns>
        SliderModel PrepareSliderModel(SliderModel model, Slider slider, bool excludeProperties = false);
    }
}
