//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TestSite.DBFirstModels
{
    using System;
    using System.Collections.Generic;
    
    public partial class Question
    {
        public Question()
        {
            this.Answers = new HashSet<Answer>();
        }
    
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public Nullable<int> StoryTypeId { get; set; }
    
        public virtual ICollection<Answer> Answers { get; set; }
        public virtual StoryType StoryType { get; set; }
    }
}
