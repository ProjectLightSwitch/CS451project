namespace ProjectLightSwitch.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("pls.TranslatedStrings")]
    public partial class TranslatedString
    {
        [Key]
        [Column(Order = 0)]
        public Guid TranslatedStringId { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LanguageId { get; set; }

        [Required]
        [StringLength(4000)]
        public string String { get; set; }

        public virtual Language Language { get; set; }
    }
}
