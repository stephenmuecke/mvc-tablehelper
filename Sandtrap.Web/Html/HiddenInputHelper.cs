using System;
using System.Collections;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;

namespace Sandtrap.Web.Html
{

    /// <summary>
    /// Renders the html for a hidden inputs.
    /// </summary>
    public static class HiddenInputHelper
    {

        #region .Methods 

        /// <summary>
        /// Returns the html for a hidden input(s) of a property.
        /// </summary>
        /// <param name="helper">
        /// The HtmlHelper instance that this method extends.
        /// </param>
        /// <param name="expression">
        /// An expression that identifies the property.
        /// </param>
        /// <remarks>
        /// If the property is a complex type, the methods is called recursively for each property
        /// of the type. Collections and complex types with null value (except those with the 
        /// Required attribute) are ignored.
        /// </remarks>
        public static MvcHtmlString HiddenInputFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression)
        {
            // Get the model metadata
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            string propertyName = ExpressionHelper.GetExpressionText(expression);
            return MvcHtmlString.Create(HiddenInput(metadata, propertyName, true));
        }

        internal static string HiddenInputForMetadata(ModelMetadata metaData, string name, bool includeDefault)
        {
            return HiddenInput(metaData, name, includeDefault);
        }

        #endregion

        #region .Metadata helper methods 

        // Returns a value indicating if the model is a collection
        private static bool IsCollection(this ModelMetadata metaData)
        {
            if (metaData.ModelType == typeof(string))
            {
                return false;
            }
            return typeof(IEnumerable).IsAssignableFrom(metaData.ModelType);
        }

        // Returns the default value for a model
        private static object DefaultValue(this ModelMetadata metaData)
        {
            return metaData.ModelType.IsValueType ? Activator.CreateInstance(metaData.ModelType) : null;
        }

        #endregion

        #region .Html helper methods 

        // Returns the html for all hidden inputs associated with the model
        private static string HiddenInput(ModelMetadata metadata, string propertyName, bool includeDefault)
        {
            StringBuilder html = new StringBuilder();
            if (metadata.ModelType.IsArray && metadata.Model != null)
            {
                // Primarily for database time stamps, this need to called before checking IsComplexType
                // otherwise an endless loop is created
                html.Append(HiddenInput(propertyName, Convert.ToBase64String(metadata.Model as byte[])));
            }
            else if (metadata.IsComplexType)
            {
                foreach (ModelMetadata propertyMetadata in metadata.Properties)
                {
                    if (propertyMetadata.IsCollection() && !propertyMetadata.ModelType.IsArray)
                    {
                        // This would just render the Count and Capacity property of List<T>
                        continue;
                    }
                    if (propertyMetadata.Model == null && propertyMetadata.ModelType != typeof(string) && !propertyMetadata.IsRequired)
                    {
                        // Ignore complex types that are null and do not have the RequiredAttribute
                        continue;
                    }
                    // Recursive call to render a hidden input for the property
                    string prefix = string.Format("{0}.{1}", propertyName, propertyMetadata.PropertyName);
                    html.Append(HiddenInput(propertyMetadata, prefix, false));
                }
            }
            else
            {
                object value = metadata.Model;
                if (value == null && includeDefault)
                {
                    value = metadata.DefaultValue();
                }
                html.Append(HiddenInput(propertyName, value));
            }
            return html.ToString();
        }

        // Returns the html for a hidden input.
        private static string HiddenInput(string propertyName, object value)
        {
            TagBuilder input = new TagBuilder("input");
            input.MergeAttribute("type", "hidden");
            input.MergeAttribute("name", propertyName);
            input.MergeAttribute("value", string.Format("{0}", value));
            return input.ToString();
        }

        #endregion

    }

}