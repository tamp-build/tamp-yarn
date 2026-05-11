using Xunit;

namespace Tamp.Yarn.V4.Tests;

public sealed class SmokeTests
{
    [Fact]
    public void Sub_Facades_Are_All_Reachable()
    {
        Assert.NotNull(typeof(Yarn));
        Assert.NotNull(typeof(YarnWorkspaces));
        Assert.NotNull(typeof(YarnNpm));
    }
}
