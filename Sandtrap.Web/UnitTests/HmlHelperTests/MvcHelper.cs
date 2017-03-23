using System.Collections;
using System.Web.Mvc;
using Moq;

namespace Sandtrap.Web.UnitTests.HtmlHelperTests
{
    public static class MvcHelper
    {

        public static HtmlHelper<TModel> GetHtmlHelper<TModel>(ViewDataDictionary<TModel> viewData)
        {
            Mock<ViewContext> mockViewContext = new Mock<ViewContext>() { CallBase = true };
            mockViewContext.Setup(c => c.ViewData).Returns(viewData);
            mockViewContext.Setup(c => c.HttpContext.Items).Returns(new Hashtable());
            IViewDataContainer container = GetViewDataContainer(viewData);
            return new HtmlHelper<TModel>(mockViewContext.Object, container);
        }

        public static IViewDataContainer GetViewDataContainer(ViewDataDictionary viewData)
        {
            Mock<IViewDataContainer> c = new Mock<IViewDataContainer>();
            c.Setup(x => x.ViewData).Returns(viewData);
            return c.Object;
        }

    }
}
