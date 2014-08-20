﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

using System.Web.Mvc;

namespace ProjectLightSwitch.Models
{
    public class StorySearchInputModel
    {
        public int TranslatedStoryTypeId { get; set; }
        public List<int> SelectedTags { get; set; }
        public int MinAge {get;set;}
        public int MaxAge {get;set;}

        public int Country { get; set; }

        private string _gender;
        [RegularExpression("^[MFI]?$")]
        public string Gender {
            get { return _gender;  }
            set 
            {
                string genderFlag = "";
                if (!string.IsNullOrEmpty(value)
                    && Enum.IsDefined(typeof(ProjectLightSwitch.Models.Enums.Gender), value[0]))
                {
                    genderFlag = value[0].ToString();
                }
            }
        }
        
        public StorySearchInputModel()
        {
            SelectedTags = new List<int>();
        }
    }

    public class StorySearchResultModel
    {
        public int TranslatedStoryTypeId { get; set; }

        public StoryResponse StoryResponse {get; set; }

        [Display(Name = "Recent Rating")]
        public int RecentRating { get; set; }

        [Display(Name="Overall Rating")]
        public int OverallRating { get; set; }


        public StorySearchResultModel()
        { 
        
        }
    }

    public class StoryTypeResultsModel
    {
        public IEnumerable<StoryTypeResultModel> StoryTypeModels {get;set;}

        public int LanguageId { get; set; }

        [Display(Name = "Search by title, description, and tag")]
        public string SearchTerm { get; set; }

        public StoryTypeResultsModel()
        {
            StoryTypeModels = Enumerable.Empty<StoryTypeResultModel>();
        }
    }
    public class StoryTypeResultModel
    {
        public int TranslatedStoryTypeId { get; set; }
        public int StoryTypeId { get; set; }

        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name="Description")]
        public string Description { get; set; }
        [Display(Name = "Associated Tags")]
        public IEnumerable<JSONTagModel> Tags { get; set; }

        public StoryTypeResultModel()
        {
            Tags = Enumerable.Empty<JSONTagModel>();
        }
    }

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
        // STORY TYPE
        public StoryTypeResultModel StoryType { get; set; }
        public IEnumerable<Question> StoryQuestions { get; set; }

        public int LanguageId { get; set; }

        // OUTPUT
        public IEnumerable<CountryListData> Countries { get; set; }
        public IEnumerable<SelectListItem> CountryListItems
        {
            get { return new SelectList(Countries, "CountryId", "CountryName"); }
        }

        // INPUT

        // Personal Info
        [Required]
        [Display(Name = "Age", ResourceType = typeof(Views.StoryPortal.StoryResources))]
        [Range(13, 150)]
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

        public IEnumerable<int> SelectedTags { get; set; }

        public IEnumerable<string> StoryAnswers { get; set; }

        public StoryResponseViewModel()
        {
            StoryQuestions = Enumerable.Empty<Question>();
            StoryAnswers = Enumerable.Empty<string>();
            Countries = Enumerable.Empty<CountryListData>();  
        }

    }

    public class StoryStepModel
    {
        public int NumSteps { get; set; }
        public int CurrentStep { get; set; }
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