using System;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sandtrap.Web.DataAnnotations;
using Resources = Sandtrap.Web.Properties.Resources;

namespace Sandtrap.Web.UnitTests.DataAnnotationTests
{

    [TestClass]
    public class TableColumnAttributeTests
    {

        class Bar
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        class Foo
        {
            [TableColumn(Exclude = true, NoRepeat = true)]
            public string ExcludedProperty { get; set; }

            [TableColumn(IsReadonly = true)]
            public string ReadonlySimpleProperty { get; set; }

            [TableColumn(DisplayProperty = "Name")]
            public Bar ComplexDisplayProperty { get; set; }

            [TableColumn(DisplayProperty = "Invalid")]
            public Bar InvalidComplexDisplayProperty { get; set; }

            [TableColumn(IncludeTotal = true)]
            public decimal TotalsProperty { get; set; }

            [TableColumn(IncludeTotal = true)]
            public string InvalidTotalsProperty { get; set; }

            [TableColumn(NoRepeat = true)]
            public string NoRepeatProperty { get; set; }

            [TableColumn(NoRepeat = true, IncludeTotal = true)]
            public decimal InvalidNoRepeatProperty { get; set; }

        }


        /// <summary>
        /// Exclude should add an AdditionalValues entry and ignore other attribute properties.
        /// </summary>
        [TestMethod]
        public void ExcludeAddsAdditionalValues()
        {
            // arrange
            var type = typeof(Foo);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, type, "ExcludedProperty");
            // assert
            Assert.IsTrue(metadata.AdditionalValues.ContainsKey(Resources.TableColumnAttribute_Exclude));
            Assert.AreEqual(1, metadata.AdditionalValues.Count);
        }

        /// <summary>
        /// Readonly should add an AdditionalValues entry.
        /// </summary>
        [TestMethod]
        public void ReadonlyAddsAdditionalValues()
        {
            // arrange
            var type = typeof(Foo);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, type, "ReadonlySimpleProperty");
            // assert
            Assert.IsTrue(metadata.AdditionalValues.ContainsKey(Resources.TableColumnAttribute_Readonly));
        }

        /// <summary>
        /// DisplayProperty should add an AdditionalValues entry if valid.
        /// </summary>
        [TestMethod]
        public void DisplayPropertyAddsAdditionalValue()
        {
            // arrange
            var type = typeof(Foo);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, type, "ComplexDisplayProperty");
            // assert
            Assert.IsTrue(metadata.AdditionalValues.ContainsKey(Resources.TableColumnAttribute_DisplayProperty));
            Assert.AreEqual("Name", metadata.AdditionalValues[Resources.TableColumnAttribute_DisplayProperty]);
        }

        /// <summary>
        /// DisplayProperty should throw exception if the propery if applied to a complex property
        /// and the property name does not exist.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DisplayPropertyThrowsExceptionForInvalidPropertyName()
        {
            // arrange
            var type = typeof(Foo);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, type, "InvalidComplexDisplayProperty");
        }

        /// <summary>
        /// IncludeTotal should add an AdditionalValues entry if valid numeric.
        /// </summary>
        [TestMethod]
        public void IncludeTotalAddsAdditionalValue()
        {
            // arrange
            var type = typeof(Foo);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, type, "TotalsProperty");
            // assert
            Assert.IsTrue(metadata.AdditionalValues.ContainsKey(Resources.TableColumnAttribute_IncludeTotal));
        }

        /// <summary>
        /// IncludeTotal should not add an AdditionalValues entry if property is not a valid numeric.
        /// </summary>
        [TestMethod]
        public void IncludeTotalShouldNotAddAdditionalValueForInvalidProperty()
        {
            // arrange
            var type = typeof(Foo);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, type, "InvalidTotalsProperty");
            // assert
            Assert.IsFalse(metadata.AdditionalValues.ContainsKey(Resources.TableColumnAttribute_IncludeTotal));
        }

        /// <summary>
        /// NoRepeat should add an AdditionalValues entry.
        /// </summary>
        [TestMethod]
        public void NoRepeatAddsAdditionalValue()
        {
            // arrange
            var type = typeof(Foo);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, type, "NoRepeatProperty");
            // assert
            Assert.IsTrue(metadata.AdditionalValues.ContainsKey(Resources.TableColumnAttribute_NoRepeat));
        }

        /// <summary>
        /// NoRepeat should not add an AdditionalValues entry if IncludeTotal = true
        /// </summary>
        [TestMethod]
        public void NoRepeatShouldNotAddAdditionalValueIfIncludeTotal()
        {
            // arrange
            var type = typeof(Foo);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, type, "InvalidNoRepeatProperty");
            // assert
            Assert.IsFalse(metadata.AdditionalValues.ContainsKey(Resources.TableColumnAttribute_NoRepeat));
        }

    }

}
