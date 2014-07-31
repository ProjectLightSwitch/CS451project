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
            StoryResponses = new HashSet<StoryResponse>();
            StoryTypeTags = new HashSet<StoryTypeTag>();
        }

        public int StoryTypeId { get; set; }

        public Guid DescriptionStringId { get; set; }

        public DateTime DateCreated { get; set; }

        public virtual ICollection<StoryResponse> StoryResponses { get; set; }

        public virtual ICollection<StoryTypeTag> StoryTypeTags { get; set; }
    }
}
