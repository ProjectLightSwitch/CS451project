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
        private static IEnumerable<TagViewModel> CreateInitialTags()
        {
            const int numCategories = 4;
            int numNavTags = 100;
            int idx = TagTree.InvisibleRootId;

            // CREATE ROOT
            yield return new TagViewModel { Tag= new Tag() { TagType = (byte)TagType.InvisibleRoot, TagId = idx }, EnglishText = "[Root]" };

            int selStart = idx + 1;
            for (int i = 0; i < numCategories; i++)
            {
                idx++;
                yield return new TagViewModel { Tag = new Tag() { TagType = (byte)TagType.Category, TagId = TagTree.InvisibleRootId }, EnglishText = "cat_" + idx };
            }

            int parent = selStart;
            for (int j = 0; j < numNavTags / 2; j++)
            {
                idx++;
                yield return new TagViewModel { Tag = new Tag() { TagType = (byte)TagType.NavigationalTag, TagId = parent }, EnglishText = "nav_" + idx };
                idx++;
                yield return new TagViewModel { Tag = new Tag() { TagType = (byte)TagType.NavigationalTag, TagId = parent }, EnglishText = "nav_" + idx };
                parent++;
            }
            int selEnd = idx;

            for (int i = selStart; i <= selEnd; i++)
            {
                idx++;
                yield return new TagViewModel { Tag = new Tag() { TagType = (byte)TagType.SelectableTag, TagId = i }, EnglishText = "tag_" + idx };
                idx++;
                yield return new TagViewModel { Tag = new Tag() { TagType = (byte)TagType.SelectableTag, TagId = i }, EnglishText = "tag_" + idx };
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
                    WHERE EXISTS(SELECT * FROM TranslatedTags jt WHERE t.TagId = jt.TagId AND jt.LanguageId = Languages.LanguageId)) THEN 1
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

      
        #endregion

        #region JSON methods
        /// <summary>
        /// Gets the path to the selected tag
        /// </summary>
        /// <param name="tagid"></param>
        /// <param name="childrenOnly"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        public static String GetFullTagNavigationPath_Json(int tagId, bool childrenOnly, int languageId = Language.DefaultLanguageId)
        {
            using (var context= new StoryModel())
            {

                // TODO: Sorting?

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
                            parent = new JSONTagModel { 
                                id = grouped.Key.TagId, 
                                type = grouped.Key.TagType, 
                                text = grouped.Key.TranslatedTags
                                                  .Where(tt => tt.LanguageId == languageId)
                                                  .Select(tt => tt.Text)
                                                  .FirstOrDefault() 
                            },
                            children = grouped
                                .Where(tt => tt.PathLength != 0)
                                //.Where(g => g.DescendantId != tagid && g.DescendantId != TagTree.InvisibleRootId)
                                //.OrderBy(g => g.Descendant.TranslatedTags
                                //                          .Where(jt=>jt.LanguageId == languageId)
                                //                          .Select(jt=>jt.Text)
                                //                          .FirstOrDefault())
                                .Select(g => new JSONTagModel { 
                                    id = g.Descendant.TagId,
                                    type = g.Descendant.TagType,
                                    text = g.Descendant.TranslatedTags
                                                       .Where(tt=>tt.LanguageId == languageId)
                                                       .Select(tt=>tt.Text)
                                                       .FirstOrDefault(),
                                })
                                .OrderBy(g => g.text)
                        });

                return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(
                    new { results=q }
                );
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
                                            && trans.Text.Contains(searchTerm)
                                            && trans.TagId == tt.DescendantId)
                                        .Any()
                            )
                        )
                        && (rootId == TagTree.InvisibleRootId || context.TagTree.Any(t => t.AncestorId == rootId && t.DescendantId == tt.DescendantId))
                        && (returnAllDescendants || tt.PathLength == 1)
                        && tt.DescendantId != TagTree.InvisibleRootId
                     )
                    .GroupBy(group => group.DescendantId)
                    //.Select(group => new { TagId = group.Key, Nodes = group.OrderByDescending(t => t.PathLength).Select(t => t.Descendants) })
                    //.ToList()
                    .Select(group => new
                    {
                        value = group.Key,
                        path = group.Where(p=>p.Ancestor.TagId != TagTree.InvisibleRootId).Select(p=>
                            new JSONTagModel { 
                                id = p.Ancestor.TagId, 
                                text = p.Ancestor.TranslatedTags.Where(tt => tt.LanguageId == languageId).Select(tt=>tt.Text).FirstOrDefault(), 
                                type = (byte)p.Ancestor.TagType 
                            }
                        ).ToList()
                    })
                    // NOTE: limiting results before sorting on client side will cause unpredictable results
                    //.Take(resultLimit)
                    //.OrderBy(t => t.label)
                    .ToList();
                return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(q);
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

        internal static void EditTag(int tagId, IDictionary<int, string> translatedNames)
        {
            using (var context = new StoryModel())
            {
                var tag = context.Tags.FirstOrDefault(t => t.TagId == tagId);
                if (tag == null)
                {
                    return;
                }

                //var translations = tag.TranslatedTags.ToList();
                foreach (int languageId in translatedNames.Keys)
                {
                    var current = tag.TranslatedTags.FirstOrDefault(t => t.LanguageId == languageId);
                    if (String.IsNullOrEmpty(translatedNames[languageId]))
                    {
                        if (current != null)
                        {
                            // Remove the item if it exists
                            tag.TranslatedTags.Remove(current);
                        }
                    }
                    else if (current == null)
                    {
                        // Add the translation if it didn't exist
                        tag.TranslatedTags.Add(new TranslatedTag { TagId = tagId, Text = translatedNames[languageId], LanguageId = languageId });
                    }
                    else
                    {
                        // Edit the translation if it already existed
                        current.Text = translatedNames[languageId];
                    }
                }
                context.SaveChanges();
                UpdateLanguageStatuses(context);
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


        public static Tag GetParent(int tagId)
        {
            using (var context = new StoryModel())
            {
                return context.TagTree.Where(tt => tt.DescendantId == tagId && tt.PathLength == 1).Select(tt=>tt.Ancestor).FirstOrDefault();
            }
        }

        public static bool AddTag(TagViewModel model)
        {
            var list = new List<TagViewModel>() { model };
            return AddTags(list) == 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tags"></param>
        /// <returns>The number of tags added</returns>
        public static int AddTags(IEnumerable<TagViewModel> tags)
        {
            var retVal = new List<int>();
            int numAdded = 0;

            using (var context = new StoryModel())
            {
            
                // Validate tag
                // *** Each tag's id refers to the parent not to self ***
                var tagsToAdd = tags
                    .Where(t=>
                        (
                        // Valid tag storyType
                            t.Tag.TagType == (byte)Enums.TagType.Category 
                            || t.Tag.TagType == (byte)Enums.TagType.NavigationalTag 
                            || t.Tag.TagType == (byte)Enums.TagType.SelectableTag
                        )
                        // Default text exists
                        && !string.IsNullOrWhiteSpace(t.EnglishText)
                        // Parent exists
                        && context.Tags.Any(tag=>
                            tag.TagId == t.Tag.TagId 
                            && (
                                tag.TagType == (byte)Enums.TagType.InvisibleRoot
                                || tag.TagType == (byte)Enums.TagType.Category 
                                || tag.TagType == (byte)Enums.TagType.NavigationalTag)))
                    .Select(t=> new { 
                        Tag = new Tag{ TagType = t.Tag.TagType },
                        ParentId = t.Tag.TagId,
                        EnglishText = t.EnglishText,
                        Translations = t.TranslationsWithIntKeys,
                    }).ToList();

                // Add the actual tags
                context.Tags.AddRange(tagsToAdd.Select(t=>t.Tag));
                context.SaveChanges();

                
                // Add structure after tags have been saved
                foreach (var tag in tagsToAdd)
                {
                    // Ignore selectable tags, tags with no default text, the invisible root, and duplicate tags
                    var parent = context.Tags.FirstOrDefault(t => t.TagId == tag.ParentId);
                    if (
                        parent.Ancestors.Any(tt=>
                            tt.PathLength == 1 
                            && tt.Descendant.TranslatedTags
                                            .Any(d=>
                                                d.LanguageId == Language.DefaultLanguageId 
                                                && d.Text == tag.EnglishText))
                        || string.IsNullOrWhiteSpace(tag.EnglishText)
                        || parent == null
                        || parent.TagType == (byte)TagType.SelectableTag
                        || parent.TagType == (byte)TagType.PendingSelectableTag)
                    {
                        continue;
                    }

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
                    foreach (var key in tag.Translations.Keys)
                    {
                        try
                        {
                            context.TranslatedTags.Add(
                                new TranslatedTag
                                {
                                    LanguageId = key,
                                    Text = tag.Translations[key],
                                    TagId = tag.Tag.TagId
                                });
                        }
                        catch (Exception)
                        {
                            // This will handle any invalid LanguageIds 
                        }
                    }
                    numAdded++;
                }
                context.SaveChanges();
                return numAdded;
            }
        }

        public static TagViewModel GetTagOutputViewModel(int tagId)
        {
            if (tagId == TagTree.InvisibleRootId)
            {
                throw new ArgumentException("You cannot edit this tag.");
            }

            using (var context = new StoryModel())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var tag = context.Tags.Include("TranslatedTags").Where(t => t.TagId == tagId).FirstOrDefault();
                if(tag == null)
                {
                    return null;
                }
                var translations = tag.TranslatedTags.ToDictionary(l=>l.LanguageId.ToString(), l=>l.Text);
                return new TagViewModel()
                {
                    Tag = tag,
                    Languages = context.Languages.ToList(),
                    Translations = translations,
                };
            }
        }

        //public static IEnumerable<TagNavigatorColumnResults> GetChildrenFromPathEnd(int tagid)
        //{
        //    using (var context = new StoryModel())
        //    {
        //        return context.TagTree
        //                .Where(jt => context.TagTree
        //                        .Where(tt2 => tt2.DescendantId == tagid)
        //                        .Select(tt2 => tt2.AncestorId)
        //                        .Contains(jt.AncestorId)
        //                        && jt.PathLength == 1
        //                )
        //                .GroupBy(jt => jt.Ancestor)
        //                .Select(grouped => new TagNavigatorColumnResults
        //                {
        //                    ParentTag = grouped.Key,
        //                    Children = grouped.Select(g => g.Descendant).AsEnumerable()
        //                }
        //                )
        //                .AsEnumerable();
        //    }
        //}

        //public static IEnumerable<Tag> GetCategories()
        //{
        //    using (var context = new StoryModel())
        //    {
        //        return context.Tags.Where(t => t.TagType == (byte)TagType.Category).ToArray();
        //    }
        //}

        //public static IEnumerable<Tag> GetChildTags(int parentId, bool includePendingTags)
        //{
        //    using (var context = new StoryModel())
        //    {
        //        context.Configuration.LazyLoadingEnabled = false;
        //        return context.TagTree.Include("Descendants")
        //            .Where(jt => jt.AncestorId == parentId && jt.PathLength == 1 && (jt.Descendant.TagType != (byte)TagType.PendingSelectableTag || includePendingTags))
        //            .Select(jt => jt.Descendant).ToList();
        //    }
        //}

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


        public static Tag GetTag(int id)
        {
            using (var context = new StoryModel())
            {
                return context.Tags.FirstOrDefault(t => t.TagId == id);
            }
        }

    }
}