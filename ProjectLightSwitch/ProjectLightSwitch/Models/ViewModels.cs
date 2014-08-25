using System;
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

    public class StorySearchResultsModel
    {
        public int Page { get; set; }

        public int ResultsPerPage { get; set; }

        public int RecentDays { get; set; }

        public List<StorySearchResultModel> StorySearchResults { get; set; }

        public StorySearchResultsModel()
        {
            StorySearchResults = new List<StorySearchResultModel>();
            ResultsPerPage = SiteSettings.DefaultResultsPerPage;
            RecentDays = SiteSettings.DefaultRecentDays;
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

    public class StoryTypeResultsModel_OLD
    {
        public IEnumerable<StoryTypeResultModel_OLD> StoryTypeModels {get;set;}

        public int LanguageId { get; set; }

        [Display(Name = "Search by title, description, and tag")]
        public string SearchTerm { get; set; }

        public StoryTypeResultsModel_OLD()
        {
            StoryTypeModels = Enumerable.Empty<StoryTypeResultModel_OLD>();
        }
    }
    public class StoryTypeResultModel_OLD
    {
        public int TranslatedStoryTypeId { get; set; }
        public int StoryTypeId { get; set; }

        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name="Description")]
        public string Description { get; set; }
        [Display(Name = "Associated Tags")]
        public IEnumerable<JSONTagModel> Tags { get; set; }

        public StoryTypeResultModel_OLD()
        {
            Tags = Enumerable.Empty<JSONTagModel>();
        }
    }

    public class StoryTypesViewModel
    {
        public int TotalAvailableResults { get; set; }
        public string SearchTerm { get; set; }
        public int? LanguageId { get; set; }
        public IEnumerable<StoryTypeViewModel> StoryTypeViewModels { get; set; }

        [Range(0, int.MaxValue)]
        public int Page { get; set; }

        public int ResultsPerPage { get { return 500; } }

        public StoryTypesViewModel()
        {
            LanguageId = Language.DefaultLanguageId;
            StoryTypeViewModels = Enumerable.Empty<StoryTypeViewModel>();
            Page = 0;
        }
    }
    public class StoryTypeViewModel
    {
        public int StoryTypeId { get; set; }
        public DateTime DateCreated { get; set; }
        public IEnumerable<LocalizedStoryType> LocalizedStoryTypes { get; set; }
        public IEnumerable<JSONTagModel> Tags { get; set; }

        public object Tags2 { get; set; }


        public StoryTypeViewModel()
        {
            Tags = Enumerable.Empty<JSONTagModel>();
            LocalizedStoryTypes = Enumerable.Empty<LocalizedStoryType>();
        }
    
    }
    public class LocalizedStoryTypeViewModel
    {
        public int LocalizedStoryTypeId { get; set; }
        public List<JSONTagModel> Tags { get; set; }
        public LocalizedStoryType LocalizedStoryType { get; set; }
    }

    public class StoryTypeCreationModel
    {
        [Display(Name="Story Type Tags: ")]
        public List<int> SelectedTags { get; set; }

        [Required]
        public int LanguageId {get; set; }


        [MaxLength(1024)]
        [Display(Name = "Story Type Title: ")]
        [Required]
        public string Title { get; set; }

        [MaxLength(4000)]
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
        public StoryTypeResultModel_OLD StoryType { get; set; }
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

        public Dictionary<int, string> StoryAnswers { get; set; }

        public StoryResponseViewModel()
        {
            StoryQuestions = Enumerable.Empty<Question>();
            StoryAnswers = new Dictionary<int, string>();
            Countries = Enumerable.Empty<CountryListData>();  
        }

    }

    public class StoryStepModel
    {
        public int NumSteps { get; set; }
        public int CurrentStep { get; set; }
    }




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

    #region Tag Management

    public class TagViewModel
    {
        public Tag Tag { get; set; }
        public List<Language> Languages { get; set; }

        /// <summary>
        /// Use this dictionary for binding because the 
        /// </summary>
        public Dictionary<string, string> Translations {get;set;}

        public Dictionary<int, string> TranslationsWithIntKeys 
        {
            get
            {
                try
                {
                    return Translations.ToDictionary(kv => int.Parse(kv.Key), kv => kv.Value);
                }
                catch
                {
                    return new Dictionary<int, string>();
                }
            }
        }

        [Required]
        public string EnglishText 
        {
            get
            {
                return Translations.Where(kv => kv.Key == Language.DefaultLanguageId.ToString()).Select(kv => kv.Value).FirstOrDefault();
            }
            set 
            {
                if(!string.IsNullOrWhiteSpace(value))
                {
                    Translations[Language.DefaultLanguageId.ToString()] = value;
                }
            }
        }

        public TagViewModel()
        {
            Tag = new Tag();
            Translations = new Dictionary<string, string>();
        }
    }

  
    #endregion
}