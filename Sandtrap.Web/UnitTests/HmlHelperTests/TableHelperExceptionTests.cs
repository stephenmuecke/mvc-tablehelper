using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sandtrap.Web.Html;

namespace Sandtrap.Web.UnitTests.HtmlHelperTests
{

    // TODO: Dont throw NullReferenceException if model is null?
    // Just display table headings and perhaps message indicating no data
    // Allow new rows to be added in TableEditorFor() method (need to ensure AllowAdditions = true)

    [TestClass]
    public class TableHelperExceptionTests
    {

        #region .Models 

        class Foo
        {
            public IEnumerable<Bar> NullModel { get; set; }
            public string StringModel { get; set; }
            public IEnumerable<IGrouping<string, Bar>> GroupedModel { get; set; }
            public IEnumerable<Baz> NoDefaultConstructorModel { get; set; }
        }

        class Bar
        {
            public string Name { get; set; }
        }

        class Baz
        {
            public Baz(string name)
            {
                Name = name;
            }
            public string Name { get; set; }

        }

        #endregion

        #region .Tests 

        /// <summary>
        /// An exception should be thrown if the model is null
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void DisplayForNullModelThrowsException()
        {
            Foo model = new Foo();
            HtmlHelper<Foo> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<Foo>() { Model = model });
            MvcHtmlString html = helper.TableDisplayFor(m => m.NullModel);
        }

        /// <summary>
        /// An exception should be thrown if the model is a string
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DisplayForStringModelThrowsException()
        {

            Foo model = new Foo() { StringModel = "test" };
            HtmlHelper<Foo> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<Foo>() { Model = model });
            MvcHtmlString html = helper.TableDisplayFor(m => m.StringModel);
        }

        /// <summary>
        /// An exception should be thrown if the model is a grouped
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DisplayForGroupedModelThrowsException()
        {
            List<Bar> bars = new List<Bar>() { new Bar() };
            Foo model = new Foo() { GroupedModel = bars.GroupBy(x => x.Name) };
            HtmlHelper<Foo> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<Foo>() { Model = model });
            MvcHtmlString html = helper.TableDisplayFor(m => m.GroupedModel);
        }

        /// <summary>
        /// An exception should be thrown if the model is null
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void EditorForNullModelThrowsException()
        {
            Foo model = new Foo();
            HtmlHelper<Foo> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<Foo>() { Model = model });
            MvcHtmlString html = helper.TableEditorFor(m => m.NullModel);
        }

        /// <summary>
        /// An exception should be thrown if the model is a string
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EditorForStringModelThrowsException()
        {

            Foo model = new Foo() { StringModel = "test" };
            HtmlHelper<Foo> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<Foo>() { Model = model });
            MvcHtmlString html = helper.TableEditorFor(m => m.StringModel);
        }

        /// <summary>
        /// An exception should be thrown if the model is a grouped
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EditorForGroupedModelThrowsException()
        {
            List<Bar> bars = new List<Bar>() { new Bar() };
            Foo model = new Foo() { GroupedModel = bars.GroupBy(x => x.Name) };
            HtmlHelper<Foo> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<Foo>() { Model = model });
            MvcHtmlString html = helper.TableEditorFor(m => m.GroupedModel);
        }

        /// <summary>
        /// An exception should be thrown if the model has no default constructor.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void EditorForModelWithNoDefaultConstructorThrowsException()
        {
            List<Baz> bazs = new List<Baz>() { new Baz("test") };
            Foo model = new Foo() { NoDefaultConstructorModel = bazs };
            HtmlHelper<Foo> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<Foo>() { Model = model });
            MvcHtmlString html = helper.TableEditorFor(m => m.NoDefaultConstructorModel);
        }

        #endregion

    }

}
