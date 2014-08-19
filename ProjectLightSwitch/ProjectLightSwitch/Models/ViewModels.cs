using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

using System.Web.Mvc;

namespace ProjectLightSwitch.Models
{
    public class StoryTypeCreationModel
    {
        [Display(Name="Story Type Tags: ")]
        public List<int> SelectedTags { get; set; }

        [Required]
        public int LanguageId {get; set; }

        [Display(Name = "Story Type Description: ")]
        [Required]
        public string Description { get; set; }

        [Display(Name = "Enter the Story Type Questions: ")]
        public List<string> Questions { get; set; }
        public StoryTypeCreationModel()
        {
            Questions = new List<string>();
        }
    }

    public class CountryListData
    {
        public int CountryId { get; set; }
        [Display(Name = "Country", ResourceType = typeof(Views.StoryPortal.StoryResources))]
        public string CountryName { get; set; }
    }

    public class StoryResponseViewModel
    {
        // OUTPUT
        public List<CountryListData> Countries { get; set; }

        public IEnumerable<SelectListItem> CountryListItems
        {
            get { return new SelectList(Countries, "CountryId", "CountryName"); }
        }

        public string StoryTypeDescription { get; set; }
        public List<Question> Questions { get; set; }

        // INPUT

        // Personal Info
        [Required]
        [Display(Name = "Age", ResourceType = typeof(Views.StoryPortal.StoryResources))]
        [Range(10, 100)]
        public int Age { get; set; }

        [Required]
        [Display(Name = "Gender", ResourceType = typeof(Views.StoryPortal.StoryResources))]
        [RegularExpression("^[MFI]$")]
        public string Gender { get; set; }

        [Required]
        [Display(Name = "Country", ResourceType = typeof(Views.StoryPortal.StoryResources))]
        
        public int Country { get; set; }

        [Required]
        [Display(Name = "TitlePrompt", ResourceType = typeof(Views.StoryPortal.StoryResources))]
        public string StoryTitle { get; set; }

        [Required]
        [Display(Name = "ResponsePrompt", ResourceType = typeof(Views.StoryPortal.StoryResources))]
        public string StoryResponse { get; set; }

        public List<int> SelectedTags { get; set; }

        public List<string> Answers { get; set; }
    }

    #region Ancestor Creation
    
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


    #region Ancestor Editing

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