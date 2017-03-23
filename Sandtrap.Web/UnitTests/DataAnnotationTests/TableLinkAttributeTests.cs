using System;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sandtrap.Web.DataAnnotations;
using Resources = Sandtrap.Web.Properties.Resources;

namespace Sandtrap.Web.UnitTests.DataAnnotationTests
{
    [TestClass]
    public class TableLinkAttributeTests
    {

        const string _IDProperty = "FooID";
        const string _DisplayProperty = "FooName";

        [TableLink]
        class FooA
        {
        }

        [TableLink(Controller = "Product")]
        class FooB
        {
            public int FooID { get; set; }
            public string FooName { get; set; }
        }

        [TableLink(Controller = "Product", IDProperty = _IDProperty)]
        class FooC
        {
            public int FooID { get; set; }
            public string FooName { get; set; }
        }

        [TableLink(Controller = "Product", IDProperty = _IDProperty, DisplayProperty = _DisplayProperty)]
        class FooD
        {
            public int FooID { get; set; }
            public string FooName { get; set; }
        }


        /// <summary>
        /// The controller name is required.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowExceptionIfControllerMissing()
        {
            // arrange
            var type = typeof(FooA);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(null, type);
        }

        /// <summary>
        /// The IDProperty must exist in the model.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowExceptionIfIDPropertyMissing()
        {
            // arrange
            var type = typeof(FooB);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(null, type);
        }

        /// <summary>
        /// The DisplayProperty must exist in the model.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowExceptionIfDisplayPropertyMissing()
        {
            // arrange
            var type = typeof(FooC);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(null, type);
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void AddsAdditionalValues()
        {
            // arrange
            var type = typeof(FooD);
            // act
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(null, type);
            Assert.IsTrue(metadata.AdditionalValues.ContainsKey(Resources.TableLinkAttribute_IncludeLink));
            Assert.IsTrue((bool)metadata.AdditionalValues[Resources.TableLinkAttribute_IncludeLink] == true);
            Assert.IsTrue(metadata.AdditionalValues.ContainsKey(Resources.TableLinkAttribute_DisplayProperty));
            Assert.IsTrue((string)metadata.AdditionalValues[Resources.TableLinkAttribute_DisplayProperty] == _DisplayProperty);
            // TODO: How can we test the URL?
            // Probably should just add the IDProperty value to AdditionalValues and then
            // generate the Url in the TableDisplayFor() method.
        }

        //[TestMethod]
        //public void TestUrl()
        //{
        //    // TODO: Can create a model so its not null, but the attribute uses HttpContext.Current.Request.RequestContext
        //    var type = typeof(FooD);
        //    var model = new FooD() { FooID = 3, FooName = "xxxx" };
        //    var metadata = ModelMetadata.FromLambdaExpression<FooD, FooD>(m => m, new ViewDataDictionary<FooD>() { Model = model });
        //    Assert.IsTrue(metadata.AdditionalValues.ContainsKey(Resources.TableLinkAttribute_IncludeLink));

        //    Mock<TableLinkAttribute> a = new Mock<TableLinkAttribute>();
            

        //}

    }
}
