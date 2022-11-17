namespace Xenial.Identity.Client;

public sealed record AddXenialClaimRequest(string UserId, XenialClaim Claim);
