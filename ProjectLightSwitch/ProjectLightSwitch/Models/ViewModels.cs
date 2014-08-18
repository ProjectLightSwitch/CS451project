using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectLightSwitch.Models
{
    #region Tag Creation

    
    public class TagInputModel
    {
        public Tag Tag { get; set; }
        public int ParentId { get; set; }
        public Dictionary<int, string> TranslatedNames = new Dictionary<int, string>();

        public string EnglishText
        {
            get { return TranslatedNames[Language.DefaultLanguageId]; }
            set { TranslatedNames[Language.DefaultLanguageId] = value; }
        }
    }

    #endregion

    public class JSONTagModel
    {
        public byte type { get; set; }
        public string text { get; set; }
        public int id { get; set; }
    }

    public class JSONChildrenModel
    {
        public JSONTagModel parent { get; set; }
        public IEnumerable<JSONTagModel> children { get; set; }
    }


    #region Tag Editing

    // To display tags with all translations for editing
    public class TagEditOutputModel
    {
        public List<Language> Languages { get; set; }
        public Tag Tag { get; set; }
        public Dictionary<int, string> Translations { get; set; }

        public TagEditOutputModel()
        {
            Translations = new Dictionary<int, string>();
        }
    }

    public class TagEditInputModel
    {
        public int TagId { get; set; }
        /// <summary>
        /// [languageID, text] of tag name
        /// </summary>
        public IDictionary<int, string> Names { get; set; }
    }
    #endregion
}