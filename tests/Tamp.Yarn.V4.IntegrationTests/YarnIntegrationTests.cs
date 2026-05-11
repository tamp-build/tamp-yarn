using System.IO;
using Tamp;
using Xunit;
using Xunit.Abstractions;

namespace Tamp.Yarn.V4.IntegrationTests;

/// <summary>
/// Exercises the wrapper against a real Yarn 4 install. Stages a tiny
/// throwaway project per test (or shared via the class fixture) and
/// runs the verb being tested.
/// </summary>
public sealed class YarnIntegrationTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly AbsolutePath _workdir;

    public YarnIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
        _workdir = AbsolutePath.Create(Path.Combine(Path.GetTempPath(), $"tamp-yarn-it-{Guid.NewGuid():N}"));
        Directory.CreateDirectory(_workdir.Value);

        // Minimum package.json that Yarn Berry will accept.
        var pkg = @"{ ""name"": ""tamp-yarn-smoke"", ""packageManager"": ""yarn@4.5.3"", ""private"": true, ""scripts"": { ""hello"": ""node -e \""console.log('hi from yarn run')\"""" } }";
        File.WriteAllText(Path.Combine(_workdir.Value, "package.json"), pkg);

        // Bootstrap yarn 4 (.yarnrc.yml with nodeLinker is what HoldFast uses).
        File.WriteAllText(Path.Combine(_workdir.Value, ".yarnrc.yml"), "nodeLinker: node-modules\n");
    }

    private static Tool ResolveTool()
    {
        // We want Yarn Berry 4.x (managed via corepack), NOT a Brew /
        // global Yarn 1 install — Berry's CLI surface is what we wrap.
        // Walk PATH and pick the first `yarn` whose --version reports 4.x.
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
        foreach (var dir in pathEnv.Split(Path.PathSeparator))
        {
            if (string.IsNullOrEmpty(dir)) continue;
            var candidate = Path.Combine(dir, "yarn");
            if (!File.Exists(candidate)) continue;
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo(candidate, "--version")
                { RedirectStandardOutput = true, UseShellExecute = false };
                using var p = System.Diagnostics.Process.Start(psi);
                if (p is null) continue;
                var version = p.StandardOutput.ReadToEnd().Trim();
                p.WaitForExit();
                if (version.StartsWith("4.", StringComparison.Ordinal))
                    return new Tool(AbsolutePath.Create(candidate));
            }
            catch { /* skip and keep looking */ }
        }
        throw new InvalidOperationException(
            "Yarn Berry 4.x not found on PATH. Install with: corepack enable && corepack prepare yarn@4 --activate");
    }

    private CaptureResult Run(CommandPlan plan)
    {
        _output.WriteLine($"$ {plan.Executable} {string.Join(' ', plan.Arguments)}");
        var result = ProcessRunner.Capture(plan);
        foreach (var line in result.Lines)
            _output.WriteLine($"  [{line.Type}] {line.Text}");
        _output.WriteLine($"  → exit {result.ExitCode}");
        return result;
    }

    [Fact]
    public void Install_Succeeds_On_Minimal_Project()
    {
        var tool = ResolveTool();
        var plan = Yarn.Install(tool, s => s.SetWorkingDirectory(_workdir.Value));
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
        // Yarn writes a lockfile on first install.
        Assert.True(File.Exists(Path.Combine(_workdir.Value, "yarn.lock")),
            "Expected yarn.lock after install.");
    }

    [Fact]
    public void Install_Immutable_Without_Lockfile_Fails_Fast()
    {
        // CI gate: without a lockfile, --immutable should fail rather
        // than create one silently.
        var tool = ResolveTool();
        var plan = Yarn.Install(tool, s => s
            .SetWorkingDirectory(_workdir.Value)
            .SetImmutable());
        var result = Run(plan);
        Assert.NotEqual(0, result.ExitCode);
    }

    [Fact]
    public void Run_Script_Executes_Node_And_Prints_Hello()
    {
        var tool = ResolveTool();
        // Need install first so the script can resolve its environment.
        Run(Yarn.Install(tool, s => s.SetWorkingDirectory(_workdir.Value))).ThrowOnFailure();

        var plan = Yarn.Run(tool, s => s
            .SetScript("hello")
            .SetWorkingDirectory(_workdir.Value));
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
        Assert.Contains(result.StdoutLines, l => l.Contains("hi from yarn run"));
    }

    [Fact]
    public void WorkspacesList_On_Single_Package_Returns_The_Root()
    {
        var tool = ResolveTool();
        Run(Yarn.Install(tool, s => s.SetWorkingDirectory(_workdir.Value))).ThrowOnFailure();

        var plan = YarnWorkspaces.List(tool, s => s
            .SetWorkingDirectory(_workdir.Value)
            .SetJson());
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
        // With one workspace (the root), --json emits one line.
        Assert.Contains(result.StdoutLines, l => l.Contains("tamp-yarn-smoke"));
    }

    [Fact]
    public void Raw_Config_Get_Returns_Configured_Value()
    {
        // Escape hatch: invoke `yarn config get` directly.
        var tool = ResolveTool();
        Run(Yarn.Install(tool, s => s.SetWorkingDirectory(_workdir.Value))).ThrowOnFailure();

        var plan = Yarn.Raw(tool, "config", "get", "nodeLinker");
        plan = plan with { WorkingDirectory = _workdir.Value };
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
        Assert.Contains(result.StdoutLines, l => l.Contains("node-modules"));
    }

    public void Dispose()
    {
        try { Directory.Delete(_workdir.Value, recursive: true); } catch { }
    }
}
