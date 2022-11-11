using System.Collections.Immutable;

namespace Xenial.Identity.Client;

public sealed record XenialUser(
    string Id,
    string UserName
)
{
    public ImmutableArray<XenialClaim> Claims { get; init; } = ImmutableArray.Create<XenialClaim>();
};

public sealed record XenialClaim(string Type, string Value);
