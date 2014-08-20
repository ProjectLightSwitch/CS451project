namespace ProjectLightSwitch.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("pls.LocalizedStoryTypes")]
    public partial class LocalizedStoryType
    {
        public LocalizedStoryType()
        {
            Questions = new HashSet<Question>();
            StoryResponses = new HashSet<StoryResponse>();
        }

        public int LocalizedStoryTypeId { get; set; }

        public int StoryTypeId { get; set; }

        public int LanguageId { get; set; }

        [StringLength(1024)]
        public string Title { get; set; }

        [StringLength(4000)]
        public string Description { get; set; }

        public virtual Language Language { get; set; }

        public virtual StoryType StoryType { get; set; }

        public virtual ICollection<Question> Questions { get; set; }

        public virtual ICollection<StoryResponse> StoryResponses { get; set; }
    }
}
