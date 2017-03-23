using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sandtrap.Web.DataAnnotations;
using Resources = Sandtrap.Web.Properties.Resources;

namespace Sandtrap.Web.UnitTests.DataAnnotationTests
{

    [TestClass]
    public class DropDownListAttributeTests
    {

        const string _SelectListProperty = "SelectList";
        const string _OptionLabel = "Please select";

        class Foo
        {
            [DropDownList(SelectListProperty = _SelectListProperty, OptionLabel = _OptionLabel)]
            public string Bar { get; set; }
            // Invalid DropDownListAttribute (missing _SelectListProperty)
            [DropDownList]
            public string Baz { get; set; }
        }

        /// <summary>
        /// AdditionalValues entries should not be added if the SelectListProperty is missing.
        /// </summary>
        [TestMethod]
        public void MissingSelectListPropertyDoesNotAddAdditionalValues()
        {
            // arrange
            var type = typeof(Foo);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, type, "Baz");
            // assert
            Assert.IsFalse(metadata.AdditionalValues.ContainsKey(Resources.DropDownListAttribute_SelectListProperty));
            Assert.IsFalse(metadata.AdditionalValues.ContainsKey(Resources.DropDownListAttribute_OptionLabel));
        }

        /// <summary>
        /// AdditionalValues entries should be added matching the SelectListProperty and OptionLabel.
        /// </summary>
        [TestMethod]
        public void SelectListAttributeAddsAdditionalValues()
        {
            // arrange
            var type = typeof(Foo);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, type, "Bar");
            // assert
            Assert.IsTrue(metadata.AdditionalValues.ContainsKey(Resources.DropDownListAttribute_SelectListProperty));
            Assert.IsTrue((string)metadata.AdditionalValues[Resources.DropDownListAttribute_SelectListProperty] == _SelectListProperty);
            Assert.IsTrue(metadata.AdditionalValues.ContainsKey(Resources.DropDownListAttribute_OptionLabel));
            Assert.IsTrue((string)metadata.AdditionalValues[Resources.DropDownListAttribute_OptionLabel] == _OptionLabel);
        }

    }

}
