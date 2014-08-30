using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjectLightSwitch
{
    public static class HelperFunctions
    {

        public static void AddGlobalMessage(TempDataDictionary TempData, string message)
        {
            var messages = TempData["UserMessages"] as List<string>;
            if (messages == null)
            {
                TempData["UserMessages"] = messages = new List<string>();
            }
            messages.Add(message);
        }

        public static IEnumerable<string> GetGlobalMessages(TempDataDictionary TempData)
        {
            var messages = TempData["UserMessages"] as List<string>;
            if (messages == null)
            {
                return new List<string>();
            }
            return messages;
        }
    }
}