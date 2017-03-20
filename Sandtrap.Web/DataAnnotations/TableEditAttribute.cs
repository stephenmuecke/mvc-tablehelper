using System;
using System.Linq;
using System.Web.Mvc;
using Sandtrap.Web.Properties;
using Sandtrap.Web.Models;

namespace Sandtrap.Web.DataAnnotations
{

    /// <summary>
    /// An attribute used to determine if a table generated by the 
    /// <see cref="Sandtrap.Web.Html.TableHelper.TableEditorFor"/> method 
    /// renders the html to allow the dynamic addition and/or deletion of rows.
    /// </summary>
    /// <remarks>
    /// When applied, an additional column with buttons to add and/or delete 
    /// rows is rendered in the table. 
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableEditAttribute : Attribute, IMetadataAware
    {

        #region .Constructors 

        /// <summary>
        /// Initialises a new instance of TableEditorAttribute class with default properties.
        /// </summary>
        /// <remarks>
        /// The default values for the <see cref="AllowAdditions"/> and <see cref="AllowDeletions"/> 
        /// properties are true. Applying the attribute renders the html to 
        /// allow both addition and deletion of rows.
        /// </remarks>
        public TableEditAttribute()
        {
            // Set defaults
            AllowAdditions = true;
            AllowDeletions = true;
        }

        #endregion

        #region .Properties 

        /// <summary>
        /// Gets or sets a value indicating if rows can be added to the table.
        /// The default is true.
        /// </summary>   
        public bool AllowAdditions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if rows can be deleted from the table.
        /// The default is true.
        /// </summary>
        public bool AllowDeletions { get; set; }

        /// <summary>
        /// Gets or sets the name of the property that identifies if the model has changed.
        /// </summary>
        /// <remarks>
        /// If the model implements <see cref="Sandtrap.Web.Models.ITableRow"/>, setting this property is not required.
        /// </remarks>
        public string IsDirtyProperty { get; set; }

        /// <summary>
        /// Gets or sets the name of the property that identifies if the model is active.
        /// </summary>
        /// <remarks>
        /// If the model implements <see cref="Sandtrap.Web.Models.ITableRow"/>, setting this property is not required.
        /// </remarks>
        public string IsActiveProperty { get; set; }

        #endregion

        #region .Methods 

        /// <summary>
        /// Adds additional metedata values used to render the html for an editable table.
        /// </summary>
        public void OnMetadataCreated(ModelMetadata metadata)
        {
            if (AllowAdditions)
            {
                metadata.AdditionalValues[Resources.TableEditAttribute_AllowAdditions] = true;
            }
            if (AllowDeletions)
            {
                metadata.AdditionalValues[Resources.TableEditAttribute_AllowDeletions] = true;
            }
            if (typeof(ITableRow).IsAssignableFrom(metadata.ModelType))
            {
                metadata.AdditionalValues[Resources.TableEditAttribute_IsDirtyProperty] = "IsDirty";
                metadata.AdditionalValues[Resources.TableEditAttribute_IsActiveProperty] = "IsActive";
            }
            else
            {
                // Check the IsDirty and IsActive exist and are booleans.
                if (IsDirtyProperty != null)
                {
                    ModelMetadata isDirtyMetadata = metadata.Properties.FirstOrDefault(m => m.PropertyName == IsDirtyProperty);
                    if (isDirtyMetadata == null)
                    {
                        string message = String.Format(Resources.TableEditAttribute_InvalidProperty, metadata.ModelType.Name, IsDirtyProperty);
                        throw new ArgumentNullException(message);
                    }
                    if (isDirtyMetadata.ModelType != typeof(bool))
                    {
                        string message = String.Format(Resources.TableEditAttribute_NotBool, IsDirtyProperty);
                        throw new ArgumentException(message);
                    }
                    metadata.AdditionalValues[Resources.TableEditAttribute_IsDirtyProperty] = IsDirtyProperty;
                }
                if (IsActiveProperty != null)
                {
                    ModelMetadata isActiveMetadata = metadata.Properties.FirstOrDefault(m => m.PropertyName == IsActiveProperty);
                    if (isActiveMetadata == null)
                    {
                        string message = String.Format(Resources.TableEditAttribute_InvalidProperty, metadata.ModelType.Name, IsActiveProperty);
                        throw new ArgumentNullException(message);
                    }
                    if (isActiveMetadata.ModelType != typeof(bool))
                    {
                        string message = String.Format(Resources.TableEditAttribute_NotBool, IsActiveProperty);
                        throw new ArgumentException(message);
                    }
                    metadata.AdditionalValues[Resources.TableEditAttribute_IsActiveProperty] = IsActiveProperty;
                }
            }
        }

        #endregion

    }

}

