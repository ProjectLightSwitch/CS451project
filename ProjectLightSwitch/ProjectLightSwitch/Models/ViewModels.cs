using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

using System.Web.Mvc;

namespace ProjectLightSwitch.Models
{


    #region Searching for user story responses

    public class StoryResponseSearchInputModel
    {
        /// <summary>
        /// The number of days used to calculate recent rating
        /// </summary>
        public int RecentDays { get { return SiteSettings.DefaultRecentDays; } }

        // Paging information
        public int Page { get; set; }
        public int ResultsPerPage { get; set; }

        /// <summary>
        /// Search for responses to this localized story type
        /// </summary>
        public int TranslatedStoryTypeId { get; set; }

        // Tag (story response and story type) search
        public List<int> SelectedTags { get; set; }
        public List<JSONTagPathModel> SelectedTagPaths { get; set; }

        // Personal information
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public int Country { get; set; }

        private string _gender;
        [RegularExpression("^[MFI]?$")]
        public string Gender
        {
            get { return _gender; }
            set
            {
                if (value != null && value.Length == 1 && "MFI".IndexOf(value) >= 0)
                {
                    _gender = value;
                }
            }
        }

        public StoryResponseSearchInputModel()
        {



            MinAge = 13;
            MaxAge = 100;


            SelectedTags = new List<int>();
            ResultsPerPage = SiteSettings.DefaultResultsPerPage;
        }
    }

    public class StoryResponseDetailsModel
    {

        /// <summary>
        /// Includes story type
        /// </summary>
        public StoryResponse Response { get; set; }
        public List<JSONTagPathModel> Tags { get; set; }
        public List<StoryResponse> RelatedResponses { get; set; }

        public StoryResponseDetailsModel()
        {
            Tags = new List<JSONTagPathModel>();
            RelatedResponses = new List<StoryResponse>();
        }
    }

    public class StoryResponseSearchOutputModel
    {
        
        public int LocalizedStoryTypeId { get; set; }

        public StoryResponse StoryResponse { get; set; }

        [Display(Name = "Recent Rating")]
        public int? RecentRating { get; set; }

        [Display(Name = "Overall Rating")]
        public int? OverallRating { get; set; }

        public LocalizedStoryType LocalizedStoryType { get; set; }


        [Display(Name = "Associated Tags")]
        public IEnumerable<JSONTagPathModel> Tags { get; set; }

        public StoryResponseSearchOutputModel()
        {
            Tags = Enumerable.Empty<JSONTagPathModel>();
            StoryResponse = new StoryResponse();

        }
    }
    
    public class StoryResponseSearchViewModel
    {
        public StoryResponseSearchInputModel SearchParameters { get; set; }
        public List<StoryResponseSearchOutputModel> Results { get; set; }
        public int TotalResultCount { get; set; }

        public StoryResponseSearchViewModel()
        {
            SearchParameters = new StoryResponseSearchInputModel();
            Results = new List<StoryResponseSearchOutputModel>();
        }
    }

    #endregion

    #region Search/browse story type available to complete
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
        public StoryTypeViewModel()
        {
            Tags = Enumerable.Empty<JSONTagModel>();
            LocalizedStoryTypes = Enumerable.Empty<LocalizedStoryType>();
        }
    
    }
    public class LocalizedStoryTypeViewModel
    {
        public List<JSONTagModel> Tags { get; set; }
        public LocalizedStoryType LocalizedStoryType { get; set; }
    }

    #endregion

    public class StoryTypeCreationModel
    {
        [Display(Name="Story Type Tags")]
        public List<int> SelectedTags { get; set; }

        /// <summary>
        /// If editing an existing translation
        /// </summary>
        public int LocalizedStoryTypeId { get; set; }
        /// <summary>
        /// Provide StoryTypeId and LanguageId to create a new or edit an existing translation
        /// </summary>
        public int StoryTypeId { get; set; }
        [Required]
        public int LanguageId {get; set; }


        [MaxLength(1024)]
        [Display(Name = "Story Type Title")]
        [Required]
        public string Title { get; set; }

        [MaxLength(4000)]
        [Display(Name = "Story Type Description")]
        [Required]
        public string Description { get; set; }

        [Display(Name = "Questions")]
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


    #region User story response entry

    public class StoryResponseCreationViewModel
    {
        // STORY TYPE
        public StoryTypeResultModel StoryTypeResultModel { get; set; }
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

        [StringLength(500)]
        [Required]
        [Display(Name = "TitlePrompt", ResourceType = typeof(Views.StoryPortal.StoryResources))]
        public string StoryTitle { get; set; }

        [StringLength(4000)]
        [Required]
        [Display(Name = "ResponsePrompt", ResourceType = typeof(Views.StoryPortal.StoryResources))]
        public string StoryResponse { get; set; }

        public IEnumerable<int> SelectedTags { get; set; }

        public Dictionary<string, string> StoryAnswers { get; set; }

        public StoryResponseCreationViewModel()
        {
            StoryQuestions = Enumerable.Empty<Question>();
            StoryAnswers = new Dictionary<string, string>();
            Countries = Enumerable.Empty<CountryListData>();  
        }

    }

    public class StoryStepModel
    {
        public int NumSteps { get; set; }
        public int CurrentStep { get; set; }
    }

    #endregion


    #region Tag Representations
    public class JSONTagPathModel
    {
        private string PathSeparator { get; set; }

        public List<JSONTagModel> path { get; set; }
        public byte type { get { return path.Select(p => p.type).LastOrDefault(); } }
        public string text { get { return path.Select(p => p.text).LastOrDefault(); } }
        public int id { get { return path.Select(p => p.id).LastOrDefault(); } }

        public string FullPathLabel {
            get 
            {
                return string.Join(PathSeparator, path.Select(p => p.text).ToList());
            }
        }


        /// <summary>
        /// Returns the first not followed by 
        /// </summary>
        public string AbbreviatedPathLabel
        {
            get
            {
                return string.Format("{0}{1}{2}",
                    path.Select(p => p.text).FirstOrDefault(),
                    PathSeparator,
                    string.Join(PathSeparator, path.Select(p => p.text).Skip(Math.Max(1, path.Count - 2)).Take(2).ToList()));
            }
        }

        public JSONTagPathModel()
        {
            PathSeparator = " > ";
        }

        public string Serialize()
        {
            return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(this);
        }
    }


    public class JSONTagModel
    {
        public byte type { get; set; }
        public string text { get; set; }
        public int id { get; set; }

        public string Serialize()
        {
            return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(this);
        }
    }

    public class JSONChildrenModel
    {
        public JSONTagModel parent { get; set; }
        public IEnumerable<JSONTagModel> children { get; set; }
    }

    #endregion


    #region Tag Ancestor Management

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