using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sandtrap.Web.Properties;


namespace Sandtrap.Web.DataAnnotations
{

    /// <summary>
    /// Defines an attribute used to render a hyperlink in a readonly table generated 
    /// by the <see cref="Sandtrap.Web.Html.TableHelper.TableDisplayFor"/> method.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// If the <see cref="Controller"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///  If the property the attribute is applied to does not contain the properties defined
    ///  by <see cref="IDProperty"/> or <see cref="DisplayProperty"/>.
    /// </exception>
    /// <remarks>
    /// If the attribute is applied to a property and the property is not a complex type, then
    /// the attribute is ignored.
    /// If the attribute is applied to a complex property, only the hyperlink is rendered and all 
    /// other properties of the complex type are ignored.
    /// </remarks>
    /// <example>
    /// Given the following class definitions:
    /// <code>
    /// public class Organisation
    /// {
    ///     public int ID { get; set; }
    ///     public string Name { get; set; }
    ///     public string Alias { get; set; }
    /// }
    /// [TableLink(Controller = "Project", Action = "Edit")]
    /// public class Project
    /// {
    ///     public int ID { get; set; }
    ///     public string Name { get;  set; }
    ///     [TableLink(Controller = "Organisation", TextProperty = "Alias")]
    ///     public Organisation Client { get; set; }
    /// }
    /// </code>
    /// where the properties of Project are
    /// <para>ID = 17</para>
    /// <para>Name = "Windsor Hospital"</para>
    /// <para>Client.ID = 104</para>
    /// <para>Client.Name = "Acme Developments Pty. Ltd."</para>
    /// <para>Client.Alias = "Acme"</para>
    /// then the @Html.TableDisplayFor() method applied to a collection of projects will 
    /// render <code>&lt;a href="/Project/Edit/17"&gt;Windsor Hospital&lt;/a&gt;</code>
    /// in the tables Name column, and
    /// <code>&lt;a href="/Organisation/Details/104"&gt;Acme&lt;/a&gt;</code>
    /// in the tables Client column.
    /// </example>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class TableLinkAttribute : Attribute, IMetadataAware
    {

        #region .Constructors 

        /// <summary>
        /// Initialises a new instance of a TableLinkAttribute class  with default 
        /// properties.
        /// </summary>
        public TableLinkAttribute()
        {
            // Set defaults
            Action = "Details";
            IDProperty = "ID";
            DisplayProperty = "Name";
        }

        #endregion

        #region .Properties 

        /// <summary>
        /// Gets or sets the name of the route controller.
        /// </summary>
        public string Controller { get; set; }

        /// <summary>
        /// Gets or sets the name of the route action.
        /// The default is 'Details'.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the name of the property that provides the value to the  
        /// route parameter. 
        /// The default is 'ID'.
        /// </summary>
        public string IDProperty { get; set; }

        /// <summary>
        /// Gets or sets the name of the property that provides the value to display 
        /// in the hyperlink.
        /// The default is 'Name'.
        /// </summary>
        public string DisplayProperty { get; set; }

        #endregion

        #region .Methods 

        /// <summary>
        /// Adds additional metadata values used to render the hyperlink.
        /// </summary>
        public void OnMetadataCreated(ModelMetadata metadata)
        {
            // If applied to a property, the property must be a complex type
            if (metadata.ContainerType != null && !metadata.IsComplexType)
            {
                return;
            }
            // Check the controller has been provided
            if (Controller == null)
            {
                throw new ArgumentNullException("Controller", Resources.TableLinkAttribute_NullController);
            }
            // Check the ID and Display properties exist
            ModelMetadata idMetadata = metadata.Properties.FirstOrDefault(m => m.PropertyName == IDProperty);
            if (idMetadata == null)
            {
                throw new ArgumentException(String.Format(Resources.TableLinkAttribute_InvalidProperty, metadata.ModelType.Name, IDProperty));
            }
            ModelMetadata textMetadata = metadata.Properties.FirstOrDefault(m => m.PropertyName == DisplayProperty);
            if (textMetadata == null)
            {
                throw new ArgumentException(String.Format(Resources.TableLinkAttribute_InvalidProperty, metadata.ModelType.Name, DisplayProperty));
            }
            // Add metadata
            metadata.AdditionalValues[Resources.TableLinkAttribute_IncludeLink] = true;
            metadata.AdditionalValues[Resources.TableLinkAttribute_DisplayProperty] = DisplayProperty;
            if (metadata.Model != null)
            {
                // TODO: Area name
                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                var url = urlHelper.Action("Details", "Home", new { id = idMetadata.Model });
                metadata.AdditionalValues[Resources.TableLinkAttribute_LinkUrl] = url;
            }
        }

        #endregion

    }

}
