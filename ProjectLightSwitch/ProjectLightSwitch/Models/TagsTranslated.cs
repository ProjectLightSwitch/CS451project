namespace ProjectLightSwitch.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("pls.TagsTranslated")]
    public partial class TagsTranslated
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TagId { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string LanguageCode { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(1024)]
        public string Text { get; set; }

        public virtual Tag Tag { get; set; }
    }
}
