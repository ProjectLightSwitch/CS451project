using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProjectLightSwitch.Models
{
    public class StoryPortalInfo
    {
        public byte Age { get; set; }

        [StringLength(10)]
        public string Gender { get; set; }

        public int ContryId { get; set; }

    }
}