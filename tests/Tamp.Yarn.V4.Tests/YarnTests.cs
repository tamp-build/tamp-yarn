using Xunit;

namespace Tamp.Yarn.V4.Tests;

public sealed class YarnTests
{
    private static Tool FakeTool() => new(AbsolutePath.Create("/fake/yarn"));

    private static int IndexOf(IReadOnlyList<string> args, string value, int start = 0)
    {
        for (var i = start; i < args.Count; i++) if (args[i] == value) return i;
        return -1;
    }

    // =========================================================
    // null-tool / null-configurer guards (every facade)
    // =========================================================

    [Fact]
    public void Install_Throws_On_Null_Tool() => Assert.Throws<ArgumentNullException>(() => Yarn.Install(null!));
    [Fact]
    public void Run_Throws_On_Null_Tool() => Assert.Throws<ArgumentNullException>(() => Yarn.Run(null!, s => s.SetScript("x")));
    [Fact]
    public void Dlx_Throws_On_Null_Tool() => Assert.Throws<ArgumentNullException>(() => Yarn.Dlx(null!, s => s.SetCommand("x")));
    [Fact]
    public void Pack_Throws_On_Null_Tool() => Assert.Throws<ArgumentNullException>(() => Yarn.Pack(null!));
    [Fact]
    public void Dedupe_Throws_On_Null_Tool() => Assert.Throws<ArgumentNullException>(() => Yarn.Dedupe(null!));
    [Fact]
    public void Exec_Throws_On_Null_Tool() => Assert.Throws<ArgumentNullException>(() => Yarn.Exec(null!, s => s.SetCommand("x")));
    [Fact]
    public void Raw_Throws_On_Null_Tool() => Assert.Throws<ArgumentNullException>(() => Yarn.Raw(null!, "install"));
    [Fact]
    public void Raw_Throws_On_Empty_Args() => Assert.Throws<ArgumentException>(() => Yarn.Raw(FakeTool()));
    [Fact]
    public void Workspaces_List_Throws_On_Null_Tool() => Assert.Throws<ArgumentNullException>(() => YarnWorkspaces.List(null!));
    [Fact]
    public void Workspaces_Foreach_Throws_On_Null_Tool() => Assert.Throws<ArgumentNullException>(() => YarnWorkspaces.Foreach(null!, s => s.SetCommand("build")));
    [Fact]
    public void Workspaces_Focus_Throws_On_Null_Tool() => Assert.Throws<ArgumentNullException>(() => YarnWorkspaces.Focus(null!));
    [Fact]
    public void Npm_Publish_Throws_On_Null_Tool() => Assert.Throws<ArgumentNullException>(() => YarnNpm.Publish(null!));
    [Fact]
    public void Npm_TagAdd_Throws_On_Null_Tool() => Assert.Throws<ArgumentNullException>(() => YarnNpm.TagAdd(null!, s => s.SetPackageVersion("p@1").SetTag("latest")));
    [Fact]
    public void Npm_TagRemove_Throws_On_Null_Tool() => Assert.Throws<ArgumentNullException>(() => YarnNpm.TagRemove(null!, s => s.SetPackage("p").SetTag("x")));

    // =========================================================
    // Common (executable, env passthrough)
    // =========================================================

    [Theory]
    [InlineData("/usr/local/bin/yarn")]
    [InlineData("/Users/scott/.nodenv/shims/yarn")]
    public void Every_Verb_Uses_Tool_Path_As_Executable(string toolPath)
    {
        // AbsolutePath.Create normalizes through Path.GetFullPath, which
        // rewrites POSIX-style paths to drive-rooted forms on Windows.
        // Compare through tool.Executable.Value (lesson from TAM-84).
        var tool = new Tool(AbsolutePath.Create(toolPath));
        var expected = tool.Executable.Value;
        Assert.Equal(expected, Yarn.Install(tool).Executable);
        Assert.Equal(expected, Yarn.Pack(tool).Executable);
        Assert.Equal(expected, YarnWorkspaces.List(tool).Executable);
        Assert.Equal(expected, YarnNpm.Whoami(tool).Executable);
    }

    [Fact]
    public void NpmAuthToken_Lands_In_Environment_And_Secrets()
    {
        var token = new Secret("npm auth", "npm_xxx");
        var plan = Yarn.Install(FakeTool(), s => s.SetNpmAuthToken(token));
        Assert.Equal("npm_xxx", plan.Environment["NPM_CONFIG_TOKEN"]);
        Assert.Same(token, Assert.Single(plan.Secrets));
        // Token MUST NOT appear in args — Yarn reads it from env.
        Assert.DoesNotContain("npm_xxx", plan.Arguments);
    }

