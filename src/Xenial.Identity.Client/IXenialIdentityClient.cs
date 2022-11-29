namespace Xenial.Identity.Client;

public interface IXenialIdentityClient
{
    Task<XenialResult<XenialUser>> AddToRoleAsync(AddToXenialRoleRequest request, CancellationToken cancellationToken = default);
    Task<XenialResult<XenialUser>> CreateUserAsync(CreateXenialUserRequest request, CancellationToken cancellationToken = default);
    Task<XenialResult<XenialIdResponse>> DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<XenialResult<XenialIdResponse>> GetUserIdAsync(CancellationToken cancellationToken = default);
    Task<XenialResult<IEnumerable<XenialUser>>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<XenialResult<XenialUser>> RemoveFromRoleAsync(RemoveFromXenialRoleRequest request, CancellationToken cancellationToken = default);
    Task<XenialResult<XenialUser>> AddClaimAsync(AddXenialClaimRequest req, CancellationToken cancellationToken = default);
    Task<XenialResult<XenialUser>> RemoveClaimAsync(RemoveXenialClaimRequest req, CancellationToken cancellationToken = default);
    Task<XenialResult<ResetXenialUserPasswordResponse>> ResetPasswordAsync(ResetXenialUserPasswordRequest req, CancellationToken cancellationToken = default);
    Task<XenialResult<XenialUser>> SetPasswordAsync(SetXenialUserPasswordRequest req, CancellationToken cancellationToken = default);

}
