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
            Descendants = new HashSet<TagTree>();
            Ancestors = new HashSet<TagTree>();
            TranslatedTags = new HashSet<TranslatedTag>();
            StoryResponses = new HashSet<StoryResponse>();
            StoryTypes = new HashSet<StoryType>();
        }
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
        public int TagId { get; set; }

        public byte TagType { get; set; }

        public virtual ICollection<TagTree> Descendants { get; set; }

        public virtual ICollection<TagTree> Ancestors { get; set; }

        public virtual ICollection<TranslatedTag> TranslatedTags { get; set; }

        public virtual ICollection<StoryResponse> StoryResponses { get; set; }

        public virtual ICollection<StoryType> StoryTypes { get; set; }
    }
}
