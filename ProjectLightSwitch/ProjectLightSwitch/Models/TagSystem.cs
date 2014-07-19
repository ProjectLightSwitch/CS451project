using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProjectLightSwitch.Models.Enums;

namespace ProjectLightSwitch.Models
{
    public class TagSystem
    {
        public void SeedData()
        {
            using (var model = new StoryModel())
            {
                model.Database.ExecuteSqlCommand("DELETE FROM Tags");
                model.Database.ExecuteSqlCommand("DELETE FROM TagTree");
                model.Database.ExecuteSqlCommand("DBCC CHECKIDENT (Tags, reseed, 0)");

                //context.Database.ExecuteSqlCommand("DELETE * FROM Tags");

                AddTagsSql(GetInitialTags());
            }
        }

        private IEnumerable<Tuple<Tag, int>> GetInitialTags()
        {
            const int numCategories = 5;
            const int numTopLevelTags = 15;
            const int numTags = 150;
            const int degree = 5;

            //var categories = Enumerable.Range(1, numCategories).Select(i=>AddTag(model, "cat_" + i, type: TagType.Category, spanish: "cat(es)_" + i));
            //var topLevelTags = Enumerable.Range(1, numTopLevelTags).Select(i=>AddTag(model, "tlt_" + i, type: TagType.TopLevelTag, spanish:"tlt(es)_"+i, parentId: (i % numCategories) + 1));
            //var tags =  Enumerable.Range(1, numTags).Select(i => AddTag(model, "tag_" + i, spanish: "tag(es)_"+i, type: TagType.tag, parentId: ((numCategories + numTopLevelTags + i) % (numTopLevelTags + i)) + 1 + numCategories));


            

            int idx = 0;
            for (int i = 0; i < numCategories; i++)
            {
                yield return Tuple.Create<Tag, int>(new Tag { TagType = (byte)TagType.Category, EnglishText = "cat_" + i }, ++idx / degree);
            }

            for (int i = 0; i < numTopLevelTags; i++)
            {
                yield return Tuple.Create<Tag, int>(new Tag { TagType = (byte)TagType.TopLevelTag, EnglishText = "tlt_" + i }, ++idx / degree);
            }

            for (int i = 0; i < numTags; i++)
            {
                yield return Tuple.Create<Tag, int>(new Tag { TagType = (byte)TagType.Tag, EnglishText = "tag_" + i }, ++idx / degree);
            }
        }

        public IList<String> GetPathById(int id)
        {
            using (var model = new StoryModel())
            {
                var q = from t in model.TagTrees
                        where t.DescendantId == id
                        orderby t.PathLength descending
                        select t.Ancestor.EnglishText;

                return q.ToList();
            }
        }

        public bool RemoveTag(int id)
        {
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

        public void AddTag(Tag tag, int parent)
        {
            AddTags(new Tuple<Tag, int>[] { Tuple.Create<Tag, int>(tag, parent) });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tags">tag, parentId, 0 for top level</param>
        public void AddTags(IEnumerable<Tuple<Tag, int>> tags)
        {
            using (var context = new StoryModel())
            {

                // Add the tags themselves first
                context.Tags.AddRange(tags.Select(t => t.Item1).AsEnumerable());
                context.SaveChanges();

                // TODO check for duplicates by name

                // Add structure after tags have been saved
                foreach (var tag in tags)
                {
                    var q = (from t in context.TagTrees
                             where tag.Item2 > 0 && t.DescendantId == tag.Item2
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
                    context.TagTrees.Add(new TagTree { PathLength = 0, AncestorId = tag.Item1.TagId, DescendantId = tag.Item1.TagId });
                    context.TagTrees.AddRange(list);
                }
                context.SaveChanges();
            }
        }

        public void AddTagsSql(IEnumerable<Tuple<Tag, int>> tags)
        {
            using (var context = new StoryModel())
            {
                // Add the tags themselves first
                var list = tags.ToList();
                context.Tags.AddRange(list.Select(t => t.Item1));
                context.SaveChanges();

                // TODO check for duplicates by name

                foreach (var tag in list)
                {
                    string query = @"
                        INSERT INTO TagTree (AncestorId, DescendantId, PathLength) VALUES ({0}, {0}, 0);
                        INSERT INTO TagTree
                        SELECT AncestorId, DescendantId = {0}, PathLength = PathLength + 1
                        FROM TagTree
                        WHERE DescendantId = {1}";


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

        public List<TagPathInfo> GetPaths(int rootId, string searchTerm, bool returnAllDescendants)
        {
            const string delimiter = " > ";
            using (var context = new StoryModel())
            {
                return context.TagTrees
                    .Where(tt =>
                        (searchTerm == null || tt.Descendant.EnglishText.Contains(searchTerm))
                        && (rootId == 0 || context.TagTrees.Any(t => t.AncestorId == rootId && t.DescendantId == tt.DescendantId))
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

        public List<Tag> GetTagsByType(
            bool showCategories = false,
            bool showTopLevelTags = false,
            bool showTags = false,
            bool ShowPending = false)
        {
            // TODO Will pending top level tags and categories be possible?
            using (var context = new StoryModel())
            {
                return context.Tags.Where(t =>
                        (ShowPending && t.TagType == (byte)TagType.PendingTag)
                    || (showCategories && t.TagType == (byte)TagType.Category)
                    || (showTopLevelTags && t.TagType == (byte)TagType.TopLevelTag)
                    || (showTags && t.TagType == (byte)TagType.Tag)
                ).OrderBy(t => t.EnglishText).ToList();
            }
        }

        public Tag GetTag(int id)
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

        //public int AddTag(TagModel model, string english, string spanish = null, int parentId = 0, TagType type = TagType.tag)
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
        //            model.SaveChanges();

        //            // Add tree structure
        //            var q = (from t in model.TagTree
        //                        where parentId > 0 && t.DescendantId == parentId
        //                        select new { 
        //                            anc = t.AncestorId, 
        //                            des = tag.TagId, 
        //                            pathlen = (byte)(t.PathLength + 1) })
        //                    .AsEnumerable().Select(x => new TagTree { AncestorId = x.anc, DescendantId = x.des, PathLength = x.pathlen });

        //            model.TagTree.Add(new TagTree() { Ancestor = tag, Descendant = tag, PathLength = 0 });
        //            model.TagTree.AddRange(q);
        //            model.SaveChanges();
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