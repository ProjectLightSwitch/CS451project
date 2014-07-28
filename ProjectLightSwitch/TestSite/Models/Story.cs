using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestSite.Models
{
    public class Story
    {
        public Survey Survey { get; set; }
        public StoryType StoryType { get; set; }
    }
}