using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Xenial.Identity.Areas.Identity.Pages.Account.Manage
{
    public class EditProfileModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();
        public class InputModel
        {
            [Display(Name = "Full Name"), Required]
            public string FullName { get; set; }
            [Display(Name = "First Name"), Required]
            public string FirstName { get; set; }
            [Display(Name = "Last Name"), Required]
            public string LastName { get; set; }
            public string Color { get; set; }
            public string Initials { get; set; }


            [Display(Name = "Company Name")]
            public string CompanyName { get; set; }

            [Display(Name = "Address 1"), Required]
            public string AddressStreetAddress1 { get; set; }

            [Display(Name = "Address 2")]
            public string AddressStreetAddress2 { get; set; }

            [Display(Name = "City"), Required]
            public string AddressLocality { get; set; }

            [Display(Name = "State or Province")]
            public string AddressRegion { get; set; }

            [Display(Name = "Zipcode or Postal Code")]
            public string AddressPostalCode { get; set; }
            [Display(Name = "County")]
            public string AddressCountry { get; set; }
        }

        public void OnGet()
        {
        }
    }
}
