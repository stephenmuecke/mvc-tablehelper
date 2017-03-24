﻿using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sandtrap.Web.Properties;

namespace Sandtrap.Web.DataAnnotations
{

    /// <summary>
    /// An attribute used to determine display options in table generated by the 
    /// <see cref="Sandtrap.Web.Html.TableHelper.TableDisplayFor"/> method.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// If <see cref="DetailsAction"/> or <see cref="EditAction"/> is set and 
    /// <see cref="Controller"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <see cref="DetailsAction"/> or <see cref="EditAction"/> is set and 
    /// the class does not contain the property identified by <see cref="IDProperty"/>.
    /// </exception>
    /// <remarks>
    /// The attribute contains properties used to determine if additional columns are
    /// rendered to display row numbers and hyperlinks for Details and Edit methods.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableDisplayAttribute : Attribute, IMetadataAware
    {

        #region .Constructors 

        /// <summary>
        /// Initialises a new instance of TableDisplayAttribute class with default properties.
        /// </summary>
        /// <remarks>
        /// The default value for  <see cref="IncludeRowNumbers"/> is false.
        /// </remarks>
        public TableDisplayAttribute()
        {
            // Set defaults
            IDProperty = "ID";
        }

        #endregion

        #region .Properties 

        /// <summary>
        /// Gets or sets a value indicating if a table generated by the 
        /// <see cref="Sandtrap.Web.Html.TableHelper.TableDisplayFor"/> method 
        /// renders an additional column to display row numbers.
        /// The default is false.
        /// </summary>   
        public bool IncludeRowNumbers { get; set; }

        /// <summary>
        /// Gets or sets the name of the route controller name used to generate 
        /// view and edit links.
        /// </summary>
        /// <remarks>
        /// The value is ignored if the <see cref="IDProperty"/> and 
        /// <see cref="ViewAction"/> or <see cref="EditAction"/> properties are not set.
        /// </remarks>
        public string Controller { get; set; }

        /// <summary>
        /// Gets or sets the name of the route action method used to view details of 
        /// a row.
        /// </summary>
        /// <remarks>
        /// The value is ignored if the <see cref="Controller"/> and 
        /// <see cref="IDProperty"/> properties are not set.
        /// </remarks>
        public string DetailsAction { get; set; }

        /// <summary>
        /// Gets or sets the name of the route action method used to edit details 
        /// of a row.
        /// </summary>
        /// <remarks>
        /// The value is ignored if the <see cref="Controller"/> and 
        /// <see cref="IDProperty"/> properties are not set.
        /// </remarks>
        public string EditAction { get; set; }

        /// <summary>
        /// Gets or sets the name of the property that provides the value to the  
        /// route action method parameter. 
        /// The default is 'ID'.
        /// </summary>
        /// <remarks>
        /// The value is ignored if the <see cref="Controller"/> and 
        /// <see cref="ViewAction"/> or <see cref="EditAction"/> properties are not set.
        /// </remarks>
        public string IDProperty { get; set; }

        #endregion

        #region .Methods 

        /// <summary>
        /// Adds additional metedata values used to render the html for an readonly table.
        /// </summary>
        /// <param name="metadata">
        /// The meta data.
        /// </param>
        public void OnMetadataCreated(ModelMetadata metadata)
        {
            if (IncludeRowNumbers)
            {
                metadata.AdditionalValues[Resources.TableDisplayAttribute_IncludeRowNumbers] = true;
            }
            if (DetailsAction != null || EditAction != null)
            {
                // Check the controller has been provided
                if (Controller == null)
                {
                    throw new ArgumentNullException("Controller", Resources.TableDisplayAttribute_NullController);
                }
                // Add metadata
                metadata.AdditionalValues.Add(Resources.TableDisplayAttribute_ControllerName, Controller);
                // Check the ID property exists
                ModelMetadata idMetadata = metadata.Properties.FirstOrDefault(m => m.PropertyName == IDProperty);
                if (idMetadata == null)
                {
                    throw new ArgumentException(string.Format(Resources.TableDisplayAttribute_InvalidProperty, metadata.ModelType.Name, IDProperty));
                }
                // Add metadata
                metadata.AdditionalValues.Add(Resources.TableDisplayAttribute_RouteValue, idMetadata.Model);
                if (DetailsAction != null)
                {
                    // Add metadata
                    metadata.AdditionalValues[Resources.TableDisplayAttribute_IncludeDetailsLink] = true;
                    if (metadata.Model != null)
                    {
                        metadata.AdditionalValues[Resources.TableDisplayAttribute_DetailsActionName] = DetailsAction;
                    }
                }
                if (EditAction != null)
                {
                    // Add metadata
                    metadata.AdditionalValues[Resources.TableDisplayAttribute_IncludeEditLink] = true;
                    if (metadata.Model != null)
                    {
                        metadata.AdditionalValues[Resources.TableDisplayAttribute_EditActionName] = EditAction;
                    }
                }
            }
        }

        #endregion

    }

}