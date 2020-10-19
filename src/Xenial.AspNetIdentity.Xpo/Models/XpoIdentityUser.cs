using System;
using System.Security.Claims;

using DevExpress.Xpo;


using Microsoft.AspNetCore.Identity;

using Xenial.AspNetIdentity.Xpo;

namespace Xenial.AspNetIdentity.Xpo.Models
{
    [XPIdentityUser(RoleType = typeof(MyRole), ClaimsType = typeof(MyUserClaim), LoginsType = typeof(MyUserLogin))]
    [Persistent]
    public partial class MyUser : XpoIdentityBaseObject
    {
        private Guid id;

        public MyUser(Session session) : base(session) { }

        [Persistent]
        [Key(AutoGenerate = true)]
        public Guid Id { get => id; set => SetPropertyValue(ref id, value); }



    }

    [XPIdentityUserClaim(UserType = typeof(MyUser))]
    [Persistent]
    public partial class MyUserClaim : XpoIdentityBaseObject
    {
        private Guid id;

        public MyUserClaim(Session session) : base(session) { }

        [Persistent]
        [Key(AutoGenerate = true)]
        public Guid Id { get => id; set => SetPropertyValue(ref id, value); }



    }

    [XPIdentityUserLogin(UserType = typeof(MyUser))]
    [Persistent]
    public partial class MyUserLogin : XpoIdentityBaseObject
    {
        private Guid id;

        public MyUserLogin(Session session) : base(session) { }

        [Persistent]
        [Key(AutoGenerate = true)]
        public Guid Id { get => id; set => SetPropertyValue(ref id, value); }
    }



    [XPIdentityRole(UserType = typeof(MyUser), ClaimsType = typeof(MyRoleClaim))]
    [Persistent]
    public partial class MyRole : XpoIdentityBaseObject
    {
        private Guid id;


        public MyRole(Session session) : base(session) { }

        [Persistent]
        [Key(AutoGenerate = true)]
        public Guid Id { get => id; set => SetPropertyValue(ref id, value); }
    }
    [XPIdentityRoleClaim(RoleType = typeof(MyRole))]
    [Persistent]
    public partial class MyRoleClaim : XpoIdentityBaseObject
    {
        private Guid id;

        public MyRoleClaim(Session session) : base(session) { }

        [Persistent]
        [Key(AutoGenerate = true)]
        public Guid Id { get => id; set => SetPropertyValue(ref id, value); }



    }
    [NonPersistent]
    public abstract class XpoIdentityUserGuid : XpoIdentityUserBase<Guid>
    {
        public XpoIdentityUserGuid(Session session) : base(session) { }

        [Association]
        public XPCollection<XpoIdentityUserClaimGuid> Claims => GetCollection<XpoIdentityUserClaimGuid>();
    }

    [NonPersistent]
    public abstract class XpoIdentityUserString : XpoIdentityUserBaseNoAuto<string>
    {
        public XpoIdentityUserString(Session session) : base(session) { }
    }

    [NonPersistent]
    public abstract class XpoIdentityUserInt : XpoIdentityUserBase<int>
    {
        public XpoIdentityUserInt(Session session) : base(session) { }
    }

    [NonPersistent]
    public abstract class XpoIdentityUserBase<TKey> : XpoIdentityUserBase
        where TKey : IEquatable<TKey>
    {
        private TKey id;

        public XpoIdentityUserBase(Session session) : base(session) { }

        /// <summary>
        /// Gets or sets the primary key for this user.
        /// </summary>
        [PersonalData]
        [Persistent]
        [Key(AutoGenerate = true)]
        public TKey Id { get => id; set => SetPropertyValue(ref id, value); }
    }

    [NonPersistent]
    public abstract class XpoIdentityUserBaseNoAuto<TKey> : XpoIdentityUserBase
        where TKey : IEquatable<TKey>
    {
        private TKey id;

        public XpoIdentityUserBaseNoAuto(Session session) : base(session) { }

        /// <summary>
        /// Gets or sets the primary key for this user.
        /// </summary>
        [PersonalData]
        [Persistent]
        [Key]
        public TKey Id { get => id; set => SetPropertyValue(ref id, value); }
    }

    [NonPersistent]
    public abstract class XpoIdentityUserBase : XpoIdentityBaseObject
    {
        private string userName;
        private string normalizedUserName;
        private string email;
        private string normalizedEmail;
        private bool emailConfirmed;
        private string passwordHash;
        private string securityStamp;
        //private string concurrencyStamp = Guid.NewGuid().ToString();
        private string phoneNumber;
        private bool phoneNumberConfirmed;
        private bool twoFactorEnabled;
        private DateTime? lockoutEnd;
        private bool lockoutEnabled;
        private int accessFailedCount;

