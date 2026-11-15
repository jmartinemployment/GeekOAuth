using System.Security.Claims;
using GeekOAuth.Abstractions.Services;
using GeekOAuth.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;

namespace GeekOAuth.IntegrationTests;

public sealed class TenantOwnersControllerTests
{
    private static readonly Guid Tenant = Guid.Parse("5b1f6f0e-7d0a-4c1e-9d55-3a8f2c9e1b44");

    private static ClaimsPrincipal Service(string? svcRole, params string[] scopes)
    {
        var identity = new ClaimsIdentity("test");
        if (svcRole is not null) identity.AddClaim(new Claim("svc_role", svcRole));
        if (scopes.Length > 0) identity.SetScopes(scopes);
        return new ClaimsPrincipal(identity);
    }

    private sealed class FakeAccounts(AccountOperationResult result) : IUserAccountService
    {
        public int Calls { get; private set; }
        public (Guid Tenant, string Email, string? DisplayName)? Last { get; private set; }

        public Task<AccountOperationResult> ProvisionTenantOwnerAsync(Guid tenantId, string email, string password, string? displayName, CancellationToken cancellationToken = default)
        {
            Calls++;
            Last = (tenantId, email, displayName);
            return Task.FromResult(result);
        }

        public Task<AccountOperationResult> RegisterAsync(string email, string password, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<AccountOperationResult> ConfirmEmailAsync(string userId, string code, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<AccountOperationResult> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<AccountOperationResult> ResetPasswordAsync(string email, string code, string newPassword, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private static TenantOwnersController ControllerFor(ClaimsPrincipal user, FakeAccounts accounts) =>
        new(accounts) { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } } };

    private static readonly ProvisionTenantOwnerRequest Request = new(Tenant, "owner@example.com", "a-long-password", "  Dana  ");

    [Fact]
    public void Only_the_geekapi_client_with_internal_api_is_accepted()
    {
        Assert.True(TenantOwnersController.IsGeekApi(Service("geekapi", "internal.api")));
        Assert.False(TenantOwnersController.IsGeekApi(Service("geekapi")));
        Assert.False(TenantOwnersController.IsGeekApi(Service("geekatyourspot-website", "internal.api")));
        Assert.False(TenantOwnersController.IsGeekApi(Service("geekgtm", "internal.api")));
        Assert.False(TenantOwnersController.IsGeekApi(Service(null, "internal.api")));
    }

    [Fact]
    public async Task A_caller_that_is_not_geekapi_is_refused_before_anything_is_created()
    {
        var accounts = new FakeAccounts(new AccountOperationResult(true, UserId: "u1"));
        var result = await ControllerFor(Service("geekgtm", "internal.api"), accounts).Provision(Request, default);

        Assert.Equal(StatusCodes.Status403Forbidden, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Equal(0, accounts.Calls);
    }

    [Fact]
    public async Task Geekapi_creates_the_owner_and_gets_201_with_the_user_and_tenant()
    {
        var accounts = new FakeAccounts(new AccountOperationResult(true, UserId: "u1"));
        var result = await ControllerFor(Service("geekapi", "internal.api"), accounts).Provision(Request, default);

        Assert.Equal(StatusCodes.Status201Created, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Equal((Tenant, "owner@example.com", "  Dana  "), accounts.Last);
    }

    [Fact]
    public async Task A_tenant_or_email_that_already_exists_is_409()
    {
        var accounts = new FakeAccounts(new AccountOperationResult(false, "That tenant already has an owner.", Conflict: true));
        var result = await ControllerFor(Service("geekapi", "internal.api"), accounts).Provision(Request, default);

        Assert.Equal(StatusCodes.Status409Conflict, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
    }

    [Fact]
    public async Task A_refused_password_is_400_with_the_reason()
    {
        var accounts = new FakeAccounts(new AccountOperationResult(false, "Passwords must have a digit."));
        var result = await ControllerFor(Service("geekapi", "internal.api"), accounts).Provision(Request, default);

        Assert.Equal(StatusCodes.Status400BadRequest, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
    }
}
