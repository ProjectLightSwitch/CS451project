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
            StoryResponses = new HashSet<StoryResponse>();
            TranslatedStrings = new HashSet<TranslatedString>();
        }

        public int LanguageId { get; set; }

        [Required]
        [StringLength(8)]
        public string Code { get; set; }

        [Required]
        [StringLength(256)]
        public string Description { get; set; }

        public virtual ICollection<StoryResponse> StoryResponses { get; set; }

        public virtual ICollection<TranslatedString> TranslatedStrings { get; set; }
    }
}
