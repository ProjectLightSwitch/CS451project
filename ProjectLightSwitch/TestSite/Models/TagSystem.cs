using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestSite.Models
{
    public class TagSystem
    {
        public enum TagType : byte
        {
            Category=1,
            TopLevelTag=2,
            Tag=3,
            PendingTag=4
        }

        public IList<String> GetPathById(int id)
        {
            using (var model = new TagModel())
            {
                var q = from t in model.TagTrees
                        where t.DescendantId == id
                        orderby t.PathLength descending
                        select t.Ancestor.EnglishText;
                
                return q.ToList();
            }
        }

        public int AddTag(string english, string spanish = null, int parentId = 0, TagType type = TagType.Tag)
        {
            int result = 0;
            using(var model = new TagModel())
            {
                // not really needed
                using (var transaction = model.Database.BeginTransaction())
                {
                    try
                    {
                        // Add tag
                        var tag = new Tag();
                        tag.TagType = (byte)type;
                        tag.EnglishText = english;
                        if (spanish != null)
                        {
                            tag.TranslatedTags.Add(new TagsTranslated { LanguageCode = "es-mx", Text = spanish });
                        }
                        model.SaveChanges();

                        // Add tree structure
                        var q = (from t in model.TagTrees
                                 where parentId > 0 && t.DescendantId == parentId
                                 select new { 
                                     anc = t.AncestorId, 
                                     des = tag.TagId, 
                                     pathlen = (byte)(t.PathLength + 1) })
                                .AsEnumerable().Select(x => new TagTree { AncestorId = x.anc, DescendantId = x.des, PathLength = x.pathlen });

                        model.TagTrees.Add(new TagTree() { Ancestor = tag, Descendant = tag, PathLength = 0 });
                        model.TagTrees.AddRange(q);
                        model.SaveChanges();
                        transaction.Commit();

                        result = tag.TagId;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                    }
                }
            }
            return result;
        }
    }
}