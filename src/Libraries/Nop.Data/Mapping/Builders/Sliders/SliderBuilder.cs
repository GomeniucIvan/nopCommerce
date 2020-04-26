using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Sliders;

namespace Nop.Data.Mapping.Builders.Sliders
{
    /// <summary>
    /// Represents a slider requirement entity builder
    /// </summary>
    public partial class SliderBuilder : NopEntityBuilder<Slider>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {

        }

        #endregion
    }
}
