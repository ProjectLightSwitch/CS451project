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
    public class Share624
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
        public void TheShare624Test()
        {
            driver.Navigate().GoToUrl(baseURL + "/");
            driver.FindElement(By.CssSelector("img[alt=\"StoryPortal\"]")).Click();
            driver.FindElement(By.LinkText("Sample Story Type title")).Click();
            driver.FindElement(By.Id("age")).Clear();
            driver.FindElement(By.Id("age")).SendKeys("14");
            driver.FindElement(By.CssSelector("span.ui-button-text")).Click();
            driver.FindElement(By.Id("radioM")).Click();
            driver.FindElement(By.LinkText("Next")).Click();
            driver.FindElement(By.Name("StoryTitle")).Clear();
            driver.FindElement(By.Name("StoryTitle")).SendKeys("My WAR story");
            driver.FindElement(By.Name("StoryResponse")).Clear();
            driver.FindElement(By.Name("StoryResponse")).SendKeys("WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR  then some more WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR WAR");
            driver.FindElement(By.XPath("(//input[@type='checkbox'])[2]")).Click();
            driver.FindElement(By.XPath("(//a[contains(text(),'Next')])[2]")).Click();
            driver.FindElement(By.Name("StoryAnswers[30]")).Clear();
            driver.FindElement(By.Name("StoryAnswers[30]")).SendKeys("Here is some answer");
            driver.FindElement(By.Name("StoryAnswers[31]")).Clear();
            driver.FindElement(By.Name("StoryAnswers[31]")).SendKeys("Here is some answer");
            driver.FindElement(By.Name("StoryAnswers[32]")).Clear();
            driver.FindElement(By.Name("StoryAnswers[32]")).SendKeys("Here is some answer");
            driver.FindElement(By.Name("StoryAnswers[33]")).Clear();
            driver.FindElement(By.Name("StoryAnswers[33]")).SendKeys("Here is some answer");
            driver.FindElement(By.CssSelector("input.next-section.button2")).Click();
            driver.FindElement(By.LinkText("View Responses")).Click();
            for (int second = 0;; second++) {
                if (second >= 60) Assert.Fail("timeout");
                try
                {
                    if (IsElementPresent(By.LinkText("My WAR story"))) break;
                }
                catch (Exception)
                {}
                Thread.Sleep(1000);
            }
            Assert.AreEqual("My WAR story", driver.FindElement(By.LinkText("My WAR story")).Text);
            driver.FindElement(By.LinkText("My WAR story")).Click();
            Assert.AreEqual("I am a 14 year old Male from United States.", driver.FindElement(By.CssSelector("p.personal-description")).Text);
            Assert.AreEqual("My Story:", driver.FindElement(By.XPath("//div[@id='story-content']/h3[3]")).Text);
            Assert.AreEqual("Here is some answer", driver.FindElement(By.XPath("//div[@id='story-content']/p[4]")).Text);
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
