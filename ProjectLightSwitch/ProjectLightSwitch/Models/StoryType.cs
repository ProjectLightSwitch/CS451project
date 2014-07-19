namespace ProjectLightSwitch.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

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

        public virtual ICollection<Question> Questions { get; set; }
    }
}
