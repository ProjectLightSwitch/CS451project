namespace ProjectLightSwitch.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("pls.StoryInformation")]
    public partial class StoryInformation
    {
        public StoryInformation()
        {
            Answers = new HashSet<Answer>();
        }

        [Key]
        public int StoryId { get; set; }

        public int StoryTypeId { get; set; }

        public int? CountryId { get; set; }

        public byte? Age { get; set; }

        [StringLength(10)]
        public string Gender { get; set; }

        public string Story { get; set; }

        public int? LanguageId { get; set; }

        public virtual ICollection<Answer> Answers { get; set; }

        public virtual Language Language { get; set; }

        public virtual StoryInformation StoryInformation1 { get; set; }

        public virtual StoryInformation StoryInformation2 { get; set; }
    }
}
