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
            Tags = new HashSet<Tag>();
        }

        public int StoryTypeId { get; set; }
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "datetime2")]
        public DateTime DateCreated { get; set; }

        public virtual ICollection<LocalizedStoryType> LocalizedStoryTypes { get; set; }

        public virtual ICollection<Tag> Tags { get; set; }
    }
}
