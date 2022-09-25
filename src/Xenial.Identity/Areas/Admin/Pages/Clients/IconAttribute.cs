namespace Xenial.Identity.Areas.Admin.Pages.Clients
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class IconAttribute : Attribute
    {
        public string Icon { get; }
        public IconAttribute(string icon)
            => Icon = icon;
    }
}
