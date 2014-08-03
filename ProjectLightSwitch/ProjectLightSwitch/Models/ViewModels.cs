using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectLightSwitch.Models
{

    #region Tag Editting 


    public class TagTranslationDetails
    {
        public int LanguageId { get; set; }
        public string LanguageDescription { get; set; }
        public string LanguageCode { get; set; }
        public string TagText { get; set; }
    }

    public class TagEditViewModel
    {
        public Tag Tag { get; set; }
        public IEnumerable<TagTranslationDetails> Translations { get; set; }

        public TagEditViewModel()
        {
            Translations = Enumerable.Empty<TagTranslationDetails>();
        }
    }

    public class TagEditChangeNameInputModel
    {
        public int TagId { get; set; }
        /// <summary>
        /// [languageID, text] of tag name
        /// </summary>
        public IDictionary<int, string> Names { get; set; }
    }

    public class TagEditAddChildrenInputModel
    {
        public int ParentId { get; set; }

        public List<Tag> Children { get; set; }
    }

    #endregion

}