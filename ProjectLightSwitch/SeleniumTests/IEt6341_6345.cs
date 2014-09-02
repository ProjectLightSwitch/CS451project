using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Support.UI;

namespace SeleniumTests
{
    [TestFixture]
    public class IET63416345
    {
        private IWebDriver driver;
        private StringBuilder verificationErrors;
        private string baseURL;
        private bool acceptNextAlert = true;
        
        [SetUp]
        public void SetupTest()
        {
            driver = new InternetExplorerDriver();
            baseURL = "http://localhost:5183/";
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
        public void TheT63416345Test()
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
            driver.FindElement(By.LinkText("Story Type Management")).Click();
            for (int second = 0;; second++) {
                if (second >= 60) Assert.Fail("timeout");
                try
                {
                    if (IsElementPresent(By.LinkText("Create New"))) break;
                }
                catch (Exception)
                {}
                Thread.Sleep(1000);
            }
            Assert.AreEqual("Create New", driver.FindElement(By.LinkText("Create New")).Text);
            driver.FindElement(By.LinkText("Create New")).Click();
            for (int second = 0;; second++) {
                if (second >= 60) Assert.Fail("timeout");
                try
                {
                    if (IsElementPresent(By.CssSelector("h2"))) break;
                }
                catch (Exception)
                {}
                Thread.Sleep(1000);
            }
            Assert.AreEqual("Write Your Story", driver.FindElement(By.CssSelector("h2")).Text);
            Assert.AreEqual("Story Type Title", driver.FindElement(By.CssSelector("label")).Text);
            Assert.AreEqual("Story Type Description", driver.FindElement(By.XPath("//div[3]/label")).Text);
            try
            {
                Assert.AreEqual("Add another question", driver.FindElement(By.CssSelector("input[type=\"button\"]")).GetAttribute("value"));
            }
            catch (AssertionException e)
            {
                verificationErrors.Append(e.Message);
            }
            try
            {
                Assert.AreEqual("Create", driver.FindElement(By.CssSelector("input[type=\"submit\"]")).GetAttribute("value"));
            }
            catch (AssertionException e)
            {
                verificationErrors.Append(e.Message);
            }
            driver.FindElement(By.Id("Title")).Clear();
            driver.FindElement(By.Id("Title")).SendKeys("Selenium Test Story");
            driver.FindElement(By.Id("Description")).Clear();
            driver.FindElement(By.Id("Description")).SendKeys("This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story");
            driver.FindElement(By.Name("Questions[0]")).Clear();
            driver.FindElement(By.Name("Questions[0]")).SendKeys("Question of some sort");
            driver.FindElement(By.Name("Questions[1]")).Clear();
            driver.FindElement(By.Name("Questions[1]")).SendKeys("Another Question?");
            driver.FindElement(By.Name("Questions[2]")).Clear();
            driver.FindElement(By.Name("Questions[2]")).SendKeys("One before Last Question?");
            driver.FindElement(By.CssSelector("input[type=\"button\"]")).Click();
            driver.FindElement(By.Name("Questions[3]")).Clear();
            driver.FindElement(By.Name("Questions[3]")).SendKeys("Does the very very last question work?");
            driver.FindElement(By.CssSelector("input[type=\"submit\"]")).Click();
            driver.FindElement(By.LinkText("Selenium Test Story")).Click();
            Assert.AreEqual("Selenium Test Story", driver.FindElement(By.CssSelector("h2")).Text);
            Assert.AreEqual("This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story This is a selenium test story", driver.FindElement(By.CssSelector("p")).Text);
            Assert.IsTrue(Regex.IsMatch(driver.FindElement(By.XPath("//ol/li[2]")).Text, "^exact:Another Question[\\s\\S]$"));
            Assert.IsTrue(Regex.IsMatch(driver.FindElement(By.XPath("//ol/li[4]")).Text, "^exact:Does the very very last question work[\\s\\S]$"));
            driver.FindElement(By.LinkText("Go Back")).Click();
            driver.FindElement(By.LinkText("Log off")).Click();
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
