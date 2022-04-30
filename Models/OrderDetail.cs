using System;
using System.ComponentModel.DataAnnotations;
namespace Jayakumar_Monish_HW4.Models
{
    public class OrderDetail
    {

        [Required]
        [Key]
        public Int32 OrderDetailID { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Number of Products must be greater than 0")]
        public Int32 Quantity { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public Decimal ProductPrice { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public Decimal ExtendedPrice { get; set; }


        // navigational properties
        public Order DetailOrder { get; set; }

        public Product DetailProduct { get; set; }
    }
}
