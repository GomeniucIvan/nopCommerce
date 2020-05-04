using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Sliders;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Sliders;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Sliders;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    public class SliderModelFactory : ISliderModelFactory
    {
        #region Fields

        private readonly ISliderService _sliderService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public SliderModelFactory(
            ISliderService sliderService,
            IPictureService pictureService,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            ISettingService settingService)
        {
            _sliderService = sliderService;
            _pictureService = pictureService;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _settingService = settingService;
        }

        #endregion

        /// <summary>
        /// Prepare paged slider list model
        /// </summary>
        /// <param name="searchModel">Slider search model</param>
        /// <returns>Slider list model</returns>
        public SliderListModel PrepareSliderListModel(SliderSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get sliders
            var sliders = _sliderService.GetSliders(showHidden: true, pageIndex: searchModel.Page, searchModel.PageSize);

            //prepare list model
            var model = new SliderListModel().PrepareToGrid(searchModel, sliders, () =>
            {
                return sliders.Select(slider =>
                {
                    //fill in model values from the entity
                    var sliderModel = slider.ToModel<SliderModel>();

                    //fill in additional values (not existing in the entity)
                    sliderModel.PictureThumbnailUrl = _pictureService.GetPictureUrl(slider.Id, 75);
                    sliderModel.EntityTypeName = _localizationService.GetLocalizedEnum((SliderEntityTypeEnum)slider.EntityTypeId);
                    sliderModel.Name = _localizationService.GetLocalized(slider, v =>v.Name);

                    return sliderModel;
                });
            });

            return model;
        }

        public SliderModel PrepareSliderModel(SliderModel model, Slider slider, bool excludeProperties = false)
        {
           Action<SliderLocalizedModel, int> localizedModelConfiguration = null;

            if (slider != null)
            {
                //fill in model values from the entity
                model ??= slider.ToModel<SliderModel>();

                //define localized model configuration action
                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = _localizationService.GetLocalized(slider, entity => entity.Name, languageId, false, false);
                    locale.Description = _localizationService.GetLocalized(slider, entity => entity.Description, languageId, false, false);
                    locale.MobilePictureId = _localizationService.GetLocalized(slider, entity => entity.MobilePictureId, languageId, false, false);
                };
            }

            //set default values for the new model
            if (slider == null)
            {
                model.Published = true;
                model.DisplayOrder = _sliderService.GetSliders().Count + 1;
            }

            //prepare localized models
            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            //mobile settings
            var mobileSettings = _settingService.LoadSetting<MobileSettings>();
            model.ShowMobileSettings = mobileSettings.ActivateMobileSettings;

            return model;
        }
    }
}
