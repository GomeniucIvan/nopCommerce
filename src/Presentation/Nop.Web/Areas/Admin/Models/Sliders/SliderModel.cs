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

        [NopResourceDisplayName("Admin.ContentManagement.Sliders.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Sliders.Fields.Description")]
        public string Description { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.ContentManagement.Sliders.Fields.Picture")]
        public int PictureId { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Sliders.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Sliders.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Sliders.Fields.Entity")]
        public SliderEntityTypeEnum EntityTypeId { get; set; }

        public int? EntityId { get; set; }

        public IList<SliderLocalizedModel> Locales { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Sliders.Fields.PictureThumbnailUrl")]
        public string PictureThumbnailUrl { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Sliders.Fields.EntityTypeName")]
        public string EntityTypeName { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.ContentManagement.Sliders.Fields.MobilePictureId")]
        public int MobilePictureId { get; set; }

        #endregion
    }

    public class SliderLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Sliders.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Sliders.Fields.Description")]
        public string Description { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.ContentManagement.Sliders.Fields.MobilePictureId")]
        public int MobilePictureId { get; set; }
    }
}
