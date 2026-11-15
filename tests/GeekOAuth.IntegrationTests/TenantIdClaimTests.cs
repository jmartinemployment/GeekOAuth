using GeekOAuth.Abstractions.Models;
using GeekOAuth.Controllers;

namespace GeekOAuth.IntegrationTests;

public sealed class TenantIdClaimTests
{
    private const string Tenant = "5b1f6f0e-7d0a-4c1e-9d55-3a8f2c9e1b44";

    [Fact]
    public void A_guid_tenant_becomes_the_tenant_id_claim()
    {
        Assert.Equal(Tenant, AuthorizationController.TenantIdOf(new ApplicationUser { Tenant = Tenant }));
    }

    [Fact]
    public void The_claim_is_written_in_canonical_lowercase_form()
    {
        var shouted = new ApplicationUser { Tenant = Tenant.ToUpperInvariant() };
        Assert.Equal(Tenant, AuthorizationController.TenantIdOf(shouted));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("acme")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void A_missing_or_non_guid_tenant_is_never_a_tenant_id(string? tenant)
    {
        Assert.Null(AuthorizationController.TenantIdOf(new ApplicationUser { Tenant = tenant }));
    }
}
