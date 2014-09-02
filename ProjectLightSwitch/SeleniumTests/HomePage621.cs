using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

namespace SeleniumTests
{
    [TestFixture]
    public class HomePage621
    {
        private IWebDriver driver;
        private StringBuilder verificationErrors;
        private string baseURL;
        private bool acceptNextAlert = true;
        
        [SetUp]
        public void SetupTest()
        {
            driver = new FirefoxDriver();
            baseURL = "http://localhost:5183";
            verificationErrors = new StringBuilder();
        }
        
        [TearDown]
        public void TeardownTest()
        {
            try
            {
                driver.Quit();
            }
            catch (Exception)
            {
                // Ignore errors if unable to close the browser
            }
            Assert.AreEqual("", verificationErrors.ToString());
        }
        
        [Test]
        public void TheHomePage621Test()
        {
            driver.Navigate().GoToUrl(baseURL + "/");
            // ERROR: Caught exception [ERROR: Unsupported command [selectWindow | null | ]]
            Assert.AreEqual("Mission", driver.FindElement(By.LinkText("Mission")).Text);
            Assert.AreEqual("About", driver.FindElement(By.LinkText("About")).Text);
            Assert.AreEqual("Projects", driver.FindElement(By.LinkText("Projects")).Text);
            Assert.AreEqual("Join Us", driver.FindElement(By.LinkText("Join Us")).Text);
            Assert.AreEqual("Future Plans", driver.FindElement(By.LinkText("Future Plans")).Text);
            Assert.AreEqual("Contact", driver.FindElement(By.LinkText("Contact")).Text);
            Assert.AreEqual("Log in", driver.FindElement(By.Id("loginLink")).Text);
            Assert.AreEqual("Story Portal", driver.FindElement(By.CssSelector("a > h2")).Text);
            Assert.AreEqual("Future Plans", driver.FindElement(By.CssSelector("#future-plans > h2")).Text);
            Assert.AreEqual("Join Us", driver.FindElement(By.CssSelector("#join-us > h2")).Text);
            Assert.AreEqual("Contact", driver.FindElement(By.CssSelector("h2.light")).Text);
        }
        private bool IsElementPresent(By by)
        {
            try
            {
                driver.FindElement(by);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
        
        private bool IsAlertPresent()
        {
            try
            {
                driver.SwitchTo().Alert();
                return true;
            }
            catch (NoAlertPresentException)
            {
                return false;
            }
        }
        
        private string CloseAlertAndGetItsText() {
            try {
                IAlert alert = driver.SwitchTo().Alert();
                string alertText = alert.Text;
                if (acceptNextAlert) {
                    alert.Accept();
                } else {
                    alert.Dismiss();
                }
                return alertText;
            } finally {
                acceptNextAlert = true;
            }
        }
    }
}