    [Fact]
    public void No_NpmAuthToken_Means_Empty_Secrets_And_No_NPM_CONFIG_TOKEN()
    {
        var plan = Yarn.Install(FakeTool());
        Assert.Empty(plan.Secrets);
        Assert.False(plan.Environment.ContainsKey("NPM_CONFIG_TOKEN"));
    }

    // =========================================================
    // install
    // =========================================================

    [Fact]
    public void Install_Bare_Just_Has_The_Verb()
    {
        var args = Yarn.Install(FakeTool()).Arguments;
        Assert.Equal(["install"], args);
    }

    [Fact]
    public void Install_Immutable_Is_The_CI_Flag()
    {
        var args = Yarn.Install(FakeTool(), s => s.SetImmutable()).Arguments;
        Assert.Contains("--immutable", args);
    }

    [Fact]
    public void Install_All_Flags_Round_Trip()
    {
        var args = Yarn.Install(FakeTool(), s => s
            .SetImmutable()
            .SetImmutableCache()
            .SetRefreshLockfile()
            .SetCheckCache()
            .SetCheckResolutions()
            .SetInlineBuilds()
            .SetJson()
            .SetMode("update-lockfile")).Arguments;
        Assert.Contains("--immutable", args);
        Assert.Contains("--immutable-cache", args);
        Assert.Contains("--refresh-lockfile", args);
        Assert.Contains("--check-cache", args);
        Assert.Contains("--check-resolutions", args);
        Assert.Contains("--inline-builds", args);
        Assert.Contains("--json", args);
        Assert.Equal("update-lockfile", args[IndexOf(args, "--mode") + 1]);
    }

    // =========================================================
    // run
    // =========================================================

    [Fact]
    public void Run_Throws_When_Script_Missing()
        => Assert.Throws<InvalidOperationException>(() => Yarn.Run(FakeTool(), _ => { }));

    [Fact]
    public void Run_Emits_Explicit_Run_Verb_Then_Script_Then_Args()
    {
        // The shorthand `yarn build` works, but we always emit `yarn run build`
        // explicitly — disambiguates from CLI verbs that share names with
        // scripts (e.g., `yarn version`).
        var args = Yarn.Run(FakeTool(), s => s
            .SetScript("build:fast")
            .AddScriptArg("--filter=@holdfast-io/frontend")).Arguments;
        Assert.Equal("run", args[0]);
        Assert.Equal("build:fast", args[1]);
        Assert.Equal("--filter=@holdfast-io/frontend", args[2]);
    }

    [Fact]
    public void Run_BinariesOnly_Round_Trips()
    {
        var args = Yarn.Run(FakeTool(), s => s.SetScript("eslint").SetBinariesOnly()).Arguments;
        Assert.Contains("--binaries-only", args);
    }

    // =========================================================
    // dlx
    // =========================================================

    [Fact]
    public void Dlx_Throws_When_Command_Missing()
        => Assert.Throws<InvalidOperationException>(() => Yarn.Dlx(FakeTool(), _ => { }));

    [Fact]
    public void Dlx_Packages_Then_Command_Then_Args()
    {
        var args = Yarn.Dlx(FakeTool(), s => s
            .AddPackage("typescript@5.6.0")
            .AddPackage("@types/node@22")
            .SetCommand("tsc")
            .AddCommandArg("--init")).Arguments;
        // -p <pkg> repeated, then command, then command args
        Assert.Equal("dlx", args[0]);
        Assert.Equal(2, args.Count(a => a == "-p"));
        Assert.Contains("typescript@5.6.0", args);
        Assert.Contains("@types/node@22", args);
        var tsc = IndexOf(args, "tsc");
        Assert.True(tsc > 0);
        Assert.Equal("--init", args[tsc + 1]);
    }

    [Fact]
    public void Dlx_Quiet_Emits_Flag()
    {
        var args = Yarn.Dlx(FakeTool(), s => s.SetCommand("x").SetQuiet()).Arguments;
        Assert.Contains("--quiet", args);
    }

    // =========================================================
    // pack
    // =========================================================

    [Fact]
    public void Pack_Out_DryRun_Filename_Json_Round_Trip()
    {
        var args = Yarn.Pack(FakeTool(), s => s
            .SetOut("dist/pkg")
            .SetDryRun()
            .AddFilename("dist/**/*")
            .SetJson()).Arguments;
        Assert.Equal("pack", args[0]);
        Assert.Equal("dist/pkg", args[IndexOf(args, "--out") + 1]);
        Assert.Contains("--dry-run", args);
        Assert.Equal("dist/**/*", args[IndexOf(args, "--filename") + 1]);
        Assert.Contains("--json", args);
    }

    // =========================================================
    // dedupe
    // =========================================================

    [Fact]
    public void Dedupe_Check_Is_The_CI_Gate_Flag()
    {
        var args = Yarn.Dedupe(FakeTool(), s => s.SetCheck()).Arguments;
        Assert.Contains("--check", args);
    }

