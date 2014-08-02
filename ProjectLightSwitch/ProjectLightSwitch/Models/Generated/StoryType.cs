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

        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
        public int StoryTypeId { get; set; }

        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Computed)]
        public Guid DescriptionStringId { get; set; }

        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Computed)]
        public DateTime DateCreated { get; set; }

        public virtual ICollection<StoryResponse> StoryResponses { get; set; }

        public virtual ICollection<StoryTypeTag> StoryTypeTags { get; set; }
    }
}
