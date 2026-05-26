namespace GeekOAuth.IntegrationTests;

public sealed class SolutionBuildTests
{
    [Fact]
    public void Solution_ProjectsExist()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        Assert.True(File.Exists(Path.Combine(root, "GeekOAuth.slnx")));
        Assert.True(Directory.Exists(Path.Combine(root, "src", "GeekOAuth.Server")));
        Assert.True(Directory.Exists(Path.Combine(root, "src", "GeekOAuth.Abstractions")));
        Assert.True(Directory.Exists(Path.Combine(root, "src", "GeekOAuth.Application")));
        Assert.True(Directory.Exists(Path.Combine(root, "src", "GeekOAuth.Infrastructure")));
    }
}
