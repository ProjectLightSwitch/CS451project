using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SeleniumTests
{
    [TestFixture]
    public class ChromeT63326337
    {
        private IWebDriver driver;
        private StringBuilder verificationErrors;
        private string baseURL;
        private bool acceptNextAlert = true;
        
        [SetUp]
        public void SetupTest()
        {
            driver = new ChromeDriver();
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
        public void TheT63326337Test()
        {
            driver.Navigate().GoToUrl(baseURL + "/");
            driver.FindElement(By.Id("loginLink")).Click();
            driver.FindElement(By.Id("UserName")).Clear();
            driver.FindElement(By.Id("UserName")).SendKeys("ericg");
            driver.FindElement(By.Id("Password")).Clear();
            driver.FindElement(By.Id("Password")).SendKeys("ericpass");
            driver.FindElement(By.CssSelector("input[type=\"submit\"]")).Click();
            driver.FindElement(By.LinkText("Administration")).Click();
            driver.FindElement(By.LinkText("Tag Management")).Click();
            driver.FindElement(By.Name("model[0].EnglishText")).Clear();
            driver.FindElement(By.Name("model[0].EnglishText")).SendKeys("TestCategory21");
            driver.FindElement(By.CssSelector("input[type=\"submit\"]")).Click();
            for (int second = 0;; second++) {
                if (second >= 60) Assert.Fail("timeout");
                try
                {
                    if (IsElementPresent(By.CssSelector("#global-messages > div > ul > li"))) break;
                }
                catch (Exception)
                {}
                Thread.Sleep(1000);
            }
            Assert.AreEqual("Messages:", driver.FindElement(By.CssSelector("h2")).Text);
            for (int second = 0;; second++) {
                if (second >= 60) Assert.Fail("timeout");
                try
                {
                    if (IsElementPresent(By.XPath("//div[@id='tagnav']/div/div/ul/li[7]/span"))) break;
                }
                catch (Exception)
                {}
                Thread.Sleep(1000);
            }
            Assert.AreEqual("TestCategory21", driver.FindElement(By.XPath("//div[@id='tagnav']/div/div/ul/li[7]/span")).Text);
            driver.FindElement(By.XPath("(//a[contains(text(),'(Edit)')])[7]")).Click();
            try
            {
                Assert.AreEqual("TestCategory21", driver.FindElement(By.Id("Translations_1_")).GetAttribute("value"));
            }
            catch (AssertionException e)
            {
                verificationErrors.Append(e.Message);
            }
            driver.FindElement(By.Id("Translations_1_")).Clear();
            driver.FindElement(By.Id("Translations_1_")).SendKeys("TestCategory22");
            driver.FindElement(By.CssSelector("input.full-width")).Click();
            for (int second = 0;; second++) {
                if (second >= 60) Assert.Fail("timeout");
                try
                {
                    if (IsElementPresent(By.CssSelector("#global-messages > div > ul > li"))) break;
                }
                catch (Exception)
                {}
                Thread.Sleep(1000);
            }
            Assert.AreEqual("Messages:", driver.FindElement(By.CssSelector("h2")).Text);
            for (int second = 0;; second++) {
                if (second >= 60) Assert.Fail("timeout");
                try
                {
                    if (IsElementPresent(By.XPath("//div[@id='tagnav']/div/div/ul/li[7]/span"))) break;
                }
                catch (Exception)
                {}
                Thread.Sleep(1000);
            }
            Assert.AreEqual("TestCategory22", driver.FindElement(By.XPath("//div[@id='tagnav']/div/div/ul/li[7]/span")).Text);
            driver.FindElement(By.Id("model_0__EnglishText")).Click();
            driver.FindElement(By.XPath("//table[@id='addTextContainer']/tbody/tr[2]/td/div/label[2]/span")).Click();
            driver.FindElement(By.Id("model_0__EnglishText")).Clear();
            driver.FindElement(By.Id("model_0__EnglishText")).SendKeys("child1");
            driver.FindElement(By.Id("tt_1_nav")).Click();
            driver.FindElement(By.Id("model_1__EnglishText")).Clear();
            driver.FindElement(By.Id("model_1__EnglishText")).SendKeys("child2");
            driver.FindElement(By.XPath("//input[@value='Add children']")).Click();
            for (int second = 0;; second++) {
                if (second >= 60) Assert.Fail("timeout");
                try
                {
                    if (IsElementPresent(By.XPath("//div[@id='tagnav']/div/div[2]/ul/li/label"))) break;
                }
                catch (Exception)
                {}
                Thread.Sleep(1000);
            }
            Assert.AreEqual("child2", driver.FindElement(By.XPath("//div[@id='tagnav']/div/div[2]/ul/li[2]/span")).Text);
            Assert.AreEqual("child1", driver.FindElement(By.XPath("//div[@id='tagnav']/div/div[2]/ul/li/label")).Text);
            driver.FindElement(By.XPath("(//a[contains(text(),'(Edit)')])[9]")).Click();
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
            driver.FindElement(By.Id("Translations_1_")).Clear();
            driver.FindElement(By.Id("Translations_1_")).SendKeys("child3");
            driver.FindElement(By.CssSelector("input.full-width")).Click();
            for (int second = 0;; second++) {
                if (second >= 60) Assert.Fail("timeout");
                try
                {
                    if (IsElementPresent(By.CssSelector("label"))) break;
                }
                catch (Exception)
                {}
                Thread.Sleep(1000);
            }
            Assert.AreEqual("child3", driver.FindElement(By.CssSelector("label")).Text);
            driver.FindElement(By.XPath("(//a[contains(text(),'(Edit)')])[10]")).Click();
            for (int second = 0;; second++) {
                if (second >= 60) Assert.Fail("timeout");
                try
                {
                    if (IsElementPresent(By.XPath("//form[2]/section/h3"))) break;
                }
                catch (Exception)
                {}
                Thread.Sleep(1000);
            }
            Assert.AreEqual("Delete Tag", driver.FindElement(By.XPath("//form[2]/section/h3")).Text);
            driver.FindElement(By.CssSelector("section.tag-section > input[type=\"submit\"]")).Click();
            Assert.IsTrue(Regex.IsMatch(CloseAlertAndGetItsText(), "^Do you really want to delete this tag[\\s\\S]$"));
            for (int second = 0;; second++) {
                if (second >= 60) Assert.Fail("timeout");
                try
                {
                    if (IsElementPresent(By.XPath("//div[@id='tagnav']/div/div[2]/ul/li"))) break;
                }
                catch (Exception)
                {}
                Thread.Sleep(1000);
            }
            Assert.AreEqual("(Edit)child2>", driver.FindElement(By.XPath("//div[@id='tagnav']/div/div[2]/ul/li")).Text);
            driver.FindElement(By.XPath("(//a[contains(text(),'(Edit)')])[9]")).Click();
            for (int second = 0;; second++) {
                if (second >= 60) Assert.Fail("timeout");
                try
                {
                    if (IsElementPresent(By.XPath("//form[3]/section/h3"))) break;
                }
                catch (Exception)
                {}
                Thread.Sleep(1000);
            }
            driver.FindElement(By.CssSelector("section.tag-section > input[type=\"submit\"]")).Click();
            Assert.IsTrue(Regex.IsMatch(CloseAlertAndGetItsText(), "^Do you really want to delete this tag[\\s\\S]$"));
            for (int second = 0;; second++) {
                if (second >= 60) Assert.Fail("timeout");
                try
                {
                    if (IsElementPresent(By.XPath("//div[@id='tagnav']/div/div/ul/li[7]/span"))) break;
                }
                catch (Exception)
                {}
                Thread.Sleep(1000);
            }
            driver.FindElement(By.XPath("(//a[contains(text(),'(Edit)')])[7]")).Click();
            for (int second = 0;; second++) {
                if (second >= 60) Assert.Fail("timeout");
                try
                {
                    if (IsElementPresent(By.XPath("//form[3]/section/h3"))) break;
                }
                catch (Exception)
                {}
                Thread.Sleep(1000);
            }
            Assert.AreEqual("Delete Tag", driver.FindElement(By.XPath("//form[3]/section/h3")).Text);
            driver.FindElement(By.CssSelector("section.tag-section > input[type=\"submit\"]")).Click();
            Assert.IsTrue(Regex.IsMatch(CloseAlertAndGetItsText(), "^Do you really want to delete this tag[\\s\\S]$"));
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
