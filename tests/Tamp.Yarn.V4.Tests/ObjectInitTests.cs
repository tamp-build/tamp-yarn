using Xunit;

namespace Tamp.Yarn.V4.Tests;

/// <summary>
/// TAM-161 satellite fanout: every public verb wrapper has both a fluent
/// (<c>Action&lt;TSettings&gt;</c>) form and an object-init (<c>TSettings</c>) form.
/// Both must emit identical CommandPlans for equivalent input.
/// </summary>
public sealed class ObjectInitTests
{
    private static Tool FakeTool() => new(AbsolutePath.Create("/fake/yarn"));

    // =========================================================
    // Round-trip equivalence — fluent vs object-init for a representative verb
    // =========================================================

    [Fact]
    public void Install_ObjectInit_Emits_Identical_Plan_To_Fluent()
    {
        var tool = FakeTool();
        var fluent = Yarn.Install(tool, s => s
            .SetImmutable()
            .SetCheckCache()
            .SetMode("skip-build")
            .SetJson());

        var objectInit = Yarn.Install(tool, new YarnInstallSettings
        {
            Immutable = true,
            CheckCache = true,
            Mode = "skip-build",
            Json = true,
        });

        Assert.Equal(fluent.Executable, objectInit.Executable);
        Assert.Equal(fluent.Arguments, objectInit.Arguments);
    }

    [Fact]
    public void Run_ObjectInit_Emits_Identical_Plan_To_Fluent()
    {
        var tool = FakeTool();
        var fluent = Yarn.Run(tool, s => s
            .SetScript("build")
            .SetBinariesOnly()
            .AddScriptArg("--verbose")
            .AddScriptArg("--no-color"));

        var objectInit = Yarn.Run(tool, new YarnRunSettings
        {
            Script = "build",
            BinariesOnly = true,
            ScriptArgs = { "--verbose", "--no-color" },
        });

        Assert.Equal(fluent.Arguments, objectInit.Arguments);
    }

    [Fact]
    public void Dlx_ObjectInit_Emits_Identical_Plan_To_Fluent()
    {
        var tool = FakeTool();
        var fluent = Yarn.Dlx(tool, s => s
            .AddPackage("create-react-app")
            .SetCommand("create-react-app")
            .AddCommandArg("my-app")
            .SetQuiet());

        var objectInit = Yarn.Dlx(tool, new YarnDlxSettings
        {
            Packages = { "create-react-app" },
            Command = "create-react-app",
            CommandArgs = { "my-app" },
            Quiet = true,
        });

        Assert.Equal(fluent.Arguments, objectInit.Arguments);
    }

    [Fact]
    public void Pack_ObjectInit_Round_Trips()
    {
        var tool = FakeTool();
        var plan = Yarn.Pack(tool, new YarnPackSettings
        {
            Out = "./out.tgz",
            DryRun = true,
            Json = true,
            Filename = { "src/**", "lib/**" },
        });
        Assert.Contains("pack", plan.Arguments);
        Assert.Contains("--out", plan.Arguments);
        Assert.Contains("./out.tgz", plan.Arguments);
        Assert.Contains("--dry-run", plan.Arguments);
        Assert.Contains("--json", plan.Arguments);
        Assert.Equal(2, plan.Arguments.Count(a => a == "--filename"));
    }

    [Fact]
    public void Dedupe_ObjectInit_Emits_Identical_Plan_To_Fluent()
    {
        var tool = FakeTool();
        var fluent = Yarn.Dedupe(tool, s => s
            .SetStrategy("highest")
            .SetCheck()
            .SetJson()
            .AddPattern("lodash"));

        var objectInit = Yarn.Dedupe(tool, new YarnDedupeSettings
        {
            Strategy = "highest",
            Check = true,
            Json = true,
            Patterns = { "lodash" },
        });

        Assert.Equal(fluent.Arguments, objectInit.Arguments);
    }

    [Fact]
    public void Exec_ObjectInit_Emits_Identical_Plan_To_Fluent()
    {
        var tool = FakeTool();
        var fluent = Yarn.Exec(tool, s => s.SetCommand("tsc").AddCommandArg("--noEmit"));
        var objectInit = Yarn.Exec(tool, new YarnExecSettings
        {
            Command = "tsc",
            CommandArgs = { "--noEmit" },
        });
        Assert.Equal(fluent.Arguments, objectInit.Arguments);
    }

    [Fact]
    public void WorkspacesList_ObjectInit_Emits_Identical_Plan_To_Fluent()
    {
        var tool = FakeTool();
        var fluent = YarnWorkspaces.List(tool, s => s
            .SetSince()
            .SetRecursive()
            .SetNoPrivate()
            .SetJson());

        var objectInit = YarnWorkspaces.List(tool, new YarnWorkspacesListSettings
        {
            Since = true,
            Recursive = true,
            NoPrivate = true,
            Json = true,
        });

        Assert.Equal(fluent.Arguments, objectInit.Arguments);
    }

