using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAppERP1.Models.Dtos
{
    public class Product
    {
        public long Id { get; set; }
        public string ExternalId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Barcode { get; set; }
        public string RetailPrice { get; set; }
        public string WholesalePrice { get; set; }
        public string Discount { get; set; }
    }

    public class ProductDto
    {
        public bool Success { get; set; }
        public List<Product> Data { get; set; }
    }

}