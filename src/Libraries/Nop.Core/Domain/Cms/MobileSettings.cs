using Nop.Core.Configuration;

namespace Nop.Core.Domain.Cms
{
    /// <summary>
    /// Flutter settings
    /// </summary>
    public class MobileSettings : ISettings
    {
        /// <summary>
        /// Display mobile settings
        /// </summary>
        public bool ActivateMobileSettings { get; set; }

        /// <summary>
        /// Specifies the delay after which the next slide will be displayed (in milliseconds).
        /// </summary>
        public int SliderDelay { get; set; }

        /// <summary>
        /// Gets ot sets if customer can change to dark theme
        /// </summary>
        public bool AllowCustomersToUseDarkTheme { get; set; }

        /// <summary>
        /// Use dark theme as default theme
        /// </summary>
        public bool DarkThemeAsDefault { get; set; }

        /// <summary>
        /// Base color
        /// </summary>
        public string BaseColor { get; set; }

        /// <summary>
        /// Show presentation for new customers
        /// </summary>
        public bool ShowPresentationOnFirstLoad { get; set; }

        /// <summary>
        /// Use current variable to know when need to update mobile resources
        /// </summary>
        public int CurrentLanguageVersion { get; set; }
    }
}
