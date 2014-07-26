using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace ProjectLightSwitch.Models.QuestionOptions
{

    public class MultipleChoice : QuestionOptions
    {
        public List<string> Options { get; set; }
        public int MinSelections { get; set; }
        public int MaxSelections { get; set; }
    }
    
    public class NumericSlider : QuestionOptions
    {
        int MinSelection { get; set; }
        int MaxSelection { get; set; }
        int InitialValue { get; set; }
    }

    public class FreeResponse : QuestionOptions
    {
        public int NumAnswersExpected { get; set; }            
    }

    public abstract class QuestionOptions
    {
        public string Serialize()
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(this);
        }

        public T Deserialize<T>(string data) where T : QuestionOptions
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<T>(data);
        }
    }
}