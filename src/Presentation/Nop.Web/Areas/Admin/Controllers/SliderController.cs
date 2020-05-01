using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Polls;
using Nop.Core.Domain.Sliders;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Sliders;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.News;
using Nop.Web.Areas.Admin.Models.Sliders;
using Nop.Web.Areas.Admin.Models.Topics;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Areas.Admin.Controllers
{
    public class SliderController : BaseAdminController
    {
        #region Fields

        private readonly ISliderModelFactory _sliderModelFactory;
        private readonly ISliderService _sliderService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly ICategoryModelFactory _categoryModelFactory;
        private readonly ITopicModelFactory _topicModelFactory;
        private readonly INewsModelFactory _newsModelFactory;

        #endregion

        #region Ctor

        public SliderController(
            ISliderModelFactory sliderModelFactory,
            ISliderService sliderService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            ILocalizedEntityService localizedEntityService,
            IProductModelFactory productModelFactory,
            ICategoryModelFactory categoryModelFactory,
            ITopicModelFactory topicModelFactory,
            INewsModelFactory newsModelFactory)
        {
            _sliderModelFactory = sliderModelFactory;
            _sliderService = sliderService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _localizedEntityService = localizedEntityService;
            _productModelFactory = productModelFactory;
            _categoryModelFactory = categoryModelFactory;
            _topicModelFactory = topicModelFactory;
            _newsModelFactory = newsModelFactory;
        }

        #endregion

        #region Helpers

        protected virtual void UpdateLocales(Slider slider, SliderModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(slider,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(slider,
                    x => x.Description,
                    localized.Description,
                    localized.LanguageId);
            }
        }

        #endregion

        #region Methods

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            return View(new SliderSearchModel());
        }

        [HttpPost]
        public virtual IActionResult List(SliderSearchModel searchModel)
        {
            //prepare model
            var model = _sliderModelFactory.PrepareSliderListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult Create()
        {
            //prepare model
            var model = _sliderModelFactory.PrepareSliderModel(new SliderModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Create(SliderModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var slider = model.ToEntity<Slider>();
                _sliderService.InsertSlider(slider);

                //locales
                UpdateLocales(slider, model);

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Sliders.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = slider.Id });
            }

            //prepare model
            model = _sliderModelFactory.PrepareSliderModel(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual IActionResult Edit(int id)
        {
            //try to get a slider with the specified id
            var slider = _sliderService.GetSliderById(id);
            if (slider == null)
                return RedirectToAction("List");

            //prepare model
            var model = _sliderModelFactory.PrepareSliderModel(null, slider);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Edit(SliderModel model, bool continueEditing)
        {
            //try to get a slider with the specified id
            var slider = _sliderService.GetSliderById(model.Id);
            if (slider == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                slider = model.ToEntity(slider);
                _sliderService.UpdateSlider(slider);

                //locales
                UpdateLocales(slider, model);

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Sliders.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = slider.Id });
            }

            //prepare model
            model = _sliderModelFactory.PrepareSliderModel(model, slider, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            //try to get a slider with the specified id
            var poll = _sliderService.GetSliderById(id);
            if (poll == null)
                return RedirectToAction("List");

            _sliderService.DeleteSlider(poll);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Sliders.Deleted"));

            return RedirectToAction("List");
        }

        public virtual IActionResult GetItemsByEntityType(SliderEntityTypeEnum entityType)
        {
            if (entityType == SliderEntityTypeEnum.None)
                return Json("");

            if (entityType == SliderEntityTypeEnum.Product)
                return Json(_productModelFactory.PrepareProductListModel(new ProductSearchModel()).Data);

            if (entityType == SliderEntityTypeEnum.Category)
                return Json(_categoryModelFactory.PrepareCategoryListModel(new CategorySearchModel()).Data);

            if (entityType == SliderEntityTypeEnum.Topic)
                return Json(_topicModelFactory.PrepareTopicListModel(new TopicSearchModel()).Data);

            if (entityType == SliderEntityTypeEnum.News)
                return Json(_newsModelFactory.PrepareNewsItemListModel(new NewsItemSearchModel()).Data);

            return Json("");
        }

        #endregion
    }
}