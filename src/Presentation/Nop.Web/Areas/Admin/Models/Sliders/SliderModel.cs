using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Core.Domain.Sliders;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Sliders
{
    public class SliderModel : BaseNopEntityModel, ILocalizedModel<SliderLocalizedModel>
    {
        #region Ctor

        public SliderModel()
        {
            Locales = new List<SliderLocalizedModel>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Slider.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Slider.Fields.Description")]
        public string Description { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.Slider.Fields.Picture")]
        public int PictureId { get; set; }

        [NopResourceDisplayName("Admin.Slider.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.Slider.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.Slider.Fields.Entity")]
        public SliderEntityTypeEnum EntityType { get; set; }

        public int? EntityId { get; set; }

        public IList<SliderLocalizedModel> Locales { get; set; }

        [NopResourceDisplayName("Admin.Slider.Fields.PictureThumbnailUrl")]
        public string PictureThumbnailUrl { get; set; }

        [NopResourceDisplayName("Admin.Slider.Fields.EntityTypeName")]
        public string EntityTypeName { get; set; }

        #endregion
    }

    public class SliderLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Slider.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Slider.Fields.Description")]
        public string Description { get; set; }
    }
}
