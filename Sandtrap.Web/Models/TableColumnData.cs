using System.Collections.Generic;
using System.Web.Mvc;

namespace Sandtrap.Web.Models
{

    /// <summary>
    /// Defines the data used to generate each column in a table.
    /// </summary>
    internal class TableColumnData
    {

        #region .Properties 

        /// <summary>
        /// Gets or sets the fully qualified property name.
        /// </summary>
        internal string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the property requires validation.
        /// </summary>
        internal bool RequireValidation { get; set; }

        /// <summary>
        /// Gets or sets the property metadata.
        /// </summary>
        internal ModelMetadata Metadata { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the column should be excluded in the table.
        /// </summary>
        internal bool IsExcluded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the column should be hidden in the table.
        /// </summary>
        /// <remarks>
        /// A column for the property is not displayed, but a hidden input for the value 
        /// is included in an editable table.
        /// </remarks>
        internal bool IsHidden { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the value of a property should be displayed 
        /// if its value is identical to the previous row.
        /// </summary>
        internal bool NoRepeat { get; set; }

        /// <summary>
        /// Gets of sets the value of the property in the previous row.
        /// </summary>
        internal string PreviousValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if totals for the property should be shown 
        /// in the footer.
        /// </summary>
        internal bool IncludeTotals { get; set; }

        /// <summary>
        /// Gets or sets the total of the property for all rows.
        /// </summary>
        internal decimal ColumnTotal { get; set; }

        /// <summary>
        ///  Gets or sets the format string for the property.
        /// </summary>
        internal string FormatString { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the property is an email address.
        /// </summary>
        internal bool IsEmailAddress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the property should be 
        /// displayed as a hyperlink.
        /// </summary>
        internal bool IsLink { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the property should be displayed as 
        /// readonly in an edit table.
        /// </summary>
        internal bool IsReadonly { get; set; }

        /// <summary>
        /// Gets or sets the property to display for a property which 
        /// is a complex type.
        /// </summary>
        internal string DisplayProperty { get; set; }

        /// <summary>
        /// Gets a value indicating if the property has a DisplayProperty.
        /// </summary>
        internal bool HasDisplayProperty
        {
            get { return DisplayProperty != null; }
        }

        /// <summary>
        /// Gets or sets the name of the property used to generate a datalist for the property.
        /// </summary>
        internal string DataListProperty { get; set; }

        /// <summary>
        /// Gets or sets the name of the property used to generate a SelectList for the property.
        /// </summary>
        internal string SelectListProperty { get; set; }

        /// <summary>
        /// Gets or sets the SelectList associated with the property.
        /// </summary>
        internal IEnumerable<SelectListItem> SelectList { get; set; } 

        #endregion

    }

}