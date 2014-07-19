using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ProjectLightSwitch.Models
{
    [MetadataType(typeof(Product.Metadata))]
    public partial class Product
    {
        private sealed class Metadata
        {
            [Required(ErrorMessage = "Product Name is required")]
            public string Name { get; set; }
        }
    }
}