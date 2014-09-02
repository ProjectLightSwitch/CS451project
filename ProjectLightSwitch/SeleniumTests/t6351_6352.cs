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
    public class T63516352
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
        public void TheT63516352Test()
        {
            driver.Navigate().GoToUrl(baseURL + "/");
            driver.FindElement(By.Id("loginLink")).Click();
            driver.FindElement(By.Id("UserName")).Clear();
            driver.FindElement(By.Id("UserName")).SendKeys("ericg");
            driver.FindElement(By.Id("Password")).Clear();
            driver.FindElement(By.Id("Password")).SendKeys("ericpass");
            driver.FindElement(By.CssSelector("input[type=\"submit\"]")).Click();
            Assert.AreEqual("Hello, ericg! Log off | Administration", driver.FindElement(By.Id("login")).Text);
            driver.FindElement(By.LinkText("Administration")).Click();
            driver.FindElement(By.LinkText("Users")).Click();
            for (int second = 0;; second++) {
                if (second >= 60) Assert.Fail("timeout");
                try
                {
                    if (IsElementPresent(By.CssSelector("input[type=\"submit\"]"))) break;
                }
                catch (Exception)
                {}
                Thread.Sleep(1000);
            }
            Assert.AreEqual("User:", driver.FindElement(By.CssSelector("div > form")).Text);
            Assert.AreEqual("", driver.FindElement(By.CssSelector("input[type=\"submit\"]")).Text);
            try
            {
                Assert.AreEqual("Add user as admin", driver.FindElement(By.CssSelector("input[type=\"submit\"]")).GetAttribute("value"));
            }
            catch (AssertionException e)
            {
                verificationErrors.Append(e.Message);
            }
            driver.FindElement(By.LinkText("Log off")).Click();
            Assert.AreEqual("Register | Log in", driver.FindElement(By.Id("login")).Text);
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
