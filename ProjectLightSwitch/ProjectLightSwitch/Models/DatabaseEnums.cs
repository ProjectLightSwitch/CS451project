using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectLightSwitch.Models.Enums
{
        public enum TagType : byte
        {
            InvisibleRoot = 0,
            Category,
            NavigationalTag,
            SelectableTag,
            PendingSelectableTag,
        }
        public enum Gender
        {
            Male = 'M',
            Female = 'F',
            Intersex = 'I',
        }
}