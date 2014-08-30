namespace ProjectLightSwitch.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("pls.Countries")]
    public partial class Country
    {
        public Country()
        {
            StoryResponses = new HashSet<StoryResponse>();        
        }

        public int CountryId { get; set; }

        [Column("Country")]
        [StringLength(512)]
        public string Country1 { get; set; }

        public virtual ICollection<StoryResponse> StoryResponses { get; set; }
    }
}
