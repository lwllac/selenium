using System;
using NUnit.Framework;

namespace OpenQA.Selenium.Interactions
{
    [TestFixture]
    public class BasicWheelInterfaceTest : DriverTestFixture
    {
        [SetUp]
        public void SetupTest()
        {
            if (driver is IActionExecutor actionExecutor)
            {
                actionExecutor.ResetInputState();
            }
            driver.SwitchTo().DefaultContent();
            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, 0)");
        }

        [Test]
        public void ShouldAllowScrollingToAnElement()
        {
            driver.Url = scrollFrameOutOfViewport;
            IWebElement iframe = driver.FindElement(By.TagName("iframe"));

            Assert.IsFalse(IsInViewport(iframe));

            new Actions(driver).ScrollToElement(iframe).Build().Perform();

            Assert.IsTrue(IsInViewport(iframe));
        }

        [Test]
        public void ShouldScrollFromElementByGivenAmount()
        {
            driver.Url = scrollFrameOutOfViewport;
            IWebElement iframe = driver.FindElement(By.TagName("iframe"));
            WheelInputDevice.ScrollOrigin scrollOrigin = new WheelInputDevice.ScrollOrigin
            {
                Element = iframe
            };

            new Actions(driver).ScrollFromOrigin(scrollOrigin, 0, 200).Build().Perform();

            driver.SwitchTo().Frame(iframe);
            IWebElement checkbox = driver.FindElement(By.Name("scroll_checkbox"));
            Assert.IsTrue(IsInViewport(checkbox));
        }

        [Test]
        public void ShouldAllowScrollingFromElementByGivenAmountWithOffset()
        {
            driver.Url = scrollFrameOutOfViewport;
            IWebElement footer = driver.FindElement(By.TagName("footer"));
            var scrollOrigin = new WheelInputDevice.ScrollOrigin
            {
                Element = footer,
                XOffset = 0,
                YOffset = -50
            };

            new Actions(driver).ScrollFromOrigin(scrollOrigin, 0, 200).Build().Perform();

            IWebElement iframe = driver.FindElement(By.TagName("iframe"));
            driver.SwitchTo().Frame(iframe);
            IWebElement checkbox = driver.FindElement(By.Name("scroll_checkbox"));
            Assert.IsTrue(IsInViewport(checkbox));
        }

        [Test]
        public void ShouldNotAllowScrollingWhenElementOriginOutOfViewport()
        {
            driver.Url = scrollFrameOutOfViewport;
            IWebElement footer = driver.FindElement(By.TagName("footer"));
            var scrollOrigin = new WheelInputDevice.ScrollOrigin
            {
                Element = footer,
                XOffset = 0,
                YOffset = 50
            };

            Assert.That(() => new Actions(driver).ScrollFromOrigin(scrollOrigin, 0, 200).Build().Perform(),
                Throws.InstanceOf<MoveTargetOutOfBoundsException>());
        }

        [Test]
        public void ShouldAllowScrollingFromViewportByGivenAmount()
        {
            driver.Url = scrollFrameOutOfViewport;
            IWebElement footer = driver.FindElement(By.TagName("footer"));
            int deltaY = footer.Location.Y;

            new Actions(driver).ScrollByAmount(0, deltaY).Build().Perform();

            Assert.IsTrue(IsInViewport(footer));
        }

        [Test]
        public void ShouldAllowScrollingFromViewportByGivenAmountFromOrigin()
        {
            driver.Url = scrollFrameInViewport;
            var scrollOrigin = new WheelInputDevice.ScrollOrigin
            {
                Viewport = true,
                XOffset = 10,
                YOffset = 10
            };

            new Actions(driver).ScrollFromOrigin(scrollOrigin, 0, 200).Build().Perform();

            IWebElement iframe = driver.FindElement(By.TagName("iframe"));
            driver.SwitchTo().Frame(iframe);
            IWebElement checkbox = driver.FindElement(By.Name("scroll_checkbox"));
            Assert.IsTrue(IsInViewport(checkbox));
        }

        [Test]
        public void ShouldNotAllowScrollingWhenOriginOffsetIsOutOfViewport()
        {
            driver.Url = scrollFrameInViewport;
            var scrollOrigin = new WheelInputDevice.ScrollOrigin
            {
                Viewport = true,
                XOffset = -10,
                YOffset = -10
            };

            Assert.That(() => new Actions(driver).ScrollFromOrigin(scrollOrigin, 0, 200).Build().Perform(),
                Throws.InstanceOf<MoveTargetOutOfBoundsException>());
        }

        private bool IsInViewport(IWebElement element)
        {
            String script =
                "for(var e=arguments[0],f=e.offsetTop,t=e.offsetLeft,o=e.offsetWidth,n=e.offsetHeight;\n"
                + "e.offsetParent;)f+=(e=e.offsetParent).offsetTop,t+=e.offsetLeft;\n"
                + "return f<window.pageYOffset+window.innerHeight&&t<window.pageXOffset+window.innerWidth&&f+n>\n"
                + "window.pageYOffset&&t+o>window.pageXOffset";
            IJavaScriptExecutor javascriptDriver = this.driver as IJavaScriptExecutor;

            return (bool)javascriptDriver.ExecuteScript(script, element);
        }
    }
}