        public XpoIdentityUserBase(Session session) : base(session) { }

        /// <summary>
        /// Gets or sets the user name for this user.
        /// </summary>
        [ProtectedPersonalData]
        [Persistent]
        [Indexed(Unique = true)]
        [Size(250)]
        public string UserName { get => userName; set => SetPropertyValue(ref userName, value); }

        /// <summary>
        /// Gets or sets the normalized user name for this user.
        /// </summary>
        [Persistent]
        [Indexed]
        public string NormalizedUserName { get => normalizedUserName; set => SetPropertyValue(ref normalizedUserName, value); }

        /// <summary>
        /// Gets or sets the email address for this user.
        /// </summary>
        [ProtectedPersonalData]
        [Persistent]
        [Size(250)]
        [Indexed]
        public string Email { get => email; set => SetPropertyValue(ref email, value); }

        /// <summary>
        /// Gets or sets the normalized email address for this user.
        /// </summary>
        [Persistent]
        [Indexed]
        public string NormalizedEmail { get => normalizedEmail; set => SetPropertyValue(ref normalizedEmail, value); }

        /// <summary>
        /// Gets or sets a flag indicating if a user has confirmed their email address.
        /// </summary>
        /// <value>True if the email address has been confirmed, otherwise false.</value>
        [PersonalData]
        [Persistent]
        public bool EmailConfirmed { get => emailConfirmed; set => SetPropertyValue(ref emailConfirmed, value); }

        /// <summary>
        /// Gets or sets a salted and hashed representation of the password for this user.
        /// </summary>
        [Persistent]
        // 50000 chosen to be explicit to allow enough size to avoid truncation, yet stay beneath the MySql row size limit of ~65K
        // apparently anything over 4K converts to nvarchar(max) on SqlServer
        [Size(50000)]
        public string PasswordHash { get => passwordHash; set => SetPropertyValue(ref passwordHash, value); }

        /// <summary>
        /// A random value that must change whenever a users credentials change (password changed, login removed)
        /// </summary>
        [Persistent]
        [Size(500)]
        public string SecurityStamp { get => securityStamp; set => SetPropertyValue(ref securityStamp, value); }

        ///// <summary>
        ///// A random value that must change whenever a user is persisted to the store
        ///// </summary>
        //[Persistent]
        //[Size(100)]
        ////TODO: CHECK FOR CONCURRENCY
        //public string ConcurrencyStamp { get => concurrencyStamp; set => SetPropertyValue(ref concurrencyStamp, value); }

        /// <summary>
        /// Gets or sets a telephone number for the user.
        /// </summary>
        [ProtectedPersonalData]
        [Persistent]
        [Size(50)]
        public string PhoneNumber { get => phoneNumber; set => SetPropertyValue(ref phoneNumber, value); }

        /// <summary>
        /// Gets or sets a flag indicating if a user has confirmed their telephone address.
        /// </summary>
        /// <value>True if the telephone number has been confirmed, otherwise false.</value>
        [PersonalData]
        [Persistent]
        public bool PhoneNumberConfirmed { get => phoneNumberConfirmed; set => SetPropertyValue(ref phoneNumberConfirmed, value); }

        /// <summary>
        /// Gets or sets a flag indicating if two factor authentication is enabled for this user.
        /// </summary>
        /// <value>True if 2fa is enabled, otherwise false.</value>
        [PersonalData]
        [Persistent]
        public bool TwoFactorEnabled { get => twoFactorEnabled; set => SetPropertyValue(ref twoFactorEnabled, value); }

        /// <summary>
        /// Gets or sets the date and time, in UTC, when any user lockout ends.
        /// </summary>
        /// <remarks>
        /// A value in the past means the user is not locked out.
        /// </remarks>
        [Persistent]
        public DateTime? LockoutEnd { get => lockoutEnd; set => SetPropertyValue(ref lockoutEnd, value); }

        /// <summary>
        /// Gets or sets a flag indicating if the user could be locked out.
        /// </summary>
        /// <value>True if the user could be locked out, otherwise false.</value>
        [Persistent]
        public bool LockoutEnabled { get => lockoutEnabled; set => SetPropertyValue(ref lockoutEnabled, value); }

        /// <summary>
        /// Gets or sets the number of failed login attempts for the current user.
        /// </summary>
        [Persistent]
        public int AccessFailedCount { get => accessFailedCount; set => SetPropertyValue(ref accessFailedCount, value); }

        /// <summary>
        /// Returns the username for this user.
        /// </summary>
        public override string ToString()
            => UserName;
    }
}
