using System;
using System.Web.Mvc;
using Sandtrap.Web.Properties;

namespace Sandtrap.Web.DataAnnotations
{

    /// <summary>
    /// Defines an attribute to generate a html select element for the property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DropDownListAttribute : Attribute, IMetadataAware
    {

        #region .Constructors 

        /// <summary>
        /// Initialises a new instance of DropDownListAttribute with default properties.
        /// </summary>
        public DropDownListAttribute()
        {
            // Set defaults
            OptionLabel = string.Empty;
        }

        /// <summary>
        /// Initialises a new instance of DropDownListAttribute with default properties.
        /// </summary>
        public DropDownListAttribute(string selectListProperty)
        {
            SelectListProperty = selectListProperty;
            // Set defaults
            OptionLabel = string.Empty;
        }

        /// <summary>
        /// Initialises a new instance of DropDownListAttribute with the specified properties.
        /// </summary>
        /// <param name="selectListProperty">
        /// The name of the property used to generate the option elements.
        /// </param>
        /// <param name="optionLabel">
        /// The text for a default empty option.
        /// </param>
        public DropDownListAttribute(string selectListProperty, string optionLabel)
        {
            SelectListProperty = selectListProperty;
            OptionLabel = optionLabel;
        }

        #endregion

        #region .Properties 

        /// <summary>
        /// Gets or sets the name of the property used to generate the option elements.
        /// </summary>
        public string SelectListProperty { get; set; }

        /// <summary>
        /// Gets or sets the text for a default empty option.
        /// </summary>
        public string OptionLabel { get; set; }

        #endregion

        #region .Methods 

        /// <summary>
        /// Adds additional metadata values used to render the select element.
        /// </summary>
        public void OnMetadataCreated(ModelMetadata metadata)
        {
            // Add metadata
            metadata.AdditionalValues[Resources.DropDownListAttribute_SelectListProperty] = SelectListProperty;
            metadata.AdditionalValues[Resources.DropDownListAttribute_OptionLabel] = OptionLabel;
        }

        #endregion

    }

}
