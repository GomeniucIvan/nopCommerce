
namespace Nop.Web.Framework.Models
{
    /// <summary>
    /// Represents base nopCommerce entity model
    /// </summary>
    public partial class BaseNopEntityModel : BaseNopModel
    {
        /// <summary>
        /// Gets or sets model identifier
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// Sets if mobile settings are active
        /// </summary>
        public bool ShowMobileSettings { get; set; }
    }
}