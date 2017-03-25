using System;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sandtrap.Web.DataAnnotations;
using Resources = Sandtrap.Web.Properties.Resources;

namespace Sandtrap.Web.UnitTests.DataAnnotationTests
{

    [TestClass]
    public class TableEditAttributeTests
    {

        const string _IsActiveDependentProperty = "IsActive";
        const string _IsDirtyDependentProperty = "IsDirty";

        [TableEdit]
        class FooA
        {
        }

        [TableEdit(IsActiveProperty = _IsActiveDependentProperty, IsDirtyProperty = _IsDirtyDependentProperty)]
        class FooB
        {
            public bool IsActive { get; set; }
            public bool IsDirty { get; set; }
        }

        [TableEdit(IsActiveProperty = _IsActiveDependentProperty)]
        class FooC
        {
        }

        [TableEdit(IsActiveProperty = _IsActiveDependentProperty)]
        class FooD
        {
            public string IsActive { get; set; }
        }

        [TableEdit(IsDirtyProperty = _IsDirtyDependentProperty)]
        class FooE
        {
        }

        [TableEdit(IsDirtyProperty = _IsDirtyDependentProperty)]
        class FooF
        {
            public string IsDirty { get; set; }
        }


        /// <summary>
        /// The default usage should add AdditionalValues entries for AllowAdditions and AllowDeletions.
        /// </summary>
        [TestMethod]
        public void DefaultAttributeAddsAllowAdditionsAndAllowDeletions()
        {
            // arrange
            var type = typeof(FooA);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(null, type);
            // assert
            Assert.IsTrue(metadata.AdditionalValues.ContainsKey(Resources.TableEditAttribute_AllowAdditions));
            Assert.IsTrue((bool)metadata.AdditionalValues[Resources.TableEditAttribute_AllowAdditions] == true);
            Assert.IsTrue(metadata.AdditionalValues.ContainsKey(Resources.TableEditAttribute_AllowDeletions));
            Assert.IsTrue((bool)metadata.AdditionalValues[Resources.TableEditAttribute_AllowDeletions] == true);
        }

        /// <summary>
        /// Specifying IsActiveProperty and IsDirtyProperty should add AdditionalValues entries 
        /// matching the property names.
        /// </summary>
        [TestMethod]
        public void AdditionalValuesContainsDependentProperties()
        {
            // arrange
            var type = typeof(FooB);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(null, type);
            Assert.IsTrue(metadata.AdditionalValues.ContainsKey(Resources.TableEditAttribute_ActiveProperty));
            Assert.IsTrue((string)metadata.AdditionalValues[Resources.TableEditAttribute_ActiveProperty] == _IsActiveDependentProperty);
            Assert.IsTrue(metadata.AdditionalValues.ContainsKey(Resources.TableEditAttribute_DirtyProperty));
            Assert.IsTrue((string)metadata.AdditionalValues[Resources.TableEditAttribute_DirtyProperty] == _IsDirtyDependentProperty);
        }

        /// <summary>
        /// The IsActiveProperty must exist in the model.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowExceptionIfMissingIsActiveProperty()
        {
            // arrange
            var type = typeof(FooC);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(null, type);
        }

        /// <summary>
        /// The IsActiveProperty must be typeof bool.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowExceptionIfInvalidIsActiveProperty()
        {
            // arrange
            var type = typeof(FooD);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(null, type);
        }

        /// <summary>
        /// The IsDirtyProperty must exist in the model.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowExceptionIfMissingIsDirtyProperty()
        {
            // arrange
            var type = typeof(FooE);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(null, type);
        }

        /// <summary>
        /// The IsDirtyProperty must be typeof bool.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowExceptionIfInvalidIsDirtyProperty()
        {
            // arrange
            var type = typeof(FooF);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(null, type);
        }

    }

}