    [Fact]
    public void WorkspacesForeach_ObjectInit_Emits_Identical_Plan_To_Fluent()
    {
        var tool = FakeTool();
        var fluent = YarnWorkspaces.Foreach(tool, s => s
            .SetAll()
            .SetParallel()
            .SetTopological()
            .SetJobs(4)
            .AddInclude("@scope/*")
            .AddExclude("@scope/legacy")
            .SetCommand("build")
            .AddCommandArg("--prod"));

        var objectInit = YarnWorkspaces.Foreach(tool, new YarnWorkspacesForeachSettings
        {
            All = true,
            Parallel = true,
            Topological = true,
            Jobs = 4,
            Include = { "@scope/*" },
            Exclude = { "@scope/legacy" },
            Command = "build",
            CommandArgs = { "--prod" },
        });

        Assert.Equal(fluent.Arguments, objectInit.Arguments);
    }

    [Fact]
    public void WorkspacesFocus_ObjectInit_Round_Trips()
    {
        var tool = FakeTool();
        var plan = YarnWorkspaces.Focus(tool, new YarnWorkspacesFocusSettings
        {
            Workspaces = { "@scope/a", "@scope/b" },
            Production = true,
            Json = true,
        });
        Assert.Contains("focus", plan.Arguments);
        Assert.Contains("--production", plan.Arguments);
        Assert.Contains("--json", plan.Arguments);
        Assert.Contains("@scope/a", plan.Arguments);
        Assert.Contains("@scope/b", plan.Arguments);
    }

    [Fact]
    public void NpmPublish_ObjectInit_Emits_Identical_Plan_To_Fluent()
    {
        var tool = FakeTool();
        var fluent = YarnNpm.Publish(tool, s => s
            .SetTag("beta")
            .SetAccess(YarnNpmAccess.Public)
            .SetTolerateRepublish());

        var objectInit = YarnNpm.Publish(tool, new YarnNpmPublishSettings
        {
            Tag = "beta",
            Access = YarnNpmAccess.Public,
            TolerateRepublish = true,
        });

        Assert.Equal(fluent.Arguments, objectInit.Arguments);
    }

    [Fact]
    public void NpmTagAdd_ObjectInit_Round_Trips()
    {
        var tool = FakeTool();
        var plan = YarnNpm.TagAdd(tool, new YarnNpmTagAddSettings
        {
            PackageVersion = "@scope/pkg@1.2.3",
            Tag = "next",
        });
        Assert.Contains("tag", plan.Arguments);
        Assert.Contains("add", plan.Arguments);
        Assert.Contains("@scope/pkg@1.2.3", plan.Arguments);
        Assert.Contains("next", plan.Arguments);
    }

    [Fact]
    public void NpmTagRemove_ObjectInit_Round_Trips()
    {
        var tool = FakeTool();
        var plan = YarnNpm.TagRemove(tool, new YarnNpmTagRemoveSettings
        {
            Package = "@scope/pkg",
            Tag = "next",
        });
        Assert.Contains("tag", plan.Arguments);
        Assert.Contains("remove", plan.Arguments);
        Assert.Contains("@scope/pkg", plan.Arguments);
        Assert.Contains("next", plan.Arguments);
    }

    [Fact]
    public void NpmWhoami_ObjectInit_Emits_Identical_Plan_To_Fluent()
    {
        var tool = FakeTool();
        var fluent = YarnNpm.Whoami(tool, s => s.SetScope("@scope").SetPublish());
        var objectInit = YarnNpm.Whoami(tool, new YarnNpmWhoamiSettings
        {
            Scope = "@scope",
            Publish = true,
        });
        Assert.Equal(fluent.Arguments, objectInit.Arguments);
    }

    // =========================================================
    // Null-tool / null-settings guards on every object-init overload
    // =========================================================

