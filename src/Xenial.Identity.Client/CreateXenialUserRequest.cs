namespace Xenial.Identity.Client;

public sealed record CreateXenialUserRequest(
    string Email,
    string Username,
    string? Password = null
);

