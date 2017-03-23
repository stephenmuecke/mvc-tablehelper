using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sandtrap.Web.DataAnnotations;
using Resources = Sandtrap.Web.Properties.Resources;

namespace Sandtrap.Web.UnitTests
{

    [TestClass]
    public class DataListAttributeTests
    {
        const string _DependentProperty = "DependentProperty";

        class Foo
        {
            // Valid DataListAttribute
            [DataList(DataListProperty = _DependentProperty)]
            public string Bar { get; set; }
            // Invalid DataListAttribute (missing DataListProperty)
            [DataList]
            public string Baz { get; set; }
        }

        [TestMethod]
        public void InvalidDataListAttributeDoesNotAddAdditionalValues()
        {
            // arrange
            var type = typeof(Foo);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, type, "Baz");
            // assert
            Assert.IsFalse(metadata.AdditionalValues.ContainsKey(Resources.DataListAttribute_DataListProperty));
        }

        [TestMethod]
        public void DataListAttributeAddsAdditionalValues()
        {
            // arrange
            var type = typeof(Foo);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, type, "Bar");
            // assert
            Assert.IsTrue(metadata.AdditionalValues.ContainsKey(Resources.DataListAttribute_DataListProperty));
            Assert.IsTrue((string)metadata.AdditionalValues[Resources.DataListAttribute_DataListProperty] == _DependentProperty);
        }

    }

}
