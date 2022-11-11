namespace Xenial.Identity.Client;

public sealed record CreateXenialUserRequest(
    string Email,
    string? Password = null
);

