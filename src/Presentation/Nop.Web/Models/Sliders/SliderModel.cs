using Nop.Core.Domain.Sliders;

namespace Nop.Web.Models.Sliders
{
    public class SliderModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string PictureUrl { get; set; }
        public string RedirectUrl { get; set; }
        public int? EntityId { get; set; }
        public SliderEntityTypeEnum EntityType { get; set; }
    }
}
