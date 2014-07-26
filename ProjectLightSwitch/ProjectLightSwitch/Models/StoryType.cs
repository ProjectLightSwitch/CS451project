namespace ProjectLightSwitch.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
using System.Web.Mvc;

    [Table("pls.StoryType")]
    public partial class StoryType
    {
        public StoryType()
        {
            Questions = new HashSet<Question>();
        }

        public int StoryTypeId { get; set; }

        [Required]
        public string Description { get; set; }


        [NotMapped]
        public static List<SelectListItem> QuestionTypeOptions {
            get 
            {
                var items = new List<SelectListItem>();
                items.Add(new SelectListItem() { Text = "Multiple Choice", Selected = true, Value = "mc" });
                items.Add(new SelectListItem() { Text = "Slider", Selected = false, Value = "slider" });
                items.Add(new SelectListItem() { Text = "Free Response", Selected = false, Value = "fr" });
                items.Add(new SelectListItem() { Text = "Multiple Choice", Selected = true, Value = "mc" });
                return items;
            }
        }

        public virtual ICollection<Question> Questions { get; set; }
    }
}
