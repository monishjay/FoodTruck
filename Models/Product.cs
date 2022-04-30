using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Jayakumar_Monish_HW4.Models
{

    public enum ProductTypes { Hot, Cold, Packaged,
                                 Drink, Other}


    public class Product
    {
        [Required]
        [Key]
        public Int32 ProductID { get; set; }

        [Required]
        [Display(Name = "Product Name:")]
        public String Name { get; set; }

        [Display(Name = "Product Description:")]
        public String Description { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:c}")]
        [Display(Name = "Product Price:")]
        public Decimal Price { get; set; }

        [Display(Name = "Product Type:")]
        public ProductTypes ProductType { get; set; }

        // navigational properties
        public List<Supplier> Suppliers { get; set; }

        public List<OrderDetail> OrderDetails { get; set; }


        public Product()
        {
            if (Suppliers == null)
            {
                Suppliers = new List<Supplier>();
            }

            if (OrderDetails == null)
            {
                OrderDetails = new List<OrderDetail>();
            }
        }



    }
}
