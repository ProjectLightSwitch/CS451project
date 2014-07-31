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
            Tags = new HashSet<Tag>();
        }

        [Key]
        public int StoryResponseId { get; set; }

        public int StoryTypeId { get; set; }

        public int? CountryId { get; set; }

        public byte? Age { get; set; }

        [StringLength(10)]
        public string Gender { get; set; }

        public string Story { get; set; }

        public int? LanguageId { get; set; }

        public virtual ICollection<Answer> Answers { get; set; }

        public virtual Language Language { get; set; }

        public virtual StoryType StoryType { get; set; }

        public virtual ICollection<Tag> Tags { get; set; }
    }
}
