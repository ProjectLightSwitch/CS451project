namespace ProjectLightSwitch.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("pls.StoryResponses")]
    public partial class StoryResponse
    {
        public StoryResponse()
        {
            Answers = new HashSet<Answer>();
            StoryResponseRatings = new HashSet<StoryResponseRating>();
            Tags = new HashSet<Tag>();
        }

        [Key]
        public int StoryResponseId { get; set; }

        public int LocalizedStoryTypeId { get; set; }

        public int? CountryId { get; set; }

        public byte? Age { get; set; }

        [StringLength(10)]
        public string Gender { get; set; }

        [Required]
        [StringLength(50)]
        public string Story { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreationDate { get; set; }

        [Required]
        [StringLength(1024)]
        public string Title { get; set; }

        public virtual ICollection<Answer> Answers { get; set; }

        public virtual LocalizedStoryType LocalizedStoryType { get; set; }

        public virtual ICollection<StoryResponseRating> StoryResponseRatings { get; set; }

        public virtual ICollection<Tag> Tags { get; set; }
    }
}
