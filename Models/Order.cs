using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;



namespace Jayakumar_Monish_HW4.Models
{
    public class Order
    {
        // : add navigational properties

        public const Decimal TAX_RATE = 0.0825m;

        [Required]
        [Key]
        public Int32 OrderID { get; set; }

        public Int32 OrderNumber { get; set; }

        public DateTime OrderDate { get; set; }

        public String OrderNotes { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public Decimal SubTotal
        {
            get { return OrderDetails.Sum(rd => rd.ExtendedPrice); }
        }


        [Display(Name = "Sales Tax (8.25%)")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public Decimal SalesTax
        {
            get { return SubTotal * TAX_RATE; }
        }

        [Display(Name = "Registration Total")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public Decimal OrderTotal
        {
            get { return SubTotal + SalesTax; }
        }




        // navigational properties
        public List<OrderDetail> OrderDetails { get; set; }

        public AppUser User { get; set; }

        public Order()
        {
            if (OrderDetails == null)
            {
                OrderDetails = new List<OrderDetail>();
            }
        }

    }
}
