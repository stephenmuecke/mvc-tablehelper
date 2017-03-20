namespace Sandtrap.Web.Models
{

    public interface ITableRow
    {
        /// <summary>
        /// Gets or sets a value indicating if the table row is marked for deletion.
        /// </summary>
        bool IsActive { get; set; }
        /// <summary>
        /// Gets of sets a value indicating if the values in the table row have changed.
        /// </summary>
        bool IsDirty { get; set; }
    }

}
