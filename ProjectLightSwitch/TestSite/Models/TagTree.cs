namespace TestSite.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("testing.TagTree")]
    public partial class TagTree
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int AncestorId { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DescendantId { get; set; }

        public byte PathLength { get; set; }

        public virtual Tag Ancestor { get; set; }

        public virtual Tag Descendant { get; set; }
    }
}
