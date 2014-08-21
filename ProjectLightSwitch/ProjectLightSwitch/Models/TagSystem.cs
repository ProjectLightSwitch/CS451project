using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProjectLightSwitch.Models.Enums;
using System.Web.Mvc;

namespace ProjectLightSwitch.Models
{
    public class TagSystem
    {

        #region Site Initialization
        public static void SeedData()
        {
            using (var context = new StoryModel())
            {
                context.Database.ExecuteSqlCommand("DELETE FROM Tags");
                context.Database.ExecuteSqlCommand("DELETE FROM TagTree");
                context.Database.ExecuteSqlCommand("DBCC CHECKIDENT (Tags, reseed, 0)");
                context.SaveChanges();

                AddTags(CreateInitialTags());
            }
        }

        internal static void UpdateLanguageStatuses(StoryModel context = null)
        {
            bool existingContext = context != null;
            context = context ?? new StoryModel();

            try
            {
                context.Database.ExecuteSqlCommand(@"
                    UPDATE Languages
                    SET IsActive = CASE
	                    WHEN LanguageId IN (SELECT t.TagId FROM Tags t
                    WHERE EXISTS(SELECT * FROM TranslatedTags tt WHERE t.TagId = tt.TagId AND tt.LanguageId = Languages.LanguageId)) THEN 1
                    ELSE 0
                    END");
            }
            finally 
            {
                if (!existingContext)
                {
                    context.Dispose();
                }
            }
        }

        internal static void UpdateTagTranslations(int tagId, Dictionary<int, string> translatedNames)
        {
            using (var context = new StoryModel())
            {
                var tag = context.Tags.FirstOrDefault(t => t.TagId == tagId);
                if (tag == null)
                {
                    return;
                }

                var translations = tag.TranslatedTags.ToList();
                foreach (int languageId in translatedNames.Keys)
                {
                    var current = translations.FirstOrDefault(t => t.LanguageId == languageId);
                    if (String.IsNullOrEmpty(translatedNames[languageId]))
                    { 
                        if(current != null)
                        {
                            // Remove the item if it exists
                            translations.Remove(current);
                        }
                    }
                    else if (current == null)
                    {
                        // Add the translation if it didn't exist
                        translations.Add(new TranslatedTag { TagId = tagId, Text = translatedNames[languageId], LanguageId = languageId });
                    }
                    else
                    {
                        // Edit the translation if it already existed
                        current.Text = translatedNames[languageId];
                    }
                }
                UpdateLanguageStatuses(context);
            }
        }

        private static IEnumerable<TagInputModel> CreateInitialTags()
        {
            const int numCategories = 4;
            const int numTags = 100;
            int numNavTags = 100;
            const int degree = numCategories;

            int idx = TagTree.InvisibleRootId;

            // CREATE ROOT
            yield return new TagInputModel { Tag = new Tag { TagType = (byte)TagType.InvisibleRoot, TagId = idx }, EnglishText = "[Root]", ParentId = -1 };

            int selStart = idx + 1;
            for (int i = 0; i < numCategories; i++)
            {
                idx++;
                yield return new TagInputModel { Tag = new Tag { TagType = (byte)TagType.Category, TagId = idx }, EnglishText = "cat_" + idx, ParentId = TagTree.InvisibleRootId };
            }

            int parent = selStart;
            for (int j = 0; j < numNavTags / 2; j++)
            {
                idx++;
                yield return new TagInputModel { Tag = new Tag { TagType = (byte)TagType.NavigationalTag, TagId = idx }, EnglishText = "nav_" + idx, ParentId = parent };
                idx++;
                yield return new TagInputModel { Tag = new Tag { TagType = (byte)TagType.NavigationalTag, TagId = idx }, EnglishText = "nav_" + idx, ParentId = parent };
                parent++;
            }
            int selEnd = idx;

            for (int i = selStart; i <= selEnd; i++)
            {
                idx++;
                yield return new TagInputModel { Tag = new Tag { TagType = (byte)TagType.SelectableTag, TagId = idx }, EnglishText = "tag_" + idx, ParentId = i };
                idx++;
                yield return new TagInputModel { Tag = new Tag { TagType = (byte)TagType.SelectableTag, TagId = idx }, EnglishText = "tag_" + idx, ParentId = i };
            }
        }

        #endregion

        //public class TagNavigatorDepthLevel
        //{
        //    Ancestor Parent { get; set; }
        //    IEnumerable<Ancestor> Children { get; set; }
        //}


        #region JSON methods

        //public static string GetPaths_Json(int rootId, string searchTerm, bool returnAllDescendants)
        //{
        //    const string delimiter = " > ";
            
        //    using (var context = new StoryModel())
        //    {
        //        context.Configuration.LazyLoadingEnabled = false;
        //        var q = context.TagTree
        //            .Where(tt =>
        //                (searchTerm == null || tt.Descendants.EnglishText.Contains(searchTerm))
        //                && (rootId == TagTree.InvisibleRootId || context.TagTree.Any(t => t.AncestorId == rootId && t.DescendantId == tt.DescendantId))
        //                && (returnAllDescendants || tt.PathLength == 1)
        //             )
        //            .GroupBy(group => group.DescendantId)
        //            .Select(group => new { TagId = group.Key, Nodes = group.OrderByDescending(t => t.PathLength).Select(t => t.Ancestors) })
        //            .ToList()
        //            .Select(t => new TagPathInfo
        //            {
        //                TagId = t.TagId,
        //                Nodes = t.Nodes,
        //                Path = String.Join(delimiter, t.Nodes.Select(tt => tt.EnglishText))
        //            })
        //            .OrderBy(t => t.Path);

        //        return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(q);
        //    }
        //}

        /// <summary>
        /// Gets the path to the selected tag
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="childrenOnly"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        public static String GetFullTagNavigationPath_Json(int tagId, bool childrenOnly, int languageId = Language.DefaultLanguageId)
        {
            using (var context= new StoryModel())
            {

                // TODO: Ordering?

                context.Configuration.LazyLoadingEnabled = false;
                var q =
                        context.TagTree.Where(tt =>
                            tt.PathLength <= 1
                            && context.TagTree
                            .Where(
                                tt2 => tt2.DescendantId == tagId
                                && (!childrenOnly || tt2.PathLength == 0)
                            )
                            .Select(tt2 => tt2.AncestorId)
                            .Contains(tt.AncestorId)
                        )
                        .GroupBy(tt => tt.Ancestor)
                        .Select(grouped => new
                        {
                            parent = new JSONTagModel
                            {
                                id = grouped.Key.TagId,
                                type = grouped.Key.TagType,
                                text =  grouped.Key.TranslatedTags
                                                   .Where(tt => tt.LanguageId == languageId)
                                                   .Select(t => t.Text)
                                                   .FirstOrDefault()
                            },
                            children = grouped
                                .Where(tt=>tt.PathLength != 0)
                                //.Where(g => g.DescendantId != tagId && g.DescendantId != TagTree.InvisibleRootId)
                                //.OrderBy(g => g.Descendant.TranslatedTags
                                //                          .Where(tt=>tt.LanguageId == languageId)
                                //                          .Select(tt=>tt.Text)
                                //                          .FirstOrDefault())
                                .Select(g => new JSONTagModel
                                {
                                    id = g.Descendant.TagId,
                                    type = g.Descendant.TagType,
                                    text = g.Descendant.TranslatedTags
                                                       .Where(tt => tt.LanguageId == languageId)
                                                       .Select(t => t.Text)
                                                       .FirstOrDefault()
                                })
                                .OrderBy(g=>g.text)
                        });

                return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(
                    new { results=q }
                );
            }
        }

        #endregion


        #region View Model Creation

        public static List<StorySearchResultModel> GetStorySearchResults(StorySearchInputModel searchModel)
        {
            using (var context = new StoryModel())
            {
                var minimumRecentDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(14));
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
                }).OrderByDescending(sr=>sr.RecentRating).ToList();
            }
        }
        

        public static void PopulateAvailableStoryTypes(StoryTypeResultsModel model)
        {

            if(model.LanguageId == 0)
            {
                model.LanguageId = Language.DefaultLanguageId;
            }
            if(String.IsNullOrEmpty(model.SearchTerm) || model.SearchTerm.Trim().Length == 0)
            {
                model.SearchTerm = null;
            }
            
            using (var context = new StoryModel())
            {
                // TODO: Add paging?
                model.StoryTypeModels = context.LocalizedStoryTypes
                       .OrderByDescending(s => s.StoryType.DateCreated)
                       .Where(s => 
                           s.LanguageId == model.LanguageId 
                           && s.Language.IsActive 
                           && (
                                model.SearchTerm == null 
                                || s.Title.Contains(model.SearchTerm)
                                || s.Description.Contains(model.SearchTerm)
                                || s.StoryType.Tags.SelectMany(t=>t.TranslatedTags).Any(t=>
                                        t.LanguageId == model.LanguageId 
                                        && t.Text.Contains(model.SearchTerm))
                           )
                        )
                       .Select(s => new StoryTypeResultModel { 
                           StoryTypeId = s.StoryTypeId,
                           TranslatedStoryTypeId = s.LocalizedStoryTypeId,
                           Title = s.Title,
                           Description = s.Description, 
                           Tags = s.StoryType.Tags.Select(t=> 
                               new JSONTagModel { 
                                   id = t.TagId, 
                                   type = t.TagType, 
                                   text = t.TranslatedTags
                                           .Where(tt=>tt.LanguageId == model.LanguageId)
                                           .Select(tt=>tt.Text).FirstOrDefault() }).ToList()
                       }).ToList();
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
                        var st = context.LocalizedStoryTypes.FirstOrDefault(s => s.LocalizedStoryTypeId == model.StoryType.TranslatedStoryTypeId);
                        if (st == null)
                        {
                            error = "Story type not found.";
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

        public static void PopulateStoryResponseModelOutput(ref StoryResponseViewModel model, int translatedStoryTypeId = -1)
        {
            using (var context = new StoryModel())
            {
                
                model = model ?? new StoryResponseViewModel();
                
                var q = context.LocalizedStoryTypes.Where(s=>s.LocalizedStoryTypeId == translatedStoryTypeId).FirstOrDefault();
                if(q == null)
                {
                    model = null;
                    return;
                }

                model.LanguageId = q.LanguageId;
                model.StoryType = new StoryTypeResultModel
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

        public static IEnumerable<Language> GetLanguages()
        {
            using (var context = new StoryModel())
            {
                return context.Languages.ToList();
            }
        }
        
        public static bool ChangeTagTranslation(int tagId, int languageId, string text)
        {
            // TODO: Improve performance (sql merge?) if time
            using (var context = new StoryModel())
            {
                if( !context.Tags.Any(t=>t.TagId == tagId) || !context.Languages.Any(l=>l.LanguageId == languageId))
                {
                    return false;
                }

                var tagTranslation = context.TranslatedTags.Where(tt => tt.TagId == tagId && tt.LanguageId == languageId).FirstOrDefault();
                if (tagTranslation != null)
                {
                    // Update existing tag
                    tagTranslation.Text = text;
                }
                else
                {
                    // Create a new tag
                    context.TranslatedTags.Add(new TranslatedTag() { TagId = tagId, Text = text, LanguageId = languageId });
                }
                context.SaveChanges();
                return true;
            }
        }

        public static IList<String> GetPathById(int id, int languageId)
        {
            using (var context = new StoryModel())
            {
                var q = from t in context.TagTree
                        where t.DescendantId == id
                        orderby t.PathLength descending
                        select t.Ancestor.TranslatedTags.Where(tt=>tt.LanguageId == languageId).Select(tt=>tt.Text).FirstOrDefault();
                return q.ToList();
            }
        }

        public static bool RemoveTag(int id)
        {
            // Don't delete root (from here at least)
            if (id == TagTree.InvisibleRootId)
            {
                return false;
            }

            using (var context = new StoryModel())
            {
                var tag = context.Tags.FirstOrDefault(t => t.TagId == id);
                if (tag == null)
                {
                    return false;
                }
                context.Tags.Remove(tag);
                return context.SaveChanges() > 0;
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
        public static Tag GetParent(int tagId)
        {
            using (var context = new StoryModel())
            {
                return context.TagTree.Where(tt => tt.DescendantId == tagId && tt.PathLength == 1).Select(tt=>tt.Ancestor).FirstOrDefault();
            }
        }

        # region Input Models

       
        #endregion


        public static int AddTag(TagType tagType, int parent, Dictionary<int, string> translatedNames)
        {
            return AddTag(new TagInputModel { Tag = new Tag { TagType = (byte)tagType }, TranslatedNames = translatedNames, ParentId = parent });
        }

        public static int AddTag(TagType tagType, int parent, string defaultLanguageName)
        {
            return AddTag(new TagInputModel { Tag = new Tag { TagType = (byte)tagType }, EnglishText = defaultLanguageName, ParentId = parent });
        }

        public static int AddTag(TagInputModel model)
        {
            var list = new List<TagInputModel>() {model};
            var result = AddTags(list).FirstOrDefault();
            return (result > 0) ? result : -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tags"></param>
        /// <returns>List of added IDs</returns>
        public static IEnumerable<int> AddTags(IEnumerable<TagInputModel> tags)
        {
            var retVal = new List<int>();
            using (var context = new StoryModel())
            {
                // Add the tags themselves first
                context.Tags.AddRange(tags.Select(t => t.Tag));
                context.SaveChanges();
                retVal = tags.Select(t => t.Tag.TagId).ToList();

                // TODO: check for duplicate tags by name

                // Add structure after tags have been saved
                foreach (var tag in tags)
                {
                    // Populate all ancestor-descendant relationships with added node
                    var q = (from t in context.TagTree
                                where tag.ParentId >= TagTree.InvisibleRootId && t.DescendantId == tag.ParentId
                                select new
                                {
                                    anc = t.AncestorId,
                                    des = tag.Tag.TagId,
                                    pathlen = (byte)(t.PathLength + 1)
                                }).ToList();

                    // Create the records to be added to the database
                    var list = q.Select(tt =>
                        new TagTree
                        {
                            AncestorId = tt.anc,
                            DescendantId = tt.des,
                            PathLength = tt.pathlen
                        }).ToList();
                    context.TagTree.AddRange(list);

                    context.TagTree.Add(new TagTree { PathLength = 0, AncestorId = tag.Tag.TagId, DescendantId = tag.Tag.TagId });

                    // Add text
                    foreach (var key in tag.TranslatedNames.Keys)
                    {
                        try
                        {
                            context.TranslatedTags.Add(
                                new TranslatedTag
                                {
                                    LanguageId = key,
                                    Text = tag.TranslatedNames[key],
                                    TagId = tag.Tag.TagId
                                });
                        }
                        catch (Exception)
                        { 
                            // This will handle any invalid LanguageIds 
                        }
                    }
                    context.SaveChanges();
                }
                return retVal;
            }
        }

        public static TagEditOutputModel GetTagEditOutputModel(int tagId)
        {
            if (tagId == TagTree.InvisibleRootId)
            {
                return new TagEditOutputModel();
            }

            using (var context = new StoryModel())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var tag = context.Tags.Where(t => t.TagId == tagId).FirstOrDefault();
                if(tag == null)
                {
                    return null;
                }

                return new TagEditOutputModel()
                {
                    Languages = context.Languages.ToList(),
                    Tag = tag,
                    Translations = 
                    context.Languages.ToDictionary(
                        l=>l.LanguageId, 
                        l=>tag.TranslatedTags
                              .Where(tt=>tt.LanguageId == l.LanguageId)
                              .Select(tt=>tt.Text)
                              .FirstOrDefault()
                    )


                        //context.Languages.GroupJoin(
                        //    context.TranslatedTags,
                        //    lang => lang.LanguageId,
                        //    tag => tag.LanguageId,
                        //    (lang, tag) => new { lang, tag }
                        //).ToDictionary(t=>t.lang.LanguageId, t=>t.tag
                        
                        //.SelectMany(
                        //    x => x.tag.DefaultIfEmpty(),
                        //    (x, tag) => new TagTranslationDetails
                        //    {
                        //        LanguageId = x.lang.LanguageId,
                        //        LanguageDescription = x.lang.Description,
                        //        LanguageCode = x.lang.Code,
                        //        TagText = tag.Text
                        //    }
                        //).ToDictionary(t=>t.)
                };
            }
        }

        //private class TagNameComparer : IEqualityComparer<Ancestor>
        //{
        //    bool IEqualityComparer<Ancestor>.Equals(Ancestor x, Ancestor y)
        //    {
        //        return  x != null 
        //                && y != null 
        //                && x.EnglishText.Equals(y.EnglishText, StringComparison.CurrentCultureIgnoreCase);
        //    }

        //    int IEqualityComparer<Ancestor>.GetHashCode(Ancestor obj)
        //    {
        //        return obj.EnglishText.GetHashCode();
        //    }
        //}

        //public static bool AddTags(IEnumerable<Ancestor> children, Ancestor parent)
        //{
        //    using (var context = new StoryModel())
        //    {
        //        // Double check the parent exists and is capable of containing children
        //        var actualParent = context.Tags.Where(t => t.TagId == parent.TagId).FirstOrDefault();
        //        if (
        //            actualParent == null 
        //            || actualParent.TagType == (byte)TagType.PendingSelectableTag 
        //            || actualParent.TagType == (byte)TagType.SelectableTag)
        //        {
        //            return false;
        //        }

        //        // TODO: Filter out duplicate names already stored in database

        //        // Add the tags themselves first
        //        context.Tags.AddRange(children);
        //        var result = context.SaveChanges() > 0;

        //        // Add structure after tags have been saved
        //        foreach (var tag in children)
        //        {
        //            if (actualParent.TagId < 0)
        //            {
        //                break;
        //            }

        //            var q = (from t in context.TagTree
        //                     where actualParent.TagId >= TagTree.InvisibleRootId && t.DescendantId == actualParent.TagId
        //                     select new
        //                     {
        //                         anc = t.AncestorId,
        //                         pathlen = (byte)(t.PathLength + 1)
        //                     }).ToList();

        //            var list = q.Select(tt =>
        //                new TagTree
        //                {
        //                    AncestorId = tt.anc,
        //                    DescendantId = tag.TagId,
        //                    PathLength = tt.pathlen
        //                }).AsEnumerable();

        //            if (tag.TagId != TagTree.InvisibleRootId)
        //            {
        //                context.TagTree.Add(new TagTree { PathLength = 0, AncestorId = tag.TagId, DescendantId = tag.TagId });
        //            }
        //            context.TagTree.AddRange(list);
        //        }
        //        result &= (context.SaveChanges() > 0);

        //        return result;
        //    }
        //}

        public class TagNavigatorColumnResults
        {
            public Tag ParentTag { get; set; }
            public IEnumerable<Tag> Children { get; set; }
        }

        public static IEnumerable<TagNavigatorColumnResults> GetChildrenFromPathEnd(int tagId)
        {
            using (var context = new StoryModel())
            {
                return context.TagTree
                        .Where(tt => context.TagTree
                                .Where(tt2 => tt2.DescendantId == tagId)
                                .Select(tt2 => tt2.AncestorId)
                                .Contains(tt.AncestorId)
                                && tt.PathLength == 1
                        )
                        .GroupBy(tt => tt.Ancestor)
                        .Select(grouped => new TagNavigatorColumnResults
                        {
                            ParentTag = grouped.Key,
                            Children = grouped.Select(g => g.Descendant).AsEnumerable()
                        }
                        )
                        .AsEnumerable();
            }
        }

        public static IEnumerable<Tag> GetCategories()
        {
            using (var context = new StoryModel())
            {
                return context.Tags.Where(t => t.TagType == (byte)TagType.Category).ToArray();
            }
        }

        public static IEnumerable<Tag> GetChildTags(int parentId, bool includePendingTags)
        {
            using (var context = new StoryModel())
            {
                context.Configuration.LazyLoadingEnabled = false;
                return context.TagTree.Include("Ancestors")
                    .Where(tt => tt.AncestorId == parentId && tt.PathLength == 1 && (tt.Descendant.TagType != (byte)TagType.PendingSelectableTag || includePendingTags))
                    .Select(tt => tt.Descendant).ToList();
            }
        }

//        public static void AddTagsSql(IEnumerable<Tuple<Ancestor, int>> tags)
//        {
//            using (var context = new StoryModel())
//            {
//                // Add the tags themselves first
//                var list = tags.ToList();
//                var list2 = tags.Where(t => t.Item1.TagId == 1).ToList();
//                context.Tags.AddRange(list.Select(t => t.Item1));
//                context.SaveChanges();

//                // TODO check for duplicates by name

//                foreach (var tag in list)
//                {
//                    string query = @"
//                        INSERT INTO TagTree (AncestorId, DescendantId, PathLength) VALUES ({0}, {0}, 0);";

//                    if (tag.Item2 != -1)
//                    {
//                        query += @"
//                        INSERT INTO TagTree
//                        SELECT AncestorId, DescendantId = {0}, PathLength = PathLength + 1
//                        FROM TagTree
//                        WHERE DescendantId = {1}";
//                    }

//                    context.Database.ExecuteSqlCommand(query, tag.Item1.TagId, tag.Item2);

//                }
//                context.SaveChanges();
//            }
//        }

        public class TagPathInfo
        {
            public int TagId { get; set; }
            public IEnumerable<Tag> Nodes { get; set; }

            public string Path { get; set; }
        }

        public static List<TagPathInfo> GetPaths(int rootId, string searchTerm, bool returnAllDescendants, int languageId = Language.DefaultLanguageId)
        {
            const string delimiter = " > ";
            using (var context = new StoryModel())
            {
                return context.TagTree
                    .Where(tt =>
                        (searchTerm == null || tt.Descendant.TranslatedTags.Any(t=>t.Text.Contains(searchTerm) && t.LanguageId == languageId))
                        && (rootId == TagTree.InvisibleRootId || context.TagTree.Any(t => t.AncestorId == rootId && t.DescendantId == tt.DescendantId))
                        && (returnAllDescendants || tt.PathLength == 1)
                     )
                    .GroupBy(group => group.DescendantId)
                    .Select(group => new { TagId = group.Key, Nodes = group.OrderByDescending(t => t.PathLength).Select(t => t.Ancestor) })
                    .ToList()
                    .Select(t => new TagPathInfo
                    {
                        TagId = t.TagId,
                        Nodes = t.Nodes,
                        Path = String.Join(delimiter, t.Nodes.Select(tt => tt.TranslatedTags.Where(trans=>trans.LanguageId == languageId)))
                    })
                    .OrderBy(t => t.Path)
                    .ToList();
            }
        }

        public static string GetPaths_Json(int rootId, string searchTerm, bool returnAllDescendants, int languageId)
        {
            //int resultLimit = 25;
            using (var context = new StoryModel())
            {
                // TODO: simplify query if possible
                // TODO: limit results?

                var q = context.TagTree
                    .Where(tt =>
                        (
                            searchTerm == null 
                            || (
                                 context.TranslatedTags
                                        .Where(trans => 
                                            trans.LanguageId == languageId 
                                            && trans.Text == searchTerm 
                                            && trans.TagId == tt.DescendantId)
                                        .Any()
                            )
                        )
                        && (rootId == TagTree.InvisibleRootId || context.TagTree.Any(t => t.AncestorId == rootId && t.DescendantId == tt.DescendantId))
                        && (returnAllDescendants || tt.PathLength == 1)
                        && tt.DescendantId != TagTree.InvisibleRootId
                     )
                    .GroupBy(group => group.DescendantId)
                    //.Select(group => new { TagId = group.Key, Nodes = group.OrderByDescending(t => t.PathLength).Select(t => t.Ancestors) })
                    //.ToList()
                    .Select(group => new
                    {
                        value = group.Key,
                        //Nodes = t.Nodes.Select(n => new { n.TagId, n.TagType }),
                        path = group.OrderByDescending(t => t.PathLength).Select(
                            tt => 
                                new {
                                    text = tt.Ancestor.TranslatedTags
                                             .Where(trans => trans.LanguageId == languageId)
                                             .Select(trans => trans.Text)
                                             .FirstOrDefault(),
                                    id = tt.AncestorId,
                                    type = tt.Ancestor.TagType
                                
                                }).ToList()
                    })
                    // NOTE: limiting results before sorting on client side will cause unpredictable results
                    //.Take(resultLimit)
                    //.OrderBy(t => t.label)
                    .ToList();

                var result = new { defLangId = SiteSettings.DefaultLanguageId, reqLangId = languageId != -1 ? languageId : SiteSettings.DefaultLanguageId, results = q };
                return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(q);    
            }
        }

        //public static List<Ancestor> GetTagsByType(
        //    bool showCategories = false,
        //    bool showTopLevelTags = false,
        //    bool showTags = false,
        //    bool ShowPending = false)
        //{
        //    // TODO Will pending top level tags and categories be possible?
        //    using (var context = new StoryModel())
        //    {
        //        return context.Tags.Where(t =>
        //                (ShowPending && t.TagType == (byte)TagType.PendingSelectableTag)
        //            || (showCategories && t.TagType == (byte)TagType.Category)
        //            || (showTopLevelTags && t.TagType == (byte)TagType.NavigationalTag)
        //            || (showTags && t.TagType == (byte)TagType.SelectableTag)
        //        ).OrderBy(t => t.EnglishText).ToList();
        //    }
        //}

        public static Tag GetTag(int id)
        {
            using (var context = new StoryModel())
            {
                return context.Tags.FirstOrDefault(t => t.TagId == id);
            }
        }

        //public IEnumerable<tag> GetDescendantBreadcrumbs(int rootId, string filter, bool onlyReturnChildren)
        //{
        //    using (var context = new TagModel())
        //    {

        //        var t = from t in 


        //        return (from t in context.TagTree
        //                where t.AncestorId == rootId && (!onlyReturnChildren || t.PathLength == 1)
        //                orderby t.PathLength, t.Descendants.TagType, t.Descendants.EnglishText
        //                select t.Descendants).ToList();
        //    }
        //}

        //public int AddTag(TagModel context, string english, string spanish = null, int tagId = 0, TagType type = TagType.tag)
        //{
        //    int result = 0;
        //    // For testing, not really needed
        //    //using (var transaction = context.Database.BeginTransaction())
        //    //{
        //        try
        //        {
        //            // Add tag
        //            var tag = new tag();
        //            tag.TagType = (byte)type;
        //            tag.EnglishText = english;
        //            if (spanish != null)
        //            {
        //                tag.TranslatedTags.Add(new TagsTranslated { LanguageCode = "es-mx", Text = spanish });
        //            }
        //            context.SaveChanges();

        //            // Add tree structure
        //            var q = (from t in context.TagTree
        //                        where tagId > 0 && t.DescendantId == tagId
        //                        select new { 
        //                            anc = t.AncestorId, 
        //                            des = tag.TagId, 
        //                            pathlen = (byte)(t.PathLength + 1) })
        //                    .AsEnumerable().Select(x => new TagTree { AncestorId = x.anc, DescendantId = x.des, PathLength = x.pathlen });

        //            context.TagTree.Add(new TagTree() { Ancestors = tag, Descendants = tag, PathLength = 0 });
        //            context.TagTree.AddRange(q);
        //            context.SaveChanges();
        //            //transaction.Commit();

        //            result = tag.TagId;
        //        //}
        //        //catch (Exception ex)
        //        //{
        //        //    transaction.Rollback();
        //        //}
        //    }
        //    return result;
        //}
    }
}