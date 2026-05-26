using Microsoft.AspNetCore.Identity;

namespace GeekOAuth.Infrastructure.Repositories;

public class NoOpRoleStore : IRoleStore<IdentityRole>
{
    public Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken ct) => Task.FromResult(IdentityResult.Success);
    public Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken ct) => Task.FromResult(IdentityResult.Success);
    public Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken ct) => Task.FromResult(IdentityResult.Success);
    public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken ct) => Task.FromResult(role.Id);
    public Task<string?> GetRoleNameAsync(IdentityRole role, CancellationToken ct) => Task.FromResult(role.Name);
    public Task SetRoleNameAsync(IdentityRole role, string? roleName, CancellationToken ct) { role.Name = roleName; return Task.CompletedTask; }
    public Task<string?> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken ct) => Task.FromResult(role.NormalizedName);
    public Task SetNormalizedRoleNameAsync(IdentityRole role, string? normalizedName, CancellationToken ct) { role.NormalizedName = normalizedName; return Task.CompletedTask; }
    public Task<IdentityRole?> FindByIdAsync(string roleId, CancellationToken ct) => Task.FromResult<IdentityRole?>(null);
    public Task<IdentityRole?> FindByNameAsync(string normalizedRoleName, CancellationToken ct) => Task.FromResult<IdentityRole?>(null);
    public void Dispose() { }
}
