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
            LocalizedStoryTypes = new HashSet<LocalizedStoryType>();
            StoryTypeTags = new HashSet<StoryTypeTag>();
        }

        public int StoryTypeId { get; set; }

        public Guid DescriptionStringId { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DateCreated { get; set; }

        public virtual ICollection<LocalizedStoryType> LocalizedStoryTypes { get; set; }

        public virtual ICollection<StoryTypeTag> StoryTypeTags { get; set; }
    }
}
