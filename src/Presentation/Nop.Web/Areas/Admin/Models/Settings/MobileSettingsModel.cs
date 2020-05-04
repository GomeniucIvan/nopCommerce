using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Settings
{
    public partial class MobileSettingsModel : BaseNopModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Configuration.Settings.MobileSettings.ActivateMobileSettings")]
        public bool ActivateMobileSettings { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.MobileSettings.SliderDelay")]
        public int SliderDelay { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.MobileSettings.AllowCustomersToSetDarkTheme")]
        public bool AllowCustomersToUseDarkTheme { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.MobileSettings.DarkThemeAsDefault")]
        public bool DarkThemeAsDefault { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.MobileSettings.BaseColor")]
        public string BaseColor { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.MobileSettings.ShowPresentationOnFirstLoad")]
        public bool ShowPresentationOnFirstLoad { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.MobileSettings.CurrentLanguageVersion")]
        public int CurrentLanguageVersion { get; set; }
       
        #endregion
    }
}