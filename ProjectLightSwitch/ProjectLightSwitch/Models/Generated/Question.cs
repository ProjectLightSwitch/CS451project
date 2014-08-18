namespace ProjectLightSwitch.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("pls.Questions")]
    public partial class Question
    {
        public Question()
        {
            Answers = new HashSet<Answer>();
        }

        public int QuestionId { get; set; }

        public int LocalizedStoryTypeId { get; set; }

        [StringLength(4000)]
        public string Options { get; set; }

        [StringLength(4000)]
        public string Prompt { get; set; }

        public virtual ICollection<Answer> Answers { get; set; }

        public virtual LocalizedStoryType LocalizedStoryType { get; set; }
    }
}
