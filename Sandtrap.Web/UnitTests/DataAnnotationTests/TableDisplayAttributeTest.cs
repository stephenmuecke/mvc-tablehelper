using System;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sandtrap.Web.DataAnnotations;
using Resources = Sandtrap.Web.Properties.Resources;

namespace Sandtrap.Web.UnitTests.DataAnnotationTests
{
    [TestClass]
    public class TableDisplayAttributeTest
    {

        const string _Controller = "Foo";
        const string _DisplayAction = "Details";
        const string _EditAction = "Edit";
        const string _IDProperty = "ID";

        [TableDisplay(IncludeRowNumbers = true)]
        class FooA
        {
        }

        [TableDisplay(DetailsAction = _DisplayAction)]
        class FooB
        {
        }

        [TableDisplay(Controller = _Controller, DetailsAction = _DisplayAction)]
        class FooC
        {
        }

        [TableDisplay(Controller = _Controller, DetailsAction = _DisplayAction, EditAction = _EditAction, IDProperty = _IDProperty)]
        class FooD
        {
            public int ID { get; set; }
        }


        /// <summary>
        /// IncludeRowNumbers should add an AdditionalValues entry.
        /// </summary>
        [TestMethod]
        public void AdditionalValuesIncludesRowNumbers()
        {
            // arrange
            var type = typeof(FooA);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(null, type);
            // assert
            Assert.IsTrue(metadata.AdditionalValues.ContainsKey(Resources.TableDisplayAttribute_IncludeRowNumbers));
        }

        /// <summary>
        /// An exception should be thrown if either the DetailsAction or EditAction is not null and Controller is null.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MissingControllerNameThrowsException()
        {
            // arrange
            var type = typeof(FooB);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(null, type);
        }

        /// <summary>
        /// A missing IDProperty should throw and exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MissingIDPropertyThrowsException()
        {
            // arrange
            var type = typeof(FooC);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(null, type);
        }

        /// <summary>
        /// AdditionalValues entries are added.
        /// </summary>
        [TestMethod]
        public void AdditionalValuesContainsDependentProperties()
        {
            // arrange
            var type = typeof(FooD);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(null, type);
            // assert
            Assert.IsTrue(metadata.AdditionalValues.ContainsKey(Resources.TableDisplayAttribute_IncludeDetailsLink));
            Assert.IsTrue(metadata.AdditionalValues.ContainsKey(Resources.TableDisplayAttribute_IncludeEditLink));
            Assert.IsTrue(metadata.AdditionalValues.ContainsKey(Resources.TableDisplayAttribute_ControllerName));
            Assert.IsTrue(metadata.AdditionalValues.ContainsKey(Resources.TableDisplayAttribute_DetailsActionName));
            Assert.IsTrue(metadata.AdditionalValues.ContainsKey(Resources.TableDisplayAttribute_EditActionName));
            Assert.IsTrue(metadata.AdditionalValues.ContainsKey(Resources.TableDisplayAttribute_RouteValue));
        }

    }
}