    [Fact]
    public void Dedupe_Strategy_And_Patterns_Round_Trip()
    {
        var args = Yarn.Dedupe(FakeTool(), s => s
            .SetStrategy("highest")
            .AddPattern("react*")
            .AddPattern("@types/*")).Arguments;
        Assert.Equal("highest", args[IndexOf(args, "--strategy") + 1]);
        Assert.Contains("react*", args);
        Assert.Contains("@types/*", args);
    }

    // =========================================================
    // exec
    // =========================================================

    [Fact]
    public void Exec_Throws_When_Command_Missing()
        => Assert.Throws<InvalidOperationException>(() => Yarn.Exec(FakeTool(), _ => { }));

    [Fact]
    public void Exec_Command_And_Args_Round_Trip()
    {
        var args = Yarn.Exec(FakeTool(), s => s
            .SetCommand("node")
            .AddCommandArg("-e")
            .AddCommandArg("console.log(1)")).Arguments;
        Assert.Equal("exec", args[0]);
        Assert.Equal("node", args[1]);
        Assert.Equal("-e", args[2]);
        Assert.Equal("console.log(1)", args[3]);
    }

    // =========================================================
    // raw escape hatch
    // =========================================================

    [Fact]
    public void Raw_Passes_Args_Verbatim()
    {
        var args = Yarn.Raw(FakeTool(), "config", "get", "npmRegistryServer").Arguments;
        Assert.Equal(["config", "get", "npmRegistryServer"], args);
    }

    // =========================================================
    // workspaces list
    // =========================================================

    [Fact]
    public void WorkspacesList_Verb_Tokens_Are_Workspaces_List()
    {
        var args = YarnWorkspaces.List(FakeTool()).Arguments;
        Assert.Equal(["workspaces", "list"], args);
    }

    [Fact]
    public void WorkspacesList_All_Flags_Round_Trip()
    {
        var args = YarnWorkspaces.List(FakeTool(), s => s
            .SetSince().SetRecursive().SetNoPrivate().SetVerbose().SetJson()).Arguments;
        Assert.Contains("--since", args);
        Assert.Contains("-R", args);
        Assert.Contains("--no-private", args);
        Assert.Contains("-v", args);
        Assert.Contains("--json", args);
    }

    // =========================================================
    // workspaces foreach
    // =========================================================

    [Fact]
    public void WorkspacesForeach_Throws_When_Command_Missing()
        => Assert.Throws<InvalidOperationException>(() => YarnWorkspaces.Foreach(FakeTool(), _ => { }));

    [Fact]
    public void WorkspacesForeach_HoldFast_Build_Shape()
    {
        // HoldFast's actual frontend build invocation, paraphrased.
        var args = YarnWorkspaces.Foreach(FakeTool(), s => s
            .SetFrom("@holdfast-io/frontend")
            .SetRecursive()
            .SetTopological()
            .SetParallel()
            .SetJobs(4)
            .SetVerbose()
            .SetCommand("run")
            .AddCommandArg("build:fast")).Arguments;
        Assert.Equal("workspaces", args[0]);
        Assert.Equal("foreach", args[1]);
        Assert.Equal("@holdfast-io/frontend", args[IndexOf(args, "--from") + 1]);
        Assert.Contains("-R", args);
        Assert.Contains("-t", args);
        Assert.Contains("-p", args);
        Assert.Equal("4", args[IndexOf(args, "-j") + 1]);
        // Command and its args go last.
        var lastIdx = args.Count - 2;
        Assert.Equal("run", args[lastIdx]);
        Assert.Equal("build:fast", args[lastIdx + 1]);
    }

    [Fact]
    public void WorkspacesForeach_Include_Exclude_Repeat_Their_Flags()
    {
        var args = YarnWorkspaces.Foreach(FakeTool(), s => s
            .AddInclude("@holdfast-io/*")
            .AddInclude("rrweb*")
            .AddExclude("@holdfast-io/legacy-*")
            .SetCommand("build")).Arguments;
        Assert.Equal(2, args.Count(a => a == "--include"));
        Assert.Single(args, a => a == "--exclude");
    }

    [Fact]
    public void WorkspacesForeach_All_Flag_Maps_To_DashA()
    {
        var args = YarnWorkspaces.Foreach(FakeTool(), s => s.SetAll().SetCommand("test")).Arguments;
        Assert.Contains("-A", args);
    }

    [Fact]
    public void WorkspacesForeach_DryRun_Maps_To_DashN()
    {
        var args = YarnWorkspaces.Foreach(FakeTool(), s => s.SetDryRun().SetCommand("build")).Arguments;
        Assert.Contains("-n", args);
    }

    // =========================================================
    // workspaces focus
    // =========================================================

