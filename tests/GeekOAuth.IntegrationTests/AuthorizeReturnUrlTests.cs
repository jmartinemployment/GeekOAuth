using GeekOAuth.Abstractions.Security;

namespace GeekOAuth.IntegrationTests;

public class AuthorizeReturnUrlTests
{
    [Theory]
    [InlineData("/connect/authorize")]
    [InlineData("/connect/authorize?client_id=geek-social&response_type=code&state=abc&code_challenge=xyz&code_challenge_method=S256")]
    public void Accepts_an_authorize_request_as_a_local_path(string value) =>
        Assert.True(AuthorizeReturnUrl.IsValid(value));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("/")]
    [InlineData("/Account/Login")]
    [InlineData("/connect/authorizeevil")]
    [InlineData("/connect/token")]
    [InlineData("//evil.example/connect/authorize?x=1")]
    [InlineData("https://evil.example/connect/authorize?x=1")]
    [InlineData("/connect/authorize?x=1\\evil")]
    [InlineData("/connect/authorize?x=1\r\nSet-Cookie: a=b")]
    public void Refuses_everything_else(string? value) =>
        Assert.False(AuthorizeReturnUrl.IsValid(value));

    [Fact]
    public void Refuses_a_value_over_the_length_limit() =>
        Assert.False(AuthorizeReturnUrl.IsValid("/connect/authorize?" + new string('a', 2100)));
}
