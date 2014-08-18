using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RatingId { get; set; }

        [Key]
        [Column(Order = 1)]
        public byte Rating { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int StoryResponseId { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? DateLeft { get; set; }

        public virtual StoryResponse StoryResponse { get; set; }
    }
}