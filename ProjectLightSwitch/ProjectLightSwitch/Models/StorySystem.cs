﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectLightSwitch.Models
{
    public static class StorySystem
    {
        #region Story Types

        public static bool DeleteStoryType(int id)
        {
            using (var context = new StoryModel())
            { 
                var storyType = context.StoryTypes.FirstOrDefault(s=>s.StoryTypeId == id);
                var result = (storyType != null) && context.StoryTypes.Remove(storyType) != null;
                context.SaveChanges();
                return result;
            }
        }


        /// <summary>
        /// Deletes the specified LocalizedStoryType.
        /// If it was the last remaining translation of a StoryType, then it deletes the StoryTypeResultModel as well.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool DeleteLocalizedStoryType(int id)
        {
            using (var context = new StoryModel())
            {
                var storyType = context.LocalizedStoryTypes.FirstOrDefault(s => s.LocalizedStoryTypeId == id);

                if (storyType == null)
                {
                    return false;
                }

                bool result;

                // If this was the last translation for the story type, delete the story type as well
                if (storyType.StoryType.LocalizedStoryTypes.Count == 1)
                {
                    result = context.StoryTypes.Remove(storyType.StoryType) != null;
                }
                else
                {
                    result = context.LocalizedStoryTypes.Remove(storyType) != null;
                }
                
                context.SaveChanges();
                return result;
            }
        }

        /// <summary>
        /// Returns a LocalizedStoryTypeViewModel with a LocalizedStoryType that includes questions and language data
        /// as well as tags in the language of the localizedstorytype
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static LocalizedStoryTypeViewModel PopulateLocalizedStoryTypeViewModel(int id)
        {
            using (var context = new StoryModel())
            {
                var lst = context.LocalizedStoryTypes.Include("Language").Include("Questions").FirstOrDefault(s => s.LocalizedStoryTypeId == id);
                if (lst == null)
                {
                    return null;
                }

                return new LocalizedStoryTypeViewModel()
                {
                    LocalizedStoryType = lst,
                    Tags = lst.StoryType.Tags.Select(t => new JSONTagModel
                    {
                        id = t.TagId,
                        text = t.TranslatedTags.Where(tt => tt.LanguageId == lst.LanguageId).Select(tt => tt.Text).FirstOrDefault()
                    }).ToList()
                };
            }
        }

        public static void PopulateLocalizedStoryTypeModel(int id, LocalizedStoryTypeViewModel model)
        {
            using (var context = new StoryModel())
            {
                var q = context.LocalizedStoryTypes.Include("Language").Include("Questions").FirstOrDefault(s => s.LocalizedStoryTypeId == id);

                if (q == null)
                {
                    return;
                }
                model.LocalizedStoryType = q;
                model.Tags = q.StoryType.Tags.Select(t=>
                    new JSONTagModel { 
                            id = t.TagId, 
                            type = t.TagType, 
                            text = t.TranslatedTags
                                    .Where(tt=>tt.LanguageId == q.LanguageId)
                                    .Select(tt=>tt.Text)
                                    .FirstOrDefault()
                    }).ToList();
            }
        }

        public static void PopulateStoryTypesViewModel(StoryTypesViewModel model)
        {
            model.Page = Math.Max(0, model.Page);
            if(model.LanguageId == 0)
            {
                model.LanguageId = Language.DefaultLanguageId;
            }

            if(String.IsNullOrWhiteSpace(model.SearchTerm))
            {
                model.SearchTerm = null;
            }
            
            int tagLangId = model.LanguageId ?? Language.DefaultLanguageId;

            using (var context = new StoryModel())
            {
                context.Configuration.LazyLoadingEnabled = true;

                var q = context.LocalizedStoryTypes
                       .Where(s =>
                           (
                                model.LanguageId == null
                                || (s.LanguageId == model.LanguageId
                                && s.Language.IsActive))
                           && (
                                model.SearchTerm == null
                                || s.Title.Contains(model.SearchTerm)
                                || s.Description.Contains(model.SearchTerm)
                                || s.StoryType.Tags.SelectMany(t => t.TranslatedTags).Any(t =>
                                        t.LanguageId == model.LanguageId
                                        && t.Text.Contains(model.SearchTerm))
                           )
                        )

                        .OrderBy(s => s.StoryTypeId)
                        .ThenBy(s => s.LanguageId)
                        .GroupBy(lst => lst.StoryType);

                // TODO: Don't pass full localized story types with full descriptions for performance

                model.TotalAvailableResults = q.Count();
                model.StoryTypeViewModels =
                   q
                    .OrderBy(s => s.Key.StoryTypeId)
                    .Skip(model.Page * model.ResultsPerPage)
                    .Take(model.ResultsPerPage)
                    .ToList()
                    .Select(g => new StoryTypeViewModel
                    {
                        StoryTypeId = g.Key.StoryTypeId,
                        DateCreated = g.Key.DateCreated,
                        LocalizedStoryTypes = g.ToList(),
                        Tags = g.Key.Tags.Select(t => new JSONTagModel
                        {
                            id = t.TagId,
                            type = t.TagType,
                            text = t.TranslatedTags
                                    .Where(tt => tt.LanguageId == tagLangId)
                                    .Select(tt => tt.Text)
                                    .FirstOrDefault()
                        }).ToList()
                    }).ToList();
            }
        }

        public static string CreateStoryType(StoryTypeCreationModel model)
        {
            using (var context = new StoryModel())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var storyType = new StoryType();
                        context.StoryTypes.Add(storyType);
                        storyType.Tags = context.Tags.Where(t2 => model.SelectedTags.Contains(t2.TagId)).ToList();
                        //context.SaveChanges();

                        var localizedStoryType = new LocalizedStoryType
                        {
                            Title = model.Title,
                            Description = model.Description,
                            // TODO: Add real localization
                            LanguageId = Language.DefaultLanguageId, // model.LanguageId;
                            StoryType = storyType,
                        };
                        context.LocalizedStoryTypes.Add(localizedStoryType);

                        model.Questions.RemoveAll(q => string.IsNullOrWhiteSpace(q));
                        foreach (var question in model.Questions)
                        {
                            localizedStoryType.Questions.Add(new Question { Prompt = question });
                        }
                        context.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {

                        transaction.Rollback();
                        return "Error";
                    }
                }
                return null;
            }
        }

        public static string SaveStoryResponse(StoryResponseViewModel model)
        {
            string error = null;
            using (var context = new StoryModel())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var st = context.LocalizedStoryTypes.FirstOrDefault(s => s.LocalizedStoryTypeId == model.StoryTypeResultModel.TranslatedStoryTypeId);
                        if (st == null)
                        {
                            error = "Story storyType not found.";
                            throw new Exception(error);
                        }
                                                
                        //Get fresh set of questions
                        var questions = st.Questions.Select(q=>q.QuestionId).ToList();
                        if (questions.Count() != model.StoryAnswers.Values.Count)
                        {
                            error = "Not all questions were answered.";
                            throw new Exception(error);
                        }
                        var answers = new List<Answer>();
                        foreach (int questionId in questions)
                        { 
                            answers.Add(new Answer 
                            { 
                                QuestionId = questionId,
                                AnswerText = model.StoryAnswers[questionId],
                            });
                        }

                        var response = new StoryResponse() 
                        {
                            LocalizedStoryType = st,
                            Age = (byte)model.Age,
                            Gender = model.Gender,
                            CountryId = model.Country,
                            Title = model.StoryTitle,
                            Story = model.StoryResponse,
                            Tags = context.Tags.Where(t=>model.SelectedTags.Contains(t.TagId)).ToList(),
                            Answers = answers,
                        };
                        context.StoryResponses.Add(response);
                        if (context.SaveChanges() == 0)
                        {
                            error = "There was an error saving your story.";
                            throw new Exception(error);
                        }
                        transaction.Commit();
                        return null;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return error;
                    }
                }
            }
        }

        public static void PopulateStoryResponseModelOutput(int id, ref StoryResponseViewModel model)
        {
            using (var context = new StoryModel())
            {
                var q = context.LocalizedStoryTypes.Where(s=>s.LocalizedStoryTypeId == id).FirstOrDefault();
                if(q == null)
                {
                    model = null;
                    return;
                }

                model.LanguageId = q.LanguageId;
                model.StoryTypeResultModel = new StoryTypeResultModel
                {
                    Description = q.Description,
                    StoryTypeId = q.StoryTypeId,
                    Tags = q.StoryType.Tags.Select(t =>
                        new JSONTagModel
                        {
                            id = t.TagId,
                            type = t.TagType,
                            text = t.TranslatedTags
                                    .Where(tt => tt.LanguageId == q.LanguageId)
                                    .Select(tt=>tt.Text)
                                    .FirstOrDefault()
                        }),
                    Title = q.Title,
                    TranslatedStoryTypeId = q.LocalizedStoryTypeId,
                };

                model.Countries = context.Countries.Select(c =>
                    new CountryListData
                    {
                        CountryId = c.CountryId,
                        CountryName = c.Country1
                    }).ToList();

                model.StoryQuestions = q.Questions.ToList();
            }
        }

        #endregion

                #region Story Responses

        public static List<StorySearchResultModel> GetStorySearchResults(StorySearchInputModel searchModel, int page, int resultsPerPage, int recentDays)
        {
            using (var context = new StoryModel())
            {
                var minimumRecentDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(recentDays));
                //searchModel.
                return context.StoryResponses.Include("Tags").Where(r =>
                    r.LocalizedStoryType.Language.IsActive
                    && (searchModel.TranslatedStoryTypeId == 0 || r.LocalizedStoryTypeId == searchModel.TranslatedStoryTypeId)
                    && (searchModel.Gender.Length == 0 || r.Gender == searchModel.Gender)
                    && ((searchModel.MinAge == 0 || searchModel.MinAge <= r.Age) && (searchModel.MaxAge == 0 || searchModel.MaxAge >= r.Age))
                    && (
                        searchModel.SelectedTags.Count == 0
                        || !searchModel.SelectedTags
                                       .Any(t => r.LocalizedStoryType.StoryType.Tags
                                                  .Select(stt => stt.TagId)
                                                  .Union(searchModel.SelectedTags)
                                                  .Contains(t)
                                        )
                    )
                ).Select(sr => new StorySearchResultModel { 
                    StoryResponse = sr, 
                    OverallRating = context.StoryResponseRatings.Where(r=> r.StoryResponseId == sr.StoryResponseId).Sum(r=>r.Rating),
                    RecentRating = context.StoryResponseRatings.Where(r => r.StoryResponseId == sr.StoryResponseId && r.DateLeft >= minimumRecentDate).Sum(r=>r.Rating),
                    TranslatedStoryTypeId = sr.LocalizedStoryTypeId,
                }).OrderByDescending(sr=>sr.RecentRating).OrderByDescending(sr => sr.StoryResponse.CreationDate)
                  .Skip(resultsPerPage*page).Take(resultsPerPage).ToList();
            }
        }

        public static bool AddRating(int storyResponseId)
        {
            using (var context = new StoryModel())
            {
                var newRating = new StoryResponseRating();
                newRating.StoryResponseId = storyResponseId;
                newRating.Rating = 1;
                context.StoryResponseRatings.Add(newRating);
                return context.SaveChanges() > 0;
            }
        }
        
        #endregion


    }
}