using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;

namespace Sandtrap.Web.Models
{

    /// <summary>
    /// Defines the data used to generate a table.
    /// </summary>
    internal class TableData
    {

        #region .Constructors 

        /// <summary>
        /// Initializes a new instance if TableData with default properties.
        /// </summary>
        internal TableData()
        {
            // Initialize defaults
            Columns = new List<TableColumnData>();
            HiddenInputs = new StringBuilder();
        }

        #endregion

        #region .Properties 

        /// <summary>
        /// Gets or sets a value indicating if a readonly table should 
        /// include a column for row numbers.
        /// </summary>
        internal bool IncludeRowNumbers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if a readonly table should 
        /// include a column for a details link.
        /// </summary>
        internal bool IncludeDetailsLink { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if a readonly table should 
        /// include a column for an edit link.
        /// </summary>
        internal bool IncludeEditLink { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if an editable table should 
        /// include a button in the footer to add new rows.
        /// </summary>
        internal bool AllowAdditions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if an editable table should 
        /// include a buttons to delete rows.
        /// </summary>
        internal bool AllowDeletions { get; set; }

        /// <summary>
        /// Gets a value indicating if a column for buttons is required.
        /// </summary>
        internal bool IncludeButtons
        {
            get { return AllowAdditions || AllowDeletions; }
        }

        /// <summary>
        /// Gets or sets a value indicating if the table is editable.
        /// </summary>
        internal bool IsEditMode { get; set; }

        /// <summary>
        /// Gets or sets the name of the property used to determine if 
        /// the data in a row has changed.
        /// </summary>
        internal string IsDirtyProperty { get; set; }

        /// <summary>
        /// Gets or sets the name of the property used to determine if 
        /// the data in a row is marked for deletion.
        /// </summary>
        internal string IsActiveProperty { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the row is marked for deletion.
        /// </summary>
        internal bool IsArchived { get; set; }

        /// <summary>
        /// Gets or sets the data for each column in the table.
        /// </summary>
        internal List<TableColumnData> Columns { get; set; }

        /// <summary>
        /// Gets or sets the html for hidden inputs associated with the row.
        /// </summary>
        internal StringBuilder HiddenInputs { get; set; }

        /// <summary>
        /// Gets or sets the collection of SelectLists used in the table.
        /// </summary>
        internal Dictionary<string, IEnumerable<SelectListItem>> SelectLists { get; set; }

        /// <summary>
        /// Gets or sets the collection of DataLists used in the table.
        /// </summary>
        internal Dictionary<string, IEnumerable<string>> DataLists { get; set; }

        #endregion

    }
}