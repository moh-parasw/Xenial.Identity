using System.ComponentModel;

namespace Xenial.Identity.Areas.Admin.Pages.Clients
{
    public enum ClientTypes
    {
        [Icon("fas fa-file-code")]
        [Header("WebApplication")]
        [Description("ServerSide (Hybrid Flow with PKCE)")]
        Web = 1,
        [Icon("fas fa-laptop")]
        [Header("SPA")]
        [Description("Javascript (Auth Code Flow with PKCE)")]
        Spa = 2,
        [Icon("fas fa-mobile")]
        [Header("Native Application")]
        [Description("Mobile/Desktop (Hybrid Flow with PKCE)")]
        Native = 3,
        [Icon("fas fa-server")]
        [Header("Machine/Robot")]
        [Description("Client Credentials flow")]
        Machine = 4,
        [Icon("fas fa-tv")]
        [Header("Device flow")]
        [Description("TV and Limited-Input Device Application")]
        Device = 5,
        [Icon("fas fa-key")]
        [Header("Username/Password")]
        [Description("Resource Owner Password flow")]
        ResourceOwnerPassword = 6,
        [Icon("fas fa-file")]
        [Header("Empty")]
        [Description("Manual configuration")]
        Empty = 999,
    }
}
