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
            Unknown = ' ',
            Male = 'M',
            Female = 'F',
            Intersex = 'I',
        }

        public static class EnumExtensions 
        { 
            public static Gender ParseGender(string gender)
            {
                try { return (Gender)gender[0]; }
                catch { return Gender.Unknown; }
            }
        }
}