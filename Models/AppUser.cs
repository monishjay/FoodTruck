using System;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;


namespace Jayakumar_Monish_HW4.Models

{
    public class AppUser : IdentityUser 
{

    [Required]
    public String FirstName { get; set; }

    [Required]
    public String LastName { get; set; }


}
}
