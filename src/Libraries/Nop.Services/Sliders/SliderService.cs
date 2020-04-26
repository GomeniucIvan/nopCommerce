using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Sliders;
using Nop.Data;
using Nop.Services.Caching.Extensions;
using Nop.Services.Events;

namespace Nop.Services.Sliders
{
    public class SliderService : ISliderService
    {
        #region Fields

        private readonly IRepository<Slider> _sliderRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        public SliderService(
            IRepository<Slider> sliderRepository,
            IEventPublisher eventPublisher)
        {
            _sliderRepository = sliderRepository;
            _eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a slider
        /// </summary>
        /// <param name="sliderId">The slider identifier</param>
        /// <returns>Slider</returns>
        public virtual Slider GetSliderById(int sliderId)
        {
            if (sliderId == 0)
                return null;

            return _sliderRepository.ToCachedGetById(sliderId);
        }

        /// <summary>
        /// Gets sliders
        /// </summary>
        /// <param name="showHidden">Whether to show hidden records (not published)</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Sliders</returns>
        public IPagedList<Slider> GetSliders(bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
             var query = _sliderRepository.Table;

            //whether to load not published, not started and expired sliders
            if (!showHidden)
            {
                query = query.Where(slider => slider.Published);
            }

            //order records by display order
            query = query.OrderBy(slider => slider.DisplayOrder).ThenBy(slider => slider.Id);

            //return paged list of sliders
            return new PagedList<Slider>(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Deletes a slider
        /// </summary>
        /// <param name="slider">The slider</param>
        public void DeleteSlider(Slider slider)
        {
            if (slider == null)
                throw new ArgumentNullException(nameof(slider));

            _sliderRepository.Delete(slider);

            //event notification
            _eventPublisher.EntityDeleted(slider);
        }

        /// <summary>
        /// Inserts a slider
        /// </summary>
        /// <param name="slider">Slider</param>
        public void InsertSlider(Slider slider)
        {
            if (slider == null)
                throw new ArgumentNullException(nameof(slider));

            _sliderRepository.Insert(slider);

            //event notification
            _eventPublisher.EntityInserted(slider);
        }

        /// <summary>
        /// Updates the slider
        /// </summary>
        /// <param name="slider">Slider</param>
        public void UpdateSlider(Slider slider)
        {
            if (slider == null)
                throw new ArgumentNullException(nameof(slider));

            _sliderRepository.Update(slider);

            //event notification
            _eventPublisher.EntityUpdated(slider);
        }

        #endregion
    }
}