    [Fact]
    public void ObjectInit_Throws_On_Null_Tool()
    {
        Assert.Throws<ArgumentNullException>(() => Yarn.Install(null!, new YarnInstallSettings()));
        Assert.Throws<ArgumentNullException>(() => Yarn.Run(null!, new YarnRunSettings { Script = "x" }));
        Assert.Throws<ArgumentNullException>(() => Yarn.Dlx(null!, new YarnDlxSettings { Command = "x" }));
        Assert.Throws<ArgumentNullException>(() => Yarn.Pack(null!, new YarnPackSettings()));
        Assert.Throws<ArgumentNullException>(() => Yarn.Dedupe(null!, new YarnDedupeSettings()));
        Assert.Throws<ArgumentNullException>(() => Yarn.Exec(null!, new YarnExecSettings { Command = "x" }));
        Assert.Throws<ArgumentNullException>(() => YarnWorkspaces.List(null!, new YarnWorkspacesListSettings()));
        Assert.Throws<ArgumentNullException>(() => YarnWorkspaces.Foreach(null!, new YarnWorkspacesForeachSettings { Command = "x" }));
        Assert.Throws<ArgumentNullException>(() => YarnWorkspaces.Focus(null!, new YarnWorkspacesFocusSettings()));
        Assert.Throws<ArgumentNullException>(() => YarnNpm.Publish(null!, new YarnNpmPublishSettings()));
        Assert.Throws<ArgumentNullException>(() => YarnNpm.TagAdd(null!, new YarnNpmTagAddSettings { PackageVersion = "p@1", Tag = "t" }));
        Assert.Throws<ArgumentNullException>(() => YarnNpm.TagRemove(null!, new YarnNpmTagRemoveSettings { Package = "p", Tag = "t" }));
        Assert.Throws<ArgumentNullException>(() => YarnNpm.Whoami(null!, new YarnNpmWhoamiSettings()));
    }

    [Fact]
    public void ObjectInit_Throws_On_Null_Settings()
    {
        var tool = FakeTool();
        Assert.Throws<ArgumentNullException>(() => Yarn.Install(tool, (YarnInstallSettings)null!));
        Assert.Throws<ArgumentNullException>(() => Yarn.Run(tool, (YarnRunSettings)null!));
        Assert.Throws<ArgumentNullException>(() => Yarn.Dlx(tool, (YarnDlxSettings)null!));
        Assert.Throws<ArgumentNullException>(() => Yarn.Pack(tool, (YarnPackSettings)null!));
        Assert.Throws<ArgumentNullException>(() => Yarn.Dedupe(tool, (YarnDedupeSettings)null!));
        Assert.Throws<ArgumentNullException>(() => Yarn.Exec(tool, (YarnExecSettings)null!));
        Assert.Throws<ArgumentNullException>(() => YarnWorkspaces.List(tool, (YarnWorkspacesListSettings)null!));
        Assert.Throws<ArgumentNullException>(() => YarnWorkspaces.Foreach(tool, (YarnWorkspacesForeachSettings)null!));
        Assert.Throws<ArgumentNullException>(() => YarnWorkspaces.Focus(tool, (YarnWorkspacesFocusSettings)null!));
        Assert.Throws<ArgumentNullException>(() => YarnNpm.Publish(tool, (YarnNpmPublishSettings)null!));
        Assert.Throws<ArgumentNullException>(() => YarnNpm.TagAdd(tool, (YarnNpmTagAddSettings)null!));
        Assert.Throws<ArgumentNullException>(() => YarnNpm.TagRemove(tool, (YarnNpmTagRemoveSettings)null!));
        Assert.Throws<ArgumentNullException>(() => YarnNpm.Whoami(tool, (YarnNpmWhoamiSettings)null!));
    }

    // =========================================================
    // Smoke: every object-init overload compiles and returns non-null
    // =========================================================

    [Fact]
    public void All_ObjectInit_Wrappers_Surface_Compiles_And_Returns_CommandPlan()
    {
        var tool = FakeTool();
        Assert.NotNull(Yarn.Install(tool, new YarnInstallSettings()));
        Assert.NotNull(Yarn.Run(tool, new YarnRunSettings { Script = "build" }));
        Assert.NotNull(Yarn.Dlx(tool, new YarnDlxSettings { Command = "x" }));
        Assert.NotNull(Yarn.Pack(tool, new YarnPackSettings()));
        Assert.NotNull(Yarn.Dedupe(tool, new YarnDedupeSettings()));
        Assert.NotNull(Yarn.Exec(tool, new YarnExecSettings { Command = "x" }));
        Assert.NotNull(YarnWorkspaces.List(tool, new YarnWorkspacesListSettings()));
        Assert.NotNull(YarnWorkspaces.Foreach(tool, new YarnWorkspacesForeachSettings { Command = "build" }));
        Assert.NotNull(YarnWorkspaces.Focus(tool, new YarnWorkspacesFocusSettings()));
        Assert.NotNull(YarnNpm.Publish(tool, new YarnNpmPublishSettings()));
        Assert.NotNull(YarnNpm.TagAdd(tool, new YarnNpmTagAddSettings { PackageVersion = "p@1", Tag = "latest" }));
        Assert.NotNull(YarnNpm.TagRemove(tool, new YarnNpmTagRemoveSettings { Package = "p", Tag = "latest" }));
        Assert.NotNull(YarnNpm.Whoami(tool, new YarnNpmWhoamiSettings()));
    }
}
