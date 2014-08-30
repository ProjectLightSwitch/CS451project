using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace ProjectLightSwitch.Models
{
    public partial class TagTree
    {
        public const int InvisibleRootId = 1;
    }

    public partial class Language
    {
        public const int DefaultLanguageId = 1;
    }

    public partial class StoryType
    {
        public const int DefaultStoryTypeId = 38;
    }

}