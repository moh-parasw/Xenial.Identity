namespace Xenial.Identity.Client;

public sealed record ResetXenialUserPasswordRequest(string UserId);
public sealed record ResetXenialUserPasswordResponse(string UserId, string Token);
