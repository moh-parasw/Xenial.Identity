using System.Collections.Immutable;

namespace Xenial.Identity.Client;

public sealed record XenialUser(
    string Id,
    string UserName
)
{
    public ImmutableArray<XenialClaim> Claims { get; init; } = ImmutableArray.Create<XenialClaim>();

    public string[] Roles => Claims.Where(m => m.Type == "role").Select(m => m.Value).ToArray();
};

public sealed record XenialClaim(string Type, string Value);
