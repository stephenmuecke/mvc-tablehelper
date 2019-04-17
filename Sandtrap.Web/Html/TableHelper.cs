using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Sandtrap.Extensions;
using Sandtrap.Web.Models;
using Sandtrap.Web.Properties;

namespace Sandtrap.Web.Html
{

    /// <summary>
    /// Renders the html for a table.
    /// </summary>
    public static class TableHelper
    {

        #region .Methods 

        /// <summary>
        /// Returns the html to render a readonly display table for a collection.
        /// </summary>
        /// <param name="helper">
        /// The HtmlHelper instance that this method extends.
        /// </param>
        /// <param name="expression">
        /// An expression that identifies the property to display.
        /// </param>
        /// <exception cref="NullReferenceException">
        /// The property is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The property is a grouped collection (<see cref="T:System.Collections.Generic.IDictionary<TKey, TValue>"/> or
        /// <see cref="System.Linq.IGrouping<TKey, TElement>"/> or <see cref="System.Linq.ILookup<TKey, TElement>"/>)
        /// </exception>
        public static MvcHtmlString TableDisplayFor<TModel>(this HtmlHelper<TModel> helper, Expression<Func<TModel, IEnumerable>> expression) 
        {
            // Get the model metadata
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            // Get the fully qualified name of the property
            string fieldName = ExpressionHelper.GetExpressionText(expression);
            if (metadata.Model == null)
            {
                string message = Resources.Table_NullCollection;
                throw new NullReferenceException(message);
            }
            // Check types that wont make any sense to render in a table (IDictionary, IGrouping, Lookup)
            if (metadata.ModelType == typeof(string))
            {
                string message = Resources.Table_UnresolvedType;
                throw new ArgumentException(message);
            }
            if (IsGrouped(metadata.Model))
            {
                string message = Resources.Table_GroupedCollection;
                throw new ArgumentException(message);
            }
            // Get the type in the collection
            Type type = GetCollectionType(metadata.Model as IEnumerable);
            if (type == null)
            {
                string message = Resources.Table_UnresolvedType;
                throw new ArgumentException(message);
            }
            // Get the metadata of the type
            ModelMetadata typeMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, type);
            // Initialize table data
            TableData tableData = new TableData()
            {
                // TODO: Add properties for AllowAdditions and AllowDeletions (EditTable)
                IncludeRowNumbers = typeMetadata.IncludeRowNumbers(),
                IncludeDetailsLink = typeMetadata.IncludeDetailsLink(),
                IncludeEditLink = typeMetadata.IncludeEditLink(),
                IsEditMode = false
            };
            StringBuilder html = new StringBuilder();
            // Add table header
            string header = TableHeader(helper, typeMetadata, tableData);
            html.Append(header);
            // Add table body
            IEnumerable collection = metadata.Model as IEnumerable;
            string body = ReadonlyTableBody(helper, collection, type, tableData);
            html.Append(body);
            // Add table footer (only if there is something to show)
            if (tableData.Columns.Any(c => c.IncludeTotals))
            {
                html.Append(TableFooter(tableData));
            }
            // Create the table
            TagBuilder table = new TagBuilder("table");
            table.MergeAttribute("id", HtmlHelper.GenerateIdFromName(fieldName));
            table.AddCssClass("readonly-table");
            table.InnerHtml = html.ToString();
            // Return the html
            return MvcHtmlString.Create(table.ToString());
        }

