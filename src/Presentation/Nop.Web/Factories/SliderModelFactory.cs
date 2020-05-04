using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Sliders;
using Nop.Core.Domain.Topics;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Sliders;
using Nop.Web.Models.Sliders;

namespace Nop.Web.Factories
{
    public class SliderModelFactory : ISliderModelFactory
    {
        #region Fields

        private readonly ISliderService _sliderService;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public SliderModelFactory(
            ISliderService sliderService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            IWebHelper webHelper)
        {
            _sliderService = sliderService;
            _localizationService = localizationService;
            _pictureService = pictureService;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _webHelper = webHelper;
        }

        #endregion

        #region Helpers

        private string GenerateRedirectUrl(Slider slider)
        {
            switch ((SliderEntityTypeEnum)slider.EntityTypeId)
            {
                case SliderEntityTypeEnum.None:
                    return "";

                case SliderEntityTypeEnum.Product:
                    return $"{_webHelper.GetStoreLocation()}{_urlRecordService.GetActiveSlug(slider.EntityId.Value, nameof(Product),_workContext.WorkingLanguage.Id)}";

                case SliderEntityTypeEnum.Category:
                    return $"{_webHelper.GetStoreLocation()}{_urlRecordService.GetActiveSlug(slider.EntityId.Value, nameof(Category),_workContext.WorkingLanguage.Id)}";

                case SliderEntityTypeEnum.Topic:
                    return $"{_webHelper.GetStoreLocation()}{_urlRecordService.GetActiveSlug(slider.EntityId.Value, nameof(Topic),_workContext.WorkingLanguage.Id)}";

                case SliderEntityTypeEnum.News:
                    return $"{_webHelper.GetStoreLocation()}{_urlRecordService.GetActiveSlug(slider.EntityId.Value, nameof(NewsItem),_workContext.WorkingLanguage.Id)}";

                default:
                    return "";
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare slider list
        /// </summary>
        /// <returns>Slider list</returns>
        public List<SliderModel> PrepareSliderList(bool mobile = false)
        {
            var sliders = _sliderService.GetSliders();

            if (mobile)
            {
                return sliders.Select(v => new SliderModel()
                {
                    Name = _localizationService.GetLocalized(v, x => x.Name),
                    PictureUrl = _pictureService.GetPictureUrl(_localizationService.GetLocalized(v, x => x.MobilePictureId), 300, true),
                    EntityId = v.EntityId,
                    EntityType = (SliderEntityTypeEnum)v.EntityTypeId,
                }).ToList();
            }
            else
            {
                return sliders.Select(v => new SliderModel()
                {
                    Name = _localizationService.GetLocalized(v, x => x.Name),
                    Description = _localizationService.GetLocalized(v, x => x.Description),
                    PictureUrl = _pictureService.GetPictureUrl(v.PictureId),
                    EntityId = v.EntityId,
                    EntityType = (SliderEntityTypeEnum)v.EntityTypeId,
                    RedirectUrl = GenerateRedirectUrl(v)
                }).ToList();
            }
        }

        #endregion
    }
}
