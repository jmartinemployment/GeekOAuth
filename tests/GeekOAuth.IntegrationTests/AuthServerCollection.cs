namespace GeekOAuth.IntegrationTests;

/// <summary>Single shared PostgreSQL container and web host for all integration tests.</summary>
[CollectionDefinition(Name)]
public sealed class AuthServerCollection : ICollectionFixture<GeekOAuthWebApplicationFactory>
{
    public const string Name = "AuthServer";
}