    [Fact]
    public void WorkspacesFocus_Production_All_Workspaces_Round_Trip()
    {
        var args = YarnWorkspaces.Focus(FakeTool(), s => s
            .SetProduction()
            .SetAll()
            .AddWorkspace("@holdfast-io/backend")).Arguments;
        Assert.Equal(["workspaces", "focus"], args.Take(2));
        Assert.Contains("--production", args);
        Assert.Contains("-A", args);
        Assert.Contains("@holdfast-io/backend", args);
    }

    // =========================================================
    // npm publish
    // =========================================================

    [Fact]
    public void NpmPublish_Verb_Tokens_Are_Npm_Publish()
    {
        var args = YarnNpm.Publish(FakeTool()).Arguments;
        Assert.Equal(["npm", "publish"], args);
    }

    [Theory]
    [InlineData(YarnNpmAccess.Public, "public")]
    [InlineData(YarnNpmAccess.Restricted, "restricted")]
    public void NpmPublish_Access_Maps_To_Lowercase_Token(YarnNpmAccess access, string expected)
    {
        var args = YarnNpm.Publish(FakeTool(), s => s.SetAccess(access)).Arguments;
        Assert.Equal(expected, args[IndexOf(args, "--access") + 1]);
    }

    [Fact]
    public void NpmPublish_Tag_TolerateRepublish_Round_Trip()
    {
        var args = YarnNpm.Publish(FakeTool(), s => s
            .SetTag("next")
            .SetTolerateRepublish()).Arguments;
        Assert.Equal("next", args[IndexOf(args, "--tag") + 1]);
        Assert.Contains("--tolerate-republish", args);
    }

    [Fact]
    public void NpmPublish_Otp_Reveals_And_Registers_As_Secret()
    {
        // OTP is a Secret because it's a sensitive short-lived value.
        // Unlike NpmAuthToken (passed via env), OTP IS a CLI arg.
        var otp = new Secret("npm OTP", "654321");
        var plan = YarnNpm.Publish(FakeTool(), s => s.SetOtp(otp));
        Assert.Equal("654321", plan.Arguments[IndexOf(plan.Arguments, "--otp") + 1]);
        Assert.Same(otp, Assert.Single(plan.Secrets));
    }

    // =========================================================
    // npm tag
    // =========================================================

    [Fact]
    public void NpmTagAdd_Throws_When_PackageVersion_Missing()
        => Assert.Throws<InvalidOperationException>(() => YarnNpm.TagAdd(FakeTool(), s => s.SetTag("latest")));

    [Fact]
    public void NpmTagAdd_Throws_When_Tag_Missing()
        => Assert.Throws<InvalidOperationException>(() => YarnNpm.TagAdd(FakeTool(), s => s.SetPackageVersion("p@1")));

    [Fact]
    public void NpmTagAdd_Positionals_PackageVersion_Then_Tag()
    {
        var args = YarnNpm.TagAdd(FakeTool(), s => s
            .SetPackageVersion("@holdfast-io/browser@1.2.3")
            .SetTag("latest")).Arguments;
        Assert.Equal(["npm", "tag", "add", "@holdfast-io/browser@1.2.3", "latest"], args);
    }

    [Fact]
    public void NpmTagRemove_Positionals_Package_Then_Tag()
    {
        var args = YarnNpm.TagRemove(FakeTool(), s => s
            .SetPackage("@holdfast-io/browser")
            .SetTag("beta")).Arguments;
        Assert.Equal(["npm", "tag", "remove", "@holdfast-io/browser", "beta"], args);
    }

    [Fact]
    public void NpmWhoami_Scope_And_Publish_Round_Trip()
    {
        var args = YarnNpm.Whoami(FakeTool(), s => s
            .SetScope("@holdfast-io")
            .SetPublish()).Arguments;
        Assert.Equal(["npm", "whoami"], args.Take(2));
        Assert.Equal("@holdfast-io", args[IndexOf(args, "--scope") + 1]);
        Assert.Contains("--publish", args);
    }

    // =========================================================
    // Working directory precedence
    // =========================================================

    [Fact]
    public void WorkingDirectory_Settings_Wins_Over_Tool()
    {
        var tool = new Tool(AbsolutePath.Create("/fake/yarn"), workingDirectory: "/from-tool");
        var plan = Yarn.Install(tool, s => s.SetWorkingDirectory("/from-settings"));
        Assert.Equal("/from-settings", plan.WorkingDirectory);
    }

    [Fact]
    public void WorkingDirectory_Falls_Back_To_Tool_When_Settings_Null()
    {
        var tool = new Tool(AbsolutePath.Create("/fake/yarn"), workingDirectory: "/from-tool");
        var plan = Yarn.Install(tool);
        Assert.Equal("/from-tool", plan.WorkingDirectory);
    }
}
