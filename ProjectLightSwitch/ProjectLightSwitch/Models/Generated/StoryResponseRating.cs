namespace ProjectLightSwitch.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("pls.StoryResponseRatings")]
    public partial class StoryResponseRating
    {
        [Key]
        public int RatingId { get; set; }

        public byte Rating { get; set; }

        public int StoryResponseId { get; set; }

        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "datetime2")]
        public DateTime? DateLeft { get; set; }

        public virtual StoryResponse StoryRespons { get; set; }
    }
}
