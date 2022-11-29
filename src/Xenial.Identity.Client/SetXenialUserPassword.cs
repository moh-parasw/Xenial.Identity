namespace Xenial.Identity.Client;

public sealed record SetXenialUserPasswordRequest(string UserId, string Token, string Password);
