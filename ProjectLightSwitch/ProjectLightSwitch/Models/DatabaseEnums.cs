using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectLightSwitch.Models.Enums
{
        public enum TagType : byte
        {
            Category = 1,
            TopLevelTag,
            Tag,
            PendingTag,
        }
        public enum Gender
        {
            Male = 'M',
            Female = 'F',
            Intersex = 'I',
        }
}