using Microsoft.AspNetCore.Identity;

namespace Xenial.Identity.Data
{
    public class XenialIdentityUser : IdentityUser
    {
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string DisplayName => string.IsNullOrWhiteSpace($"{FirstName} {LastName}")
            ? UserName
            : $"{FirstName} {LastName}";

        public string Color { get; set; }
        public string Initials { get; set; }

        public string CompanyName { get; set; }
        public string AddressStreetAddress1 { get; set; }
        public string AddressStreetAddress2 { get; set; }
        public string AddressLocality { get; set; }
        public string AddressRegion { get; set; }
        public string AddressPostalCode { get; set; }
        public string AddressCountry { get; set; }

        public byte[] Picture { get; set; }
        public string PictureMimeType { get; set; }
        public string PictureId { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
