namespace ProjectLightSwitch.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("pls.Answers")]
    public partial class Answer
    {
        public int AnswerId { get; set; }

        public int StoryId { get; set; }

        public int? QuestionId { get; set; }

        [StringLength(2048)]
        public string AnswerText { get; set; }

        public virtual StoryInformation StoryInformation { get; set; }

        public virtual Question Question { get; set; }
    }
}
