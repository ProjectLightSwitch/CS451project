namespace ProjectLightSwitch.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("pls.Languages")]
    public partial class Language
    {
        public Language()
        {
            LocalizedStoryTypes = new HashSet<LocalizedStoryType>();
            TranslatedStrings = new HashSet<TranslatedString>();
            TranslatedTags = new HashSet<TranslatedTag>();
        }

        public int LanguageId { get; set; }

        [Required]
        [StringLength(8)]
        public string Code { get; set; }

        [Display(Name = "Language")]
        [Required]
        [StringLength(256)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<LocalizedStoryType> LocalizedStoryTypes { get; set; }

        public virtual ICollection<TranslatedString> TranslatedStrings { get; set; }

        public virtual ICollection<TranslatedTag> TranslatedTags { get; set; }
    }
}
