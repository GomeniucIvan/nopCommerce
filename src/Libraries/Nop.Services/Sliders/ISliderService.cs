using Nop.Core;
using Nop.Core.Domain.Sliders;

namespace Nop.Services.Sliders
{
    public interface ISliderService
    {
        /// <summary>
        /// Gets a slider
        /// </summary>
        /// <param name="sliderId">The slider identifier</param>
        /// <returns>Poll</returns>
        Slider GetSliderById(int sliderId);

        /// <summary>
        /// Gets sliders
        /// </summary>
        /// <param name="showHidden">Whether to show hidden records (not published)</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Sliders</returns>
        IPagedList<Slider> GetSliders(bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Deletes a slider
        /// </summary>
        /// <param name="slider">The slider</param>
        void DeleteSlider(Slider slider);

        /// <summary>
        /// Inserts a slider
        /// </summary>
        /// <param name="slider">Slider</param>
        void InsertSlider(Slider slider);

        /// <summary>
        /// Updates the slider
        /// </summary>
        /// <param name="slider">Slider</param>
        void UpdateSlider(Slider slider);
    }
}
