namespace TestSite.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("testing.Surveys")]
    public partial class Survey
    {
        public Survey()
        {
            Tags = new HashSet<Tag>();
        }

        public int SurveyId { get; set; }

        public byte? Age { get; set; }

        [StringLength(256)]
        public string Location { get; set; }

        [StringLength(1)]
        public string Sex { get; set; }

        [Required]
        [StringLength(4000)]
        public string Story { get; set; }

        public virtual ICollection<Tag> Tags { get; set; }
    }
}