        /// <summary>
        /// Returns the html to render an editable table for a collection.
        /// </summary>
        /// <param name="helper">
        /// The HtmlHelper instance that this method extends.
        /// </param>
        /// <param name="expression">
        /// An expression that identifies the property to display.
        /// </param>
        /// <exception cref="InvalidCastException">
        /// If the property does not implement <see cref="T:System.Collections.IEnumerable"/> or 
        /// the type in the collection could not be resolved.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If the type in the collection does not have a parameterless constructor
        /// </exception>
        public static MvcHtmlString TableEditorFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression)
        {
            // Get the model metadata
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            // Get the fully qualified name of the property
            string propertyName = ExpressionHelper.GetExpressionText(expression);
            // Get the collection to render in the table
            if (metadata.Model == null)
            {
                string message = Resources.Table_NullCollection;
                throw new NullReferenceException(message);
            }
            // Check types that wont make any sense to render in a table (IDictionary, IGrouping, Lookup)
            if (metadata.ModelType == typeof(string))
            {
                string message = Resources.Table_UnresolvedType;
                throw new ArgumentException(message);
            }
            if (IsGrouped(metadata.Model))
            {
                string message = Resources.Table_GroupedCollection;
                throw new ArgumentException(message);
            }
            // Get the type in the collection
            Type type = GetCollectionType(metadata.Model as IEnumerable);
            if (type == null)
            {
                string message = Resources.Table_UnresolvedType;
                throw new ArgumentException(message);
            }
            // Get the metadata of the type
            ModelMetadata typeMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, type);
            // Check the type has a parameterless constructor
            if (typeMetadata.ModelType.GetConstructor(Type.EmptyTypes) == null)
            {
                // The hidden new row cannot be rendered because the type needs to be initialised
                // so default values are rendered
                string message = Resources.Table_NoTypeConstructor;
                throw new InvalidOperationException(message);
            }
            // Create a new instance so default values are rendered for the new row
            object instance = Activator.CreateInstance(typeMetadata.ModelType);
            ModelMetadata itemMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => instance, type);
            // Get any datalists and selectlists
            Dictionary<string, IEnumerable<string>> dataLists = GetDataLists(itemMetadata, helper.ViewData.Model);
            Dictionary<string, IEnumerable<SelectListItem>> selectLists = GetSelectLists(itemMetadata, helper.ViewData.Model);
            // Initialize table data
            TableData tableData = new TableData()
            {
                AllowAdditions = typeMetadata.AllowAdditions(),
                AllowDeletions = typeMetadata.AllowDeletions(),
                IsDirtyProperty = typeMetadata.IsDirtyProperty(),
                IsActiveProperty = typeMetadata.IsActiveProperty(),
                DataLists = dataLists,
                SelectLists = selectLists,
                IsEditMode = true
            };
            // Add table components
            StringBuilder html = new StringBuilder();
            // Add table header
            string header = TableHeader(helper, typeMetadata, tableData);
            html.Append(header);
            // Add table body
            IEnumerable collection = metadata.Model as IEnumerable;
            string body = EditableTableBody(helper, collection, type, propertyName, tableData);
            html.Append(body);
            // Add the hidden table body for generating new rows
            string hiddenBody = HiddenTableBody(helper, itemMetadata, propertyName, tableData);
            html.Append(hiddenBody);
            // Add table footer (only if there is something to show)
            if (tableData.IncludeButtons || tableData.Columns.Any(c => c.IncludeTotals))
            {
                string footer = TableFooter(tableData);
                html.Append(footer);
            }
            // Create the table
            TagBuilder table = new TagBuilder("table");
            table.AddCssClass("edit-table");
            table.MergeAttribute("id", HtmlHelper.GenerateIdFromName(propertyName));
            if (tableData.IsActiveProperty != null)
            {
                table.MergeAttribute("data-isactiveproperty", tableData.IsActiveProperty);
            }
            if (tableData.IsDirtyProperty != null)
            {
                table.MergeAttribute("data-isdirtyproperty", tableData.IsDirtyProperty);
            }
            table.InnerHtml = html.ToString();
            // Add any datalists
            if (tableData.DataLists.Any())
            {
                html = new StringBuilder();
                html.Append(table.ToString());
                foreach (var item in tableData.DataLists)
                {
                    string id = String.Format("{0}-datalist", item.Key).ToLower();
                    string datalist = DataList(id, item.Value);
                    html.Append(datalist);
                }
                return MvcHtmlString.Create(html.ToString());
            }
            // Return the html
            return MvcHtmlString.Create(table.ToString());
        }

        #endregion

        #region .Metadata helper methods 

        // Returns the type in the collection
        private static Type GetCollectionType(IEnumerable collection)
        {
            Type type = collection.GetType();
            if (type.IsGenericType)
            {
                return type.GetInterfaces().Where(t => t.IsGenericType)
                    .Where(t => t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    .Single().GetGenericArguments()[0];
            }
            if (type.IsArray)
            {
                return type.GetElementType();
            }
            // TODO: Can we get here?
            return null;
        }

        // Determines if an object is a grouped collection (IDictionary, IGrouping, ILookup)
        private static bool IsGrouped(dynamic collection)
        {
            return CheckGrouping(collection);
        }

        private static bool CheckGrouping<TKey, TValue>(IDictionary<TKey, TValue> collection)
        {
            return true;
        }

        private static bool CheckGrouping<TKey, TElement>(IEnumerable<IGrouping<TKey, TElement>> collection)
        {
            return true;
        }

        private static bool CheckGrouping<TKey, TElement>(ILookup<TKey, TElement> collection)
        {
            return true;
        }

        private static bool CheckGrouping(object collection)
        {
            return false;
        }

        // Returns the datalists for the model
        private static Dictionary<string, IEnumerable<string>> GetDataLists(ModelMetadata metadata, object parentModel)
        {
            var dataLists = metadata.Properties.Where(x => x.AdditionalValues.ContainsKey(Resources.DataListAttribute_DataListProperty)).Select(x => new
            {
                propertyName = x.PropertyName,
                optionsPropertyName = x.AdditionalValues[Resources.DataListAttribute_DataListProperty]
            });
            if (dataLists.Any())
            {
                Dictionary<string, IEnumerable<string>> optionsLists = new Dictionary<string, IEnumerable<string>>();
                Type parentType = parentModel.GetType();
                var parentMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => parentModel, parentType);
                foreach (var item in dataLists)
                {
                    var optionsMetadata = parentMetadata.Properties.FirstOrDefault(x => x.PropertyName == (string)item.optionsPropertyName);
                    if (optionsMetadata == null)
                    {
                        string message = String.Format(Resources.DataListAttribute_MissingDataList, item.optionsPropertyName);
                        throw new MissingFieldException(message); // TODO: What is the correct exception
                    }
                    if (optionsMetadata.Model == null)
                    {
                        string message = String.Format(Resources.DataListAttribute_NullDataList, item.optionsPropertyName);
                        throw new NullReferenceException(message);
                    }
                    IEnumerable<string> options = optionsMetadata.Model as IEnumerable<string>;
                    if (options == null)
                    {
                        string message = String.Format(Resources.DataListAttribute__InvalidDataList, item.optionsPropertyName);
                        throw new InvalidCastException(message); // TODO: What is the correct exception
                    }
                    if (!options.Any())
                    {
                        string message = String.Format(Resources.DataListAttribute_EmptyDataList, item.optionsPropertyName);
                        throw new ArgumentException(message); // TODO: What is the correct exception
                    }
                    optionsLists.Add(item.propertyName, options);
                }
                return optionsLists;
            }
            return new Dictionary<string, IEnumerable<string>>();
        }

        // Returns the SelectLists for the model
        private static Dictionary<string, IEnumerable<SelectListItem>> GetSelectLists(ModelMetadata metadata, object parentModel)
        {
            // Get datalists
            var selectLists = metadata.Properties.Where(x => x.AdditionalValues.ContainsKey(Resources.DropDownListAttribute_SelectListProperty)).Select(x => new
            {
                propertyName = x.PropertyName,
                optionsPropertyName = x.AdditionalValues[Resources.DropDownListAttribute_SelectListProperty],
                optionLabel = x.AdditionalValues[Resources.DropDownListAttribute_OptionLabel]
            });
            if (selectLists.Any())
            {
                Dictionary<string, IEnumerable<SelectListItem>> optionsLists = new Dictionary<string, IEnumerable<SelectListItem>>();
                Type parentType = parentModel.GetType();
                var parentMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => parentModel, parentType);
                foreach (var item in selectLists)
                {
                    var optionsMetadata = parentMetadata.Properties.FirstOrDefault(x => x.PropertyName == (string)item.optionsPropertyName);
                    if (optionsMetadata == null)
                    {
                        string message = String.Format(Resources.DropDownListAttribute_MissingSelectList, item.optionsPropertyName);
                        throw new MissingFieldException(message); // TODO: What is the correct exception
                    }
                    if (optionsMetadata.Model == null)
                    {
                        string message = String.Format(Resources.DropDownListAttribute_NullSelectList, item.optionsPropertyName);
                        throw new NullReferenceException(message);
                    }
                    IEnumerable<SelectListItem> options = optionsMetadata.Model as IEnumerable<SelectListItem>;
                    if (options == null)
                    {
                        string message = String.Format(Resources.DropDownListAttribute__InvalidSelectList, item.optionsPropertyName);
                        throw new InvalidCastException(message); // TODO: What is the correct exception
                    }
                    if (!options.Any())
                    {
                        string message = String.Format(Resources.DropDownListAttribute_EmptySelectList, item.optionsPropertyName);
                        throw new ArgumentException(message); // TODO: What is the correct exception
                    }
                    // Build new option list
                    List<SelectListItem> selectList = new List<SelectListItem>();
                    if (item.optionLabel != null)
                    {
                        selectList.Add(new SelectListItem() { Value = "", Text = (string)item.optionLabel });
                    }
                    foreach (var option in options)
                    {
                        selectList.Add(new SelectListItem() { Value = option.Value, Text = option.Text });
                    }
                    optionsLists.Add(item.propertyName, selectList);
                }
                return optionsLists;
            }
            return new Dictionary<string, IEnumerable<SelectListItem>>();
        }

        // Determines if the IncludeRowNumbers property of TableDisplayAttribute has been set
        private static bool IncludeRowNumbers(this ModelMetadata metadata)
        {
            return metadata.AdditionalValues.ContainsKey(Resources.TableDisplayAttribute_IncludeRowNumbers);
        }

        // Determines if the IncludeDetailsLink property of TableDisplayAttribute has been set
        private static bool IncludeDetailsLink(this ModelMetadata metadata)
        {
            return metadata.AdditionalValues.ContainsKey(Resources.TableDisplayAttribute_IncludeDetailsLink);
        }

        // Determines if the IncludeEditLink property of TableDisplayAttribute has been set
        private static bool IncludeEditLink(this ModelMetadata metadata)
        {
            return metadata.AdditionalValues.ContainsKey(Resources.TableDisplayAttribute_IncludeEditLink);
        }

        // Determines if the property should be displayed as a link.
        private static bool IsLink(this ModelMetadata metadata)
        {
            return metadata.AdditionalValues.ContainsKey(Resources.TableLinkAttribute_IncludeLink);
        }

        // Determines if the HiddenInputAttribute has been applied to the property
        private static bool IsHidden(this ModelMetadata metaData)
        {
            return metaData.TemplateHint == "HiddenInput";
        }

        // Deterimines if a property should be excluded in a table
        private static bool IsExcluded(this ModelMetadata metadata)
        {
            return metadata.AdditionalValues.ContainsKey(Resources.TableColumnAttribute_Exclude);
        }

        // Deterimines if a property is readonly in a table
        private static bool IsReadOnly(this ModelMetadata metadata)
        {
            if (metadata.IsReadOnly)
            {
                return true;
            }
            return metadata.AdditionalValues.ContainsKey(Resources.TableColumnAttribute_Readonly);
        }

        // Deterimines if a complex object has a display property
        private static bool HasDisplayProperty(this ModelMetadata metadata)
        {
            return metadata.AdditionalValues.ContainsKey(Resources.TableColumnAttribute_DisplayProperty);
        }

        // Deterimines if a totals should be displayed for the property in the table footer
        private static bool IncludeTotals(this ModelMetadata metadata)
        {
            return metadata.AdditionalValues.ContainsKey(Resources.TableColumnAttribute_IncludeTotal);
        }

        // Determines if a property should be displayed if the preceding table row contains the same value
        private static bool NoRepeat(this ModelMetadata metadata)
        {
            return metadata.AdditionalValues.ContainsKey(Resources.TableColumnAttribute_NoRepeat);
        }

        // Determines if a edit table can allow rows to be added dynamically
        private static bool AllowAdditions(this ModelMetadata metadata)
        {
            return metadata.AdditionalValues.ContainsKey(Resources.TableEditAttribute_AllowAdditions);
        }

        // Determines if a edit table can allow rows to be deleted dynamically
        private static bool AllowDeletions(this ModelMetadata metadata)
        {
            return metadata.AdditionalValues.ContainsKey(Resources.TableEditAttribute_AllowDeletions);
        }

        // Returns the name of the property used to determine if an object is active
        public static string IsActiveProperty(this ModelMetadata metadata)
        {
            if (metadata.AdditionalValues.ContainsKey(Resources.TableEditAttribute_IsActiveProperty))
            {
                return (string)metadata.AdditionalValues[Resources.TableEditAttribute_IsActiveProperty];
            }
            return null;
        }

        // Returns the name of the property used to determine if an object is dirty
        public static string IsDirtyProperty(this ModelMetadata metadata)
        {
            if (metadata.AdditionalValues.ContainsKey(Resources.TableEditAttribute_IsDirtyProperty))
            {
                return (string)metadata.AdditionalValues[Resources.TableEditAttribute_IsDirtyProperty];
            }
            return null;
        }

        // Returns the name of the property used to generate a <datalist>
        public static string DataListProperty(this ModelMetadata metadata)
        {
            if (metadata.AdditionalValues.ContainsKey(Resources.DataListAttribute_DataListProperty))
            {
                return (string)metadata.AdditionalValues[Resources.DataListAttribute_DataListProperty];
            }
            return null;
        }

        // Returns the name of the property used to generate a <select>
        public static string SelectListProperty(this ModelMetadata metadata)
        {
            if (metadata.AdditionalValues.ContainsKey(Resources.DropDownListAttribute_SelectListProperty))
            {
                return (string)metadata.AdditionalValues[Resources.DropDownListAttribute_SelectListProperty];
            }
            return null;
        }

        // Returns the formatted value of the property
        private static string GetFormattedValue(this ModelMetadata metadata)
        {
            if (metadata.Model == null)
            {
                return metadata.NullDisplayText;
            }
            Type type = Nullable.GetUnderlyingType(metadata.ModelType) ?? metadata.ModelType;

            if (type == typeof(bool))
            {
                // If we got this far it has a value
                return (bool)metadata.Model ? "Yes" : "No";
            }
            if (type.IsEnum)
            {
                return ((Enum)metadata.Model).ToDisplayName();
            }
            // Return the formatted value
            string formatString = metadata.DisplayFormatString ?? "{0}";
            return string.Format(formatString, metadata.Model);
        }

        #endregion

        #region .Html helper methods 

        // Returns the <thead> element
        private static string TableHeader(HtmlHelper helper, ModelMetadata metaData, TableData data)
        {
            // Build <tr>
            TagBuilder row = new TagBuilder("tr");
            row.InnerHtml = TableHeaderCells(helper, metaData, data);
            // Build <thead>
            TagBuilder head = new TagBuilder("thead");
            head.InnerHtml = row.ToString();
            // Return html
            return head.ToString();
        }

        // Returns the collection of <th> elements for the header
        private static string TableHeaderCells(HtmlHelper helper, ModelMetadata metaData, TableData data, bool isRecursive = false)
        {
            // Build the html for each cell
            StringBuilder html = new StringBuilder();
            bool isReadonly = !data.IsEditMode;
            if (isReadonly && !isRecursive && data.IncludeRowNumbers)
            {
                string rowNumberCell = HeaderCell("No.");
                html.Append(rowNumberCell);
            }
            string linkProperty = null;
            if (isReadonly && metaData.IsLink())
            {
                linkProperty = (string)metaData.AdditionalValues[Resources.TableLinkAttribute_DisplayProperty];
            }
            int index = 0;
            foreach (ModelMetadata propertyMetadata in metaData.Properties)
            {
                TableColumnData column = new TableColumnData();
                // for debugging
                column.PropertyName = propertyMetadata.PropertyName;
                if (propertyMetadata.IsExcluded())
                {
                    column.IsExcluded = true;
                    data.Columns.Add(column);
                    continue;
                }
                if (propertyMetadata.PropertyName == data.IsActiveProperty || propertyMetadata.PropertyName == data.IsDirtyProperty)
                {
                    column.IsHidden = true;
                    data.Columns.Add(column);
                    continue;
                }
                // Do we need to include the property
                if (propertyMetadata.IsHidden())
                {
                    column.IsHidden = true;
                    data.Columns.Add(column);
                    continue;
                }
                if (isReadonly && propertyMetadata.PropertyName == linkProperty)
                {
                    column.IsLink = true;
                }
                else if (isReadonly && propertyMetadata.DataTypeName == "EmailAddress")
                {
                    column.IsEmailAddress = true;
                }
                // Is the value omitted if its the same as the previous row
                if (propertyMetadata.NoRepeat())
                {
                    // This only makes sense if its the first column or the
                    // previous column is NoRepeat
                    if (index == 0 || data.Columns[index - 1].NoRepeat)
                    {
                        column.NoRepeat = true;
                    }
                }
                if (!isReadonly)
                {
                    column.DataListProperty = propertyMetadata.DataListProperty();
                    column.SelectListProperty = propertyMetadata.SelectListProperty();
                }

                if (propertyMetadata.IsComplexType)
                {
                    if (typeof(IEnumerable).IsAssignableFrom(propertyMetadata.ModelType))
                    {
                        // Its a collection within the collection so ignore.
                        column.IsExcluded = true;
                        data.Columns.Add(column);
                        continue;
                    }
                    // Is the property displayed as a hyperlink
                    if (isReadonly && propertyMetadata.IsLink())
                    {
                        column.IsLink = true;
                        data.Columns.Add(column);
                        string cell = HeaderCell(propertyMetadata.GetDisplayName());
                        html.Append(cell);
                    }
                    // Does the property have a display property
                    else if (propertyMetadata.HasDisplayProperty())
                    {
                        column.DisplayProperty = (string)propertyMetadata.AdditionalValues[Resources.TableColumnAttribute_DisplayProperty];
                        ModelMetadata displayPropertyMetadata = propertyMetadata.Properties.First(m => m.PropertyName == column.DisplayProperty);
                        if (propertyMetadata.IsReadOnly())
                        {
                            column.IsReadonly = true;
                        }
                        data.Columns.Add(column);
                        string cell = HeaderCell(displayPropertyMetadata.GetDisplayName());
                        html.Append(cell);
                    }
                    else
                    {
                        // Add columns for each property of the type (recursive call)
                        html.Append(TableHeaderCells(helper, propertyMetadata, data, true));
                    }
                }
                else
                {
                    // Are column totals required
                    if (propertyMetadata.IncludeTotals())
                    {
                        column.IncludeTotals = true;
                        string formatString = propertyMetadata.DisplayFormatString;
                        if (formatString == null)
                        {
                            if (propertyMetadata.ModelType.IsIntegralNumeric())
                            {
                                formatString = "{0:N0}";
                            }
                            else
                            {
                                formatString = "{0:N2}";
                            }
                        }
                        column.FormatString = formatString;
                    }
                    if (propertyMetadata.IsReadOnly() || metaData.IsReadOnly())
                    {
                        column.IsReadonly = true;
                    }
                    else
                    {
                        column.RequireValidation = true;
                    }
                    data.Columns.Add(column);
                    string headerCell = HeaderCell(propertyMetadata.GetDisplayName());
                    html.Append(headerCell);
                }
                index++;
            }
            if (!isRecursive)
            {
                if (data.IncludeDetailsLink)
                {
                    string cell = HeaderCell(null);
                    html.Append(cell);
                }
                if (data.IncludeEditLink)
                {
                    string cell = HeaderCell(null);
                    html.Append(cell);
                }
                if (data.IncludeButtons)
                {
                    string buttonHeader = HeaderCell(string.Empty, "button-header-cell");
                    html.Append(buttonHeader);
                }
                if (data.IsEditMode)
                {
                    // An extra column is required for the hidden inputs 
                    string hiddenHeader = HeaderCell(string.Empty, "hidden-header-cell");
                    html.Append(hiddenHeader);
                }
            }
            return html.ToString();
        }

        // Returns the <tbody> element
        private static string ReadonlyTableBody(HtmlHelper helper, IEnumerable collection, Type type, TableData data)
        {
            StringBuilder html = new StringBuilder();
            int? rowNumber = null;
            if (data.IncludeRowNumbers)
            {
                rowNumber = 0;
            }
            foreach (var item in collection)
            {
                rowNumber++;
                int columnNumber = 0;
                ModelMetadata itemMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => item, type);
                TagBuilder row = new TagBuilder("tr");
                row.InnerHtml = ReadonlyTableBodyCells(helper, itemMetadata, data, rowNumber, ref columnNumber);
                html.Append(row.ToString());
            }
            // Build table body
            TagBuilder body = new TagBuilder("tbody");
            body.InnerHtml = html.ToString();
            // Return the html
            return body.ToString();
        }

        // Returns the <td> elements for each table row
        private static string ReadonlyTableBodyCells(HtmlHelper helper, ModelMetadata metadata, TableData data, int? rowNumber, ref int columnIndex, bool isRecursive = false)
        {
            // Build the html for each column
            string blankCell = TableCell(null);
            StringBuilder html = new StringBuilder();
            if (!isRecursive && rowNumber.HasValue)
            {
                string cell = TableCell(rowNumber.Value.ToString());
                html.Append(cell);
            }
            foreach (ModelMetadata propertyMetadata in metadata.Properties)
            {
                TableColumnData column = data.Columns[columnIndex];
                if (column.IsExcluded || column.IsHidden)
                {
                    // Nothing to display
                    columnIndex++;
                    continue;
                }
                if (column.IncludeTotals)
                {
                    // Update running total
                    if (column.HasDisplayProperty)
                    {
                        column.ColumnTotal += Convert.ToDecimal(propertyMetadata.Properties
                            .FirstOrDefault(m => m.PropertyName == column.DisplayProperty).Model);
                    }
                    else
                    {
                        column.ColumnTotal += Convert.ToDecimal(propertyMetadata.Model);
                    }
                }
                if (propertyMetadata.IsComplexType && !column.IsLink && !column.HasDisplayProperty)
                {
                    if (typeof(IEnumerable).IsAssignableFrom(propertyMetadata.ModelType))
                    {
                        // Its a collection within the collection so ignore.
                        continue;
                    }
                    else
                    {
                        // Add cells for each property of the type (recursive call)
                        string cells = ReadonlyTableBodyCells(helper, propertyMetadata, data, null, ref columnIndex, true);
                        html.Append(cells);
                    }
                }
                else
                {
                    // Get the display text
                    string text = null;
                    if (propertyMetadata.Model != null && column.IsEmailAddress)
                    {
                        // Build link
                        TagBuilder link = new TagBuilder("a");
                        link.MergeAttribute("href", string.Format("mailto:{0}", propertyMetadata.Model));
                        link.InnerHtml = string.Format("{0}", propertyMetadata.Model);
                        text = link.ToString();
                    }
                    else if (propertyMetadata.Model != null && column.IsLink)
                    {
                        // The link can be applied at either class or property level
                        ModelMetadata linkMetadata = metadata;
                        if (propertyMetadata.AdditionalValues.ContainsKey(Resources.TableLinkAttribute_IncludeLink))
                        {
                            linkMetadata = propertyMetadata;
                        }
                        string controller = (string)linkMetadata.AdditionalValues[Resources.TableLinkAttribute_ControllerName];
                        string action = (string)linkMetadata.AdditionalValues[Resources.TableLinkAttribute_ActionName];
                        object routeValue = linkMetadata.AdditionalValues[Resources.TableLinkAttribute_RouteValue];
                        string displayProperty = (string)linkMetadata.AdditionalValues[Resources.TableLinkAttribute_DisplayProperty];
                        ModelMetadata displayPropertyMetadata = linkMetadata.Properties.FirstOrDefault(m => m.PropertyName == displayProperty);
                        string displayText = displayPropertyMetadata.GetFormattedValue();
                        MvcHtmlString link = helper.ActionLink(displayText, action, controller, new { id = routeValue }, null);
                        text = link.ToString();
                    }
                    else if (propertyMetadata.Model != null && column.HasDisplayProperty)
                    {
                        string formatString = column.FormatString ?? "{0}";
                        text = string.Format(formatString, propertyMetadata.Properties.FirstOrDefault(m => m.PropertyName == column.DisplayProperty).Model);
                    }
                    else
                    {
                        text = GetFormattedValue(propertyMetadata);
                    }
                    if (column.NoRepeat)
                    {
                        if (text == column.PreviousValue)
                        {
                            text = string.Empty;
                        }
                        else
                        {
                            column.PreviousValue = text;
                        }
                    }
                    string cell = TableCell(text);
                    html.Append(cell);
                    columnIndex++;
                }
            }
            if (!isRecursive)
            {
                if (data.IncludeDetailsLink || data.IncludeEditLink)
                {
                    // TODO: The controller can be set when building the header
                    string controller = (string)metadata.AdditionalValues[Resources.TableDisplayAttribute_ControllerName];

                    object routeValue = metadata.AdditionalValues[Resources.TableDisplayAttribute_RouteValue];
                    if (data.IncludeDetailsLink)
                    {
                        string action = (string)metadata.AdditionalValues[Resources.TableDisplayAttribute_DetailsActionName];
                        MvcHtmlString link = helper.ActionLink("Details", action, controller, new { id = routeValue }, new { @class = "details-link" });
                        string cell = TableCell(link.ToString());
                        html.Append(cell);
                    }
                    if (data.IncludeEditLink)
                    {
                        string action = (string)metadata.AdditionalValues[Resources.TableDisplayAttribute_EditActionName];
                        MvcHtmlString link = helper.ActionLink("Edit", action, controller, new { id = routeValue }, new { @class = "edit-link" });
                        string cell = TableCell(link.ToString());
                        html.Append(cell);
                    }
                }
            }
            // Return the html
            return html.ToString();
        }

        // Returns the <tbody> element
        private static string EditableTableBody(HtmlHelper helper, IEnumerable collection, Type type, string propertyName, TableData data)
        {
            StringBuilder html = new StringBuilder();
            int rowNumber = 0;
            foreach (var item in collection)
            {
                int columnNumber = 0;
                string prefix = string.Format("{0}[{1}]", propertyName, rowNumber.ToString());
                ModelMetadata itemMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => item, type);
                // Add edit row
                TagBuilder editRow = new TagBuilder("tr");
                editRow.AddCssClass("edit-row");
                // Check for an archived property
                if (data.IsActiveProperty != null)
                {
                    var isActiveMetadata = itemMetadata.Properties.FirstOrDefault(m => m.PropertyName == data.IsActiveProperty);
                    if (!(bool)isActiveMetadata.Model)
                    {
                        editRow.AddCssClass("archived");
                        data.IsArchived = true;
                    }
                    else
                    {
                        data.IsArchived = false;
                    }
                }
                data.HiddenInputs.Clear();
                string cells = EditableTableBodyCells(helper, itemMetadata, prefix, ref columnNumber, data);
                editRow.InnerHtml = cells;
                html.Append(editRow.ToString());
                // Add validation row
                TagBuilder validationRow = new TagBuilder("tr");
                validationRow.AddCssClass("validation-row");
                validationRow.InnerHtml = ValidationRowCells(helper, data);
                html.Append(validationRow.ToString());
                // Increment row number
                rowNumber++;
            }
            // Build table boddy
            TagBuilder body = new TagBuilder("tbody");
            body.InnerHtml = html.ToString();
            // Return the html
            return body.ToString();
        }

        // Returns the hidden <tbody> element for generating new rows
        private static string HiddenTableBody(HtmlHelper helper, ModelMetadata metadata, string propertyName, TableData data)
        {
            StringBuilder html = new StringBuilder();
            string prefix = string.Format("{0}[#]", propertyName);
            int columnNumber = 0;
            // Add edit row
            TagBuilder editRow = new TagBuilder("tr");
            data.HiddenInputs.Clear();
            string cells = EditableTableBodyCells(helper, metadata, prefix, ref columnNumber, data);
            editRow.AddCssClass("edit-row");
            editRow.InnerHtml = cells;
            html.Append(editRow.ToString());
            TagBuilder validationRow = new TagBuilder("tr");
            validationRow.AddCssClass("validation-row");
            validationRow.InnerHtml = ValidationRowCells(helper, data);
            html.Append(validationRow.ToString());
            // Build table boddy
            TagBuilder body = new TagBuilder("tbody");
            body.MergeAttribute("style", "display:none;");
            body.InnerHtml = html.ToString();
            // Return the html
            return body.ToString();
        }

        // Returns the <td> elements for each editable table row
        private static string EditableTableBodyCells(HtmlHelper helper, ModelMetadata metadata,
            string prefix, ref int columnIndex, TableData data, bool isRecursive = false)
        {
            // Build the html for each cell
            StringBuilder html = new StringBuilder();
            foreach (ModelMetadata propertyMetadata in metadata.Properties)
            {
                TableColumnData column = data.Columns[columnIndex];
                if (column.IsExcluded)
                {
                    // Nothing to do
                    columnIndex++;
                    continue;
                }
                column.Metadata = propertyMetadata;
                column.PropertyName = string.Format("{0}.{1}", prefix, propertyMetadata.PropertyName);
                if (data.IsActiveProperty == propertyMetadata.PropertyName)
                {
                    object htmlAttributes = new { id = "", @class = data.IsActiveProperty };
                    MvcHtmlString hiddenInput = helper.Hidden(column.PropertyName, column.Metadata.Model, htmlAttributes);
                    data.HiddenInputs.Append(hiddenInput);
                    columnIndex++;
                    continue;
                }
                if (data.IsDirtyProperty == propertyMetadata.PropertyName)
                {
                    object htmlAttributes = new { id = "", @class = data.IsDirtyProperty };
                    MvcHtmlString hiddenInput = helper.Hidden(column.PropertyName, column.Metadata.Model, htmlAttributes);
                    data.HiddenInputs.Append(hiddenInput);
                    columnIndex++;
                    continue;
                }
                if (column.IsHidden)
                {
                    // Add hidden inputs for the property
                    string hiddenInputs = HiddenInputHelper.HiddenInputForMetadata(column.Metadata, column.PropertyName, false);
                    data.HiddenInputs.Append(hiddenInputs);
                    columnIndex++;
                    continue;
                }
                if (propertyMetadata.IsComplexType && !column.HasDisplayProperty)
                {
                    // Add cells for each property of the type (recursive call)
                    string cells = EditableTableBodyCells(helper, column.Metadata, column.PropertyName, ref columnIndex, data, true);
                    html.Append(cells);
                }
                else
                {
                    if (column.HasDisplayProperty)
                    {
                        string hiddenInput = HiddenInputHelper.HiddenInputForMetadata(column.Metadata, column.PropertyName, true);
                        data.HiddenInputs.Append(hiddenInput);
                        column.PropertyName = String.Format("{0}.{1}", column.PropertyName, column.DisplayProperty);
                        column.Metadata = propertyMetadata.Properties.FirstOrDefault(m => m.PropertyName == column.DisplayProperty);
                    }
                    if (column.IncludeTotals && !data.IsArchived)
                    {
                        // Update running total
                        column.ColumnTotal += Convert.ToDecimal(column.Metadata.Model);
                    }
                    if (column.SelectListProperty != null)
                    {
                        // Build SelectList
                        IEnumerable<SelectListItem> selectList = data.SelectLists.First(x => x.Key == propertyMetadata.PropertyName).Value;
                        string defaultValue = Convert.ToString(column.Metadata.Model);
                        List<SelectListItem> options = new List<SelectListItem>();
                        foreach (SelectListItem item in selectList)
                        {
                            item.Selected = (item.Value != null) ? item.Value == defaultValue : item.Text == defaultValue;
                            options.Add(item);
                        }
                        column.SelectList = options;
                    }
                    if (column.IsReadonly)
                    {
                        string text = column.Metadata.GetFormattedValue();
                        if (column.NoRepeat)
                        {
                            if (text == column.PreviousValue)
                            {
                                text = string.Empty;
                            }
                            else
                            {
                                column.PreviousValue = text;
                            }
                        }
                        // Add the readonly cell
                        text = TableText(text);
                        string cell = TableCell(text);
                        html.Append(cell);
                        // Add the hidden input
                        if (!column.HasDisplayProperty)
                        {
                            string hiddenInput = HiddenInputHelper.HiddenInputForMetadata(column.Metadata, column.PropertyName, true);
                            data.HiddenInputs.Append(hiddenInput);
                        }
                    }
                    else
                    {
                        string formControl = FormControl(helper, column, data.IsArchived);
                        html.Append(formControl);

                    }
                    columnIndex++;
                }
            }
            if (!isRecursive)
            {
                // Add button column
                if (data.IncludeButtons)
                {
                    string button = ButtonCell(TableButtonType.Delete);
                    html.Append(button);
                }
                string indexer = RowIndexer(prefix);
                data.HiddenInputs.Append(indexer);
                string cell = TableCell(data.HiddenInputs.ToString());
                html.Append(cell);
            }
            // Return the html
            return html.ToString();
        }

        // Returns the <td> elements for each validation row
        private static string ValidationRowCells(HtmlHelper helper, TableData data)
        {
            StringBuilder html = new StringBuilder();
            foreach (TableColumnData column in data.Columns)
            {
                if (column.IsExcluded || column.IsHidden)
                {
                    continue;
                }
                TagBuilder cell = new TagBuilder("td");
                if (column.RequireValidation)
                {
                    MvcHtmlString validation = helper.ValidationMessage(column.PropertyName);
                    cell.InnerHtml = validation.ToString();
                }
                html.Append(cell.ToString());
            }
            TagBuilder blankCell = new TagBuilder("td");
            if (data.IncludeButtons)
            {
                string buttonCell = TableCell(null, "button-cell");
                html.Append(buttonCell);
            }
            string hiddenCell = TableCell(null);
            html.Append(hiddenCell);
            return html.ToString();
        }

        // Returns the <tfoot> element
        private static string TableFooter(TableData data)
        {
            TagBuilder row = new TagBuilder("tr");
            row.InnerHtml = TableFooterCells(data);
            TagBuilder footer = new TagBuilder("tfoot");
            footer.InnerHtml = row.ToString();
            return footer.ToString();
        }

        // Returns the <td> elements for the footer
        private static string TableFooterCells(TableData data)
        {
            string emptyCell = TableCell(null);
            // Build the html for each cell
            StringBuilder html = new StringBuilder();
            if (data.IncludeRowNumbers)
            {
                html.Append(emptyCell);
            }
            foreach (TableColumnData column in data.Columns)
            {
                if (column.IsHidden || column.IsExcluded)
                {
                    continue;
                }
                if (column.IncludeTotals)
                {
                    string formattedValue = string.Format(column.FormatString, column.ColumnTotal);
                    string text = TableText(formattedValue, "footer-total");
                    string cell = TableCell(text);
                    html.Append(cell);
                }
                else
                {
                    html.Append(emptyCell);
                }
            }
            if (data.IncludeDetailsLink)
            {
                html.Append(emptyCell);
            }
            if (data.IncludeEditLink)
            {
                html.Append(emptyCell);
            }
            if (data.IncludeButtons)
            {
                string buttonCell = ButtonCell(TableButtonType.Add);
                html.Append(buttonCell);
            }
            else if (data.IsEditMode)
            {
                html.Append(emptyCell);
            }
            //  Return the html
            return html.ToString();
        }

        // Returns a <th> element
        private static string HeaderCell(string text, string className = null)
        {
            TagBuilder cell = new TagBuilder("th");
            if (!String.IsNullOrEmpty(className))
            {
                cell.AddCssClass(className);
            }
            if (text != null)
            {
                cell.InnerHtml = text;
            }
            return cell.ToString();
        }

        // Returns a <td> element
        private static string TableCell(string text, string className = null)
        {
            TagBuilder cell = new TagBuilder("td");
            if (!String.IsNullOrEmpty(className))
            {
                cell.AddCssClass(className);
            }
            if (text != null)
            {
                cell.InnerHtml = text;
            }
            return cell.ToString();
        }

        // Returns a <div> element containing the text
        private static string TableText(string text, string className = null)
        {
            TagBuilder div = new TagBuilder("div");
            if (String.IsNullOrEmpty(className))
            {
                className = "table-text";
            }
            div.AddCssClass(className);
            div.InnerHtml = text;
            return div.ToString();
        }

        // Generates a <td> element containing a form control
        private static string FormControl(HtmlHelper helper, TableColumnData column, bool isArchived)
        {
            ModelMetadata metadata = column.Metadata;
            string name = column.PropertyName;
            MvcHtmlString formControl = null;
            Type type = Nullable.GetUnderlyingType(column.Metadata.ModelType) ?? column.Metadata.ModelType;
            IDictionary<string, object> htmlAttributes = helper.GetUnobtrusiveValidationAttributes(name, metadata);
            htmlAttributes.Add("id", null);
            htmlAttributes.Add("class", "table-control");
            if (isArchived)
            {
                htmlAttributes.Add("style", "display:none;");
            }
            if (column.IncludeTotals)
            {
                htmlAttributes["class"] = "table-control numeric-input";
            }
            else
            if (column.DataListProperty != null)
            {
                string id = String.Format("{0}-datalist", column.Metadata.PropertyName.ToLower());
                htmlAttributes.Add("list", id);
            }
            if (column.SelectList != null)
            {
                formControl = helper.DropDownList(name, column.SelectList, htmlAttributes);
            }
            else if (metadata.DataTypeName == "MultilineText")
            {
                formControl = helper.TextArea(name, (metadata.Model ?? string.Empty).ToString(), htmlAttributes);
            }
            else if (metadata.ModelType == typeof(bool))
            {
                formControl = helper.CheckBox(name, (bool)metadata.Model, htmlAttributes);
            }
            else if (metadata.ModelType == typeof(bool?))
            {
                bool? defaultValue = (bool?)metadata.Model;
                List<SelectListItem> selectList = new List<SelectListItem>()
                {
                    new SelectListItem(){ Value = "", Text = "", Selected = !defaultValue.HasValue }, // TODO: Text from Resource File
                    new SelectListItem(){ Value = "true", Text = "Yes", Selected = defaultValue.HasValue && defaultValue.Value },
                    new SelectListItem(){ Value = "false", Text = "No", Selected = defaultValue.HasValue && !defaultValue.Value }
                };
                formControl = helper.DropDownList(name, selectList, htmlAttributes);
            }
            else if (type.IsEnum)
            {
                string defaultValue = Convert.ToString(metadata.Model);
                List<SelectListItem> selectList = new List<SelectListItem>();
                var nullText = metadata.NullDisplayText; // TODO: Review
                selectList.Add(new SelectListItem() { Value = "", Text = nullText });
                foreach (Enum item in Enum.GetValues(type))
                {
                    string value = item.ToString();
                    string text = item.ToDisplayName();
                    selectList.Add(new SelectListItem() { Value = value, Text = text, Selected = (value == defaultValue) });
                }
                formControl = helper.DropDownList(name, selectList, htmlAttributes);
            }
            else
            {
                formControl = helper.TextBox(name, metadata.Model, htmlAttributes);
            }
            StringBuilder html = new StringBuilder();
            html.Append(formControl.ToString());
            if (isArchived)
            {
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass("table-text");
                div.InnerHtml = metadata.GetFormattedValue();
                html.Append(div.ToString());
            }
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = html.ToString();
            return cell.ToString();
        }

        // Returns the hidden input for the row indexer.
        private static string RowIndexer(string prefix)
        {
            string pattern = @"(.+)\[([^\]]+)\]";
            Regex regex = new Regex(pattern);
            var match = regex.Match(prefix);
            string name = string.Format("{0}.Index", match.Groups[1].Value);
            string indexer = match.Groups[2].Value;
            if (indexer == "#")
            {
                indexer = "%";
            }
            TagBuilder input = new TagBuilder("input");
            input.MergeAttribute("type", "hidden");
            input.MergeAttribute("name", name);
            input.MergeAttribute("value", indexer);
            return input.ToString(); ;
        }

        // Returns a <td> element containing a <button> to add and delete table rows
        private static string ButtonCell(TableButtonType type)
        {
            TagBuilder button = new TagBuilder("button");
            button.AddCssClass("table-button");
            button.AddCssClass(string.Format("{0}-button", type.ToString().ToLower()));
            button.MergeAttribute("type", "button");
            TagBuilder cell = new TagBuilder("td");
            cell.AddCssClass("button-cell");
            cell.InnerHtml = button.ToString();
            return cell.ToString();
        }

        // Generates a datalist element
        private static string DataList(string id, IEnumerable<string> options)
        {
            if (options == null)
            {
                return null;
            }
            StringBuilder html = new StringBuilder();
            foreach (string item in options)
            {
                TagBuilder option = new TagBuilder("option");
                option.MergeAttribute("value", item);
                html.Append(option.ToString(TagRenderMode.StartTag));
            }
            TagBuilder dataList = new TagBuilder("datalist");
            dataList.MergeAttribute("id", id);
            dataList.InnerHtml = html.ToString();
            return dataList.ToString();
        }

        #endregion

    }

}
