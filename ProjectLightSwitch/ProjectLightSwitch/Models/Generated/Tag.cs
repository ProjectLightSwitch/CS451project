namespace ProjectLightSwitch.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("pls.Tags")]
    public partial class Tag
    {
        public Tag()
        {
            StoryTypeTags = new HashSet<StoryTypeTag>();
            TranslatedTags = new HashSet<TranslatedTag>();
            Ancestors = new HashSet<TagTree>();
            Descendants = new HashSet<TagTree>();
            StoryResponses = new HashSet<StoryResponse>();
        }

        public int TagId { get; set; }

        public byte TagType { get; set; }

        [Required]
        [StringLength(1024)]
        public string EnglishText { get; set; }

        public virtual ICollection<StoryTypeTag> StoryTypeTags { get; set; }

        public virtual ICollection<TranslatedTag> TranslatedTags { get; set; }

        public virtual ICollection<TagTree> Ancestors { get; set; }

        public virtual ICollection<TagTree> Descendants { get; set; }

        public virtual ICollection<StoryResponse> StoryResponses { get; set; }
    }
}
