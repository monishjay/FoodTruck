using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace Jayakumar_Monish_HW4.Models
{
    public class Supplier
    {
        [Required]
        [Key]
        public Int32 SupplierID { get; set; }

        [Required]
        [Display(Name = "Supplier Name:")]
        public String SupplierName { get; set; }

        [Required]
        public String Email { get; set; }

        [Required]
        public String PhoneNumber { get; set; }

        // navigational properties
        public List<Product> Products { get; set; }

        public Supplier()
        {
            if (Products == null)
            {
                Products = new List<Product>();
            }
        }
    }
}
