namespace GeekOAuth.Abstractions.Security;

/// <summary>
/// The only return address Register, the confirmation email and Login carry for a person who started
/// signing up from an app: the authorize request they came from, as a local path. Anything else (another
/// path, an absolute address, a protocol-relative "//host", a backslash form) is refused, so the value can
/// be put in a page link or an email without turning either into an open redirect.
/// </summary>
public static class AuthorizeReturnUrl
{
    private const string AuthorizePath = "/connect/authorize";
    private const int MaxLength = 2048;

    public static bool IsValid(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Length > MaxLength) return false;
        if (value != AuthorizePath && !value.StartsWith(AuthorizePath + "?", StringComparison.Ordinal)) return false;
        return value.All(c => c >= 0x20 && c != 0x7F && c != '\\');
    }
}
