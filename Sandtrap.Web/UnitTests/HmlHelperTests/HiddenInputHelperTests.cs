using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sandtrap.Web.Html;

namespace Sandtrap.Web.UnitTests.HtmlHelperTests
{
    [TestClass]
    public class HiddenInputHelperTests
    {

        public class Foo
        {
            public int FooID { get; set; }
            public string FooName { get; set; }
            [Required]
            public Bar Bar { get; set; }
            public Baz Baz { get; set; }
            public List<Bar> BarsList { get; set; }
        }
        public class Bar
        {
            [Required]
            public int? BarID { get; set; } // nullable and required
            public string BarName { get; set; }
        }
        public class Baz
        {
            public int? BazID { get; set; } // nullable and not required
            public string BazName { get; set; }
        }

        /// <summary>
        /// Ensure no parse errors.
        /// </summary>
        [TestMethod]
        public void ValidateParseErrors()
        {
            Foo model = new Foo() { FooID = 1, FooName = "Foo", Bar = new Bar() { BarID = 1, BarName = "Bar" } };
            HtmlHelper<Foo> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<Foo>() { Model = model });
            MvcHtmlString html = helper.HiddenInputFor(m => m);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html.ToString());
            int errors = document.ParseErrors.Count();
            Assert.AreEqual(0, errors);
        }

        /// <summary>
        /// Hidden inputs for required properties should be generated if null.
        /// </summary>
        [TestMethod]
        public void InputsShouldBeCreatedForRequiredNulls()
        {
            Foo model = new Foo() { FooID = 1, FooName = "Foo" };
            HtmlHelper<Foo> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<Foo>() { Model = model });
            MvcHtmlString html = helper.HiddenInputFor(m => m);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html.ToString());
            HtmlNode root = document.DocumentNode;
            IEnumerable<HtmlNode> inputs = root.Descendants("input").Where(x => x.GetAttributeValue("type", "").Equals("hidden"));
            Assert.AreEqual(4, inputs.Count());
        }

        /// <summary>
        /// Hidden inputs for collection properties should not be created.
        /// </summary>
        [TestMethod]
        public void InputsShouldNotBeCreatedForCollectionProperties()
        {
            Foo model = new Foo() { FooID = 1, FooName = "Foo", BarsList = new List<Bar>(){ new Bar() }};
            HtmlHelper<Foo> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<Foo>() { Model = model });
            MvcHtmlString html = helper.HiddenInputFor(m => m);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html.ToString());
            HtmlNode root = document.DocumentNode;
            IEnumerable<HtmlNode> inputs = root.Descendants("input").Where(x => x.GetAttributeValue("type", "").Equals("hidden"));
            Assert.AreEqual(4, inputs.Count());
        }

        /// <summary>
        /// Inputs should be created for all proprrties of Foo, Bar and Baz except BazID (nullable and null)
        /// </summary>
        [TestMethod]
        public void ValidateNameAndValueAttributes()
        {
            Foo model = new Foo() { FooID = 1, FooName = "Foo", Baz = new Baz() { BazName = "Baz" } };
            HtmlHelper<Foo> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<Foo>() { Model = model });
            MvcHtmlString html = helper.HiddenInputFor(m => m);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html.ToString());
            HtmlNode root = document.DocumentNode;
            IEnumerable<HtmlNode> allInputs = root.Descendants("input");
            Assert.AreEqual(5, allInputs.Count());
            HtmlNode FooID = allInputs.SingleOrDefault(x => x.GetAttributeValue("name", "").Equals("FooID"));
            Assert.IsNotNull(FooID);
            HtmlAttribute FooIDValue = FooID.Attributes.SingleOrDefault(x => x.Value == "1");
            Assert.IsNotNull(FooIDValue);
            HtmlNode FooName = allInputs.SingleOrDefault(x => x.GetAttributeValue("name", "").Equals("FooName"));
            Assert.IsNotNull(FooName);
            HtmlAttribute FooNameValue = FooName.Attributes.SingleOrDefault(x => x.Value == "Foo");
            Assert.IsNotNull(FooNameValue);
            HtmlNode BarID = allInputs.SingleOrDefault(x => x.GetAttributeValue("name", "").Equals("Bar.BarID"));
            Assert.IsNotNull(BarID);
            HtmlAttribute BarIDValue = BarID.Attributes.SingleOrDefault(x => x.Value == "");
            Assert.IsNotNull(BarIDValue);
            HtmlNode BarName = allInputs.SingleOrDefault(x => x.GetAttributeValue("name", "").Equals("Bar.BarName"));
            Assert.IsNotNull(BarName);
            HtmlAttribute BarNameValue = BarName.Attributes.SingleOrDefault(x => x.Value == "");
            Assert.IsNotNull(BarNameValue);
            HtmlNode BazName = allInputs.SingleOrDefault(x => x.GetAttributeValue("name", "").Equals("Baz.BazName"));
            Assert.IsNotNull(BazName);
            HtmlAttribute BazNameValue = BazName.Attributes.SingleOrDefault(x => x.Value == "Baz");
            Assert.IsNotNull(BarNameValue);
        }
    }
}
