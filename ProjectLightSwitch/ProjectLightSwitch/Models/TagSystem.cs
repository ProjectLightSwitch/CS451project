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

                AddTagsSql(CreateInitialTags());
            }
        }

        private static IEnumerable<Tuple<Tag, int>> CreateInitialTags()
        {
            const int numCategories = 4;
            const int numTags = 100;
            int numNavTags = 100;
            const int degree = numCategories;

            int idx = TagTree.InvisibleRootId;

            // CREATE ROOT
            yield return Tuple.Create<Tag, int>(new Tag { TagType = (byte)TagType.InvisibleRoot, TagId = idx, EnglishText = "[Root]" }, -1);

            int selStart = idx + 1;
            for (int i = 0; i < numCategories; i++)
            {
                idx++;
                yield return Tuple.Create<Tag, int>(new Tag { TagType = (byte)TagType.Category, TagId = idx, EnglishText = "cat_" + idx }, TagTree.InvisibleRootId);
            }

            int parent = selStart;
            for (int j = 0; j < numNavTags / 2; j++)
            {
                idx++;
                yield return Tuple.Create<Tag, int>(new Tag { TagId = idx, TagType = (byte)TagType.NavigationalTag, EnglishText = "nav_" + idx }, parent);
                idx++;
                yield return Tuple.Create<Tag, int>(new Tag { TagId = idx, TagType = (byte)TagType.NavigationalTag, EnglishText = "nav_" + idx }, parent);
                parent++;
            }
            int selEnd = idx;

            for (int i = selStart; i <= selEnd; i++)
            {
                idx++;
                yield return Tuple.Create<Tag, int>(new Tag { TagId = idx, TagType = (byte)TagType.SelectableTag, EnglishText = "tag_" + idx }, i);
                idx++;
                yield return Tuple.Create<Tag, int>(new Tag { TagId = idx, TagType = (byte)TagType.SelectableTag, EnglishText = "tag_" + idx }, i);
            }
        }

        #endregion

        public class TagNavigatorDepthLevel
        {
            Tag Parent { get; set; }
            IEnumerable<Tag> Children { get; set; }
        }


        #region JSON methods

        //public static string GetPaths_Json(int rootId, string searchTerm, bool returnAllDescendants)
        //{
        //    const string delimiter = " > ";
            
        //    using (var context = new StoryModel())
        //    {
        //        context.Configuration.LazyLoadingEnabled = false;
        //        var q = context.TagTree
        //            .Where(tt =>
        //                (searchTerm == null || tt.Descendant.EnglishText.Contains(searchTerm))
        //                && (rootId == TagTree.InvisibleRootId || context.TagTree.Any(t => t.AncestorId == rootId && t.DescendantId == tt.DescendantId))
        //                && (returnAllDescendants || tt.PathLength == 1)
        //             )
        //            .GroupBy(group => group.DescendantId)
        //            .Select(group => new { TagId = group.Key, Nodes = group.OrderByDescending(t => t.PathLength).Select(t => t.Ancestor) })
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

        public static String GetFullTagNavigationPath_Json(int parentId, bool childrenOnly, int languageId)
        {
            if (languageId == SiteSettings.DefaultLanguageId)
            {
                languageId = -1;
            }
            using (var context= new StoryModel())
            {

                // TODO: Ordering?

                context.Configuration.LazyLoadingEnabled = false;
                var q =
                        context.TagTree.Where(tt =>
                            tt.PathLength <= 1
                            && context.TagTree
                            .Where(
                                tt2 => tt2.DescendantId == parentId
                                && (!childrenOnly || tt2.PathLength == 0)
                            )
                            .Select(tt2 => tt2.AncestorId)
                            .Contains(tt.AncestorId)
                        )
                        .GroupBy(tt => tt.Ancestor)
                        .Select(grouped => new
                        {
                            parent = new
                            {
                                id = grouped.Key.TagId,
                                eng = grouped.Key.EnglishText,
                                type = grouped.Key.TagType,
                                text = languageId != -1 ? grouped.Key.TranslatedTags.Where(tt => tt.LanguageId == languageId).Select(t => t.Text).FirstOrDefault() : ""
                            },
                            children = grouped.Where(g=>g.DescendantId != parentId).OrderBy(g => g.Descendant.EnglishText).Select(g => new
                                {
                                    id = g.Descendant.TagId,
                                    eng = g.Descendant.EnglishText,
                                    type = g.Descendant.TagType,
                                    text = languageId != -1 ? g.Descendant.TranslatedTags.Where(tt => tt.LanguageId == languageId).Select(t => t.Text).FirstOrDefault() : ""
                                })
                        });


                var result = new { defLangId=SiteSettings.DefaultLanguageId, reqLangId=languageId!=-1?languageId:SiteSettings.DefaultLanguageId, results=q };

                return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(result);

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

        public static IList<String> GetPathById(int id)
        {
            using (var context = new StoryModel())
            {
                var q = from t in context.TagTree
                        where t.DescendantId == id
                        orderby t.PathLength descending
                        select t.Ancestor.EnglishText;

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

        public static Tag GetParent(int tagId)
        {
            using (var context = new StoryModel())
            {
                return context.TagTree.Where(tt => tt.DescendantId == tagId && tt.PathLength == 1).Select(tt=>tt.Ancestor).FirstOrDefault();
            }
        }

        public static bool AddTag(Tag tag, int parent)
        {
            return AddTags(new Tuple<Tag, int>[] { Tuple.Create<Tag, int>(tag, parent) });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tags">tag, parentId, 0 for top level</param>
        /// <returns>true if at least one tag and one relationship were added (not rigorous check)</returns>
        public static bool AddTags(IEnumerable<Tuple<Tag, int>> tags)
        {
            using (var context = new StoryModel())
            {

                // Add the tags themselves first
                context.Tags.AddRange(tags.Select(t => t.Item1).AsEnumerable());
                var result = context.SaveChanges() > 0;

                // TODO: check for duplicate tags by name

                // Add structure after tags have been saved
                foreach (var tag in tags)
                {
                    if (tag.Item2 < 0)
                    {
                        continue;
                    }

                    var q = (from t in context.TagTree
                             where tag.Item2 >= TagTree.InvisibleRootId && t.DescendantId == tag.Item2
                             select new
                             {
                                 anc = t.AncestorId,
                                 des = tag.Item1.TagId,
                                 pathlen = (byte)(t.PathLength + 1)
                             }).ToList();

                    var list = q.Select(tt =>
                        new TagTree
                        {
                            AncestorId = tt.anc,
                            DescendantId = tt.des,
                            PathLength = tt.pathlen
                        }).AsEnumerable();

                    if(tag.Item1.TagId != TagTree.InvisibleRootId)
                    {
                        context.TagTree.Add(new TagTree { PathLength = 0, AncestorId = tag.Item1.TagId, DescendantId = tag.Item1.TagId });
                    }
                    context.TagTree.AddRange(list);
                }
                result &= (context.SaveChanges() > 0);

                return result;
            }
        }

        public static TagEditViewModel GetTagEditViewModel(int tagId)
        {
            if (tagId == TagTree.InvisibleRootId)
            {
                return new TagEditViewModel();
            }

            using (var context = new StoryModel())
            {
                context.Configuration.LazyLoadingEnabled = false;
                return new TagEditViewModel()
                {
                    
                    Tag = context.Tags.Where(t => t.TagId == tagId).FirstOrDefault(),
                    //Translations =   context.Languages
                    //                        .Join(
                    //                            context.TranslatedTags, 
                    //                            lang=>lang.LanguageId, 
                    //                            tag=>tag.LanguageId, 
                    //                            (lang, tag) => new TagTranslationDetails { 
                    //                                LanguageId = lang.LanguageId, 
                    //                                LanguageDescription = lang.Description, 
                    //                                LanguageCode = lang.Code, 
                    //                                TagText = tag.Text 
                    //                            }
                    //                        ).ToList()
                    Translations = context.Languages.GroupJoin(
                        context.TranslatedTags,
                        lang => lang.LanguageId,
                        tag => tag.LanguageId,
                        (lang, tag) => new { lang, tag }
                    ).SelectMany(
                        x => x.tag.DefaultIfEmpty(),
                        (x, tag) => new TagTranslationDetails
                        {
                            LanguageId = x.lang.LanguageId,
                            LanguageDescription = x.lang.Description,
                            LanguageCode = x.lang.Code,
                            TagText = tag.Text
                        }
                    ).ToList()
                };
            }
        }

        private class TagNameComparer : IEqualityComparer<Tag>
        {
            bool IEqualityComparer<Tag>.Equals(Tag x, Tag y)
            {
                return  x != null 
                        && y != null 
                        && x.EnglishText.Equals(y.EnglishText, StringComparison.CurrentCultureIgnoreCase);
            }

            int IEqualityComparer<Tag>.GetHashCode(Tag obj)
            {
                return obj.EnglishText.GetHashCode();
            }
        }

        public static bool AddTags(IEnumerable<Tag> children, Tag parent)
        {
            using (var context = new StoryModel())
            {
                // Double check the parent exists and is capable of containing children
                var actualParent = context.Tags.Where(t => t.TagId == parent.TagId).FirstOrDefault();
                if (
                    actualParent == null 
                    || actualParent.TagType == (byte)TagType.PendingSelectableTag 
                    || actualParent.TagType == (byte)TagType.SelectableTag)
                {
                    return false;
                }

                // TODO: Filter out duplicate names already stored in database

                // Add the tags themselves first
                context.Tags.AddRange(children.Distinct(new TagNameComparer()));
                var result = context.SaveChanges() > 0;

                // Add structure after tags have been saved
                foreach (var tag in children)
                {
                    if (actualParent.TagId < 0)
                    {
                        break;
                    }

                    var q = (from t in context.TagTree
                             where actualParent.TagId >= TagTree.InvisibleRootId && t.DescendantId == actualParent.TagId
                             select new
                             {
                                 anc = t.AncestorId,
                                 pathlen = (byte)(t.PathLength + 1)
                             }).ToList();

                    var list = q.Select(tt =>
                        new TagTree
                        {
                            AncestorId = tt.anc,
                            DescendantId = tag.TagId,
                            PathLength = tt.pathlen
                        }).AsEnumerable();

                    if (tag.TagId != TagTree.InvisibleRootId)
                    {
                        context.TagTree.Add(new TagTree { PathLength = 0, AncestorId = tag.TagId, DescendantId = tag.TagId });
                    }
                    context.TagTree.AddRange(list);
                }
                result &= (context.SaveChanges() > 0);

                return result;
            }
        }

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

        public static void AddTagsSql(IEnumerable<Tuple<Tag, int>> tags)
        {
            using (var context = new StoryModel())
            {
                // Add the tags themselves first
                var list = tags.ToList();
                var list2 = tags.Where(t => t.Item1.TagId == 1).ToList();
                context.Tags.AddRange(list.Select(t => t.Item1));
                context.SaveChanges();

                // TODO check for duplicates by name

                foreach (var tag in list)
                {
                    string query = @"
                        INSERT INTO TagTree (AncestorId, DescendantId, PathLength) VALUES ({0}, {0}, 0);";

                    if (tag.Item2 != -1)
                    {
                        query += @"
                        INSERT INTO TagTree
                        SELECT AncestorId, DescendantId = {0}, PathLength = PathLength + 1
                        FROM TagTree
                        WHERE DescendantId = {1}";
                    }

                    context.Database.ExecuteSqlCommand(query, tag.Item1.TagId, tag.Item2);

                }
                context.SaveChanges();
            }
        }

        public class TagPathInfo
        {
            public int TagId { get; set; }
            public IEnumerable<Tag> Nodes { get; set; }

            public string Path { get; set; }
        }

        public static List<TagPathInfo> GetPaths(int rootId, string searchTerm, bool returnAllDescendants)
        {
            const string delimiter = " > ";
            using (var context = new StoryModel())
            {
                return context.TagTree
                    .Where(tt =>
                        (searchTerm == null || tt.Descendant.EnglishText.Contains(searchTerm))
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
                        Path = String.Join(delimiter, t.Nodes.Select(tt => tt.EnglishText))
                    })
                    .OrderBy(t => t.Path)
                    .ToList();
            }
        }

        public static string GetPaths_Json(int rootId, string searchTerm, bool returnAllDescendants, int languageId)
        {
            int resultLimit = 25;
            using (var context = new StoryModel())
            {
                bool defaultLanguage = languageId == SiteSettings.DefaultLanguageId;

                // TODO: improve query

                var q = context.TagTree
                    .Where(tt =>
                        (searchTerm == null || tt.Descendant.EnglishText.Contains(searchTerm))
                        && (rootId == TagTree.InvisibleRootId || context.TagTree.Any(t => t.AncestorId == rootId && t.DescendantId == tt.DescendantId))
                        && (returnAllDescendants || tt.PathLength == 1)
                        && tt.DescendantId != TagTree.InvisibleRootId
                     )
                    .GroupBy(group => group.DescendantId)
                    //.Select(group => new { TagId = group.Key, Nodes = group.OrderByDescending(t => t.PathLength).Select(t => t.Ancestor) })
                    //.ToList()
                    .Select(group => new
                    {
                        value = group.Key,
                        //Nodes = t.Nodes.Select(n => new { n.TagId, n.TagType }),
                        label = group.OrderByDescending(t => t.PathLength).Select(
                            tt => defaultLanguage
                                ? tt.Ancestor.EnglishText
                                : tt.Ancestor.TranslatedTags
                                    .Where(trans => trans.LanguageId == languageId)
                                    .Select(trans => trans.Text)
                                    .FirstOrDefault()).ToList()
                    })
                    // NOTE: limiting results before sorting on client side will cause unpredictable results
                    .Take(resultLimit)
                    //.OrderBy(t => t.label)
                    .ToList();

                return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(q);    
            }
        }

        public static List<Tag> GetTagsByType(
            bool showCategories = false,
            bool showTopLevelTags = false,
            bool showTags = false,
            bool ShowPending = false)
        {
            // TODO Will pending top level tags and categories be possible?
            using (var context = new StoryModel())
            {
                return context.Tags.Where(t =>
                        (ShowPending && t.TagType == (byte)TagType.PendingSelectableTag)
                    || (showCategories && t.TagType == (byte)TagType.Category)
                    || (showTopLevelTags && t.TagType == (byte)TagType.NavigationalTag)
                    || (showTags && t.TagType == (byte)TagType.SelectableTag)
                ).OrderBy(t => t.EnglishText).ToList();
            }
        }

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
        //                orderby t.PathLength, t.Descendant.TagType, t.Descendant.EnglishText
        //                select t.Descendant).ToList();
        //    }
        //}

        //public int AddTag(TagModel context, string english, string spanish = null, int parentId = 0, TagType type = TagType.tag)
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
        //                        where parentId > 0 && t.DescendantId == parentId
        //                        select new { 
        //                            anc = t.AncestorId, 
        //                            des = tag.TagId, 
        //                            pathlen = (byte)(t.PathLength + 1) })
        //                    .AsEnumerable().Select(x => new TagTree { AncestorId = x.anc, DescendantId = x.des, PathLength = x.pathlen });

        //            context.TagTree.Add(new TagTree() { Ancestor = tag, Descendant = tag, PathLength = 0 });
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