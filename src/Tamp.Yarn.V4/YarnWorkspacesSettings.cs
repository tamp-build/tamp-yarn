namespace Tamp.Yarn.V4;

/// <summary>Settings for <c>yarn workspaces list</c>.</summary>
public sealed class YarnWorkspacesListSettings : YarnSettingsBase
{
    /// <summary>Only show workspaces touched since the last release.</summary>
    public bool Since { get; set; }

    /// <summary>Recurse into transitive workspace dependencies.</summary>
    public bool Recursive { get; set; }

    /// <summary>Exclude private (unpublished) workspaces.</summary>
    public bool NoPrivate { get; set; }

    /// <summary>Verbose mode (shows dependency arrows).</summary>
    public bool Verbose { get; set; }

    /// <summary>NDJSON output.</summary>
    public bool Json { get; set; }

    public YarnWorkspacesListSettings SetSince(bool v = true) { Since = v; return this; }
    public YarnWorkspacesListSettings SetRecursive(bool v = true) { Recursive = v; return this; }
    public YarnWorkspacesListSettings SetNoPrivate(bool v = true) { NoPrivate = v; return this; }
    public YarnWorkspacesListSettings SetVerbose(bool v = true) { Verbose = v; return this; }
    public YarnWorkspacesListSettings SetJson(bool v = true) { Json = v; return this; }

    protected override IEnumerable<string> BuildArguments()
    {
        yield return "workspaces";
        yield return "list";
        if (Since) yield return "--since";
        if (Recursive) yield return "-R";
        if (NoPrivate) yield return "--no-private";
        if (Verbose) yield return "-v";
        if (Json) yield return "--json";
    }
}

/// <summary>Settings for <c>yarn workspaces foreach &lt;commandName&gt; [args...]</c>.</summary>
public sealed class YarnWorkspacesForeachSettings : YarnSettingsBase
{
    /// <summary>Start the iteration set from the given filter glob (transitive deps included).</summary>
    public string? From { get; set; }

    /// <summary>Iterate over every workspace.</summary>
    public bool All { get; set; }

    /// <summary>Recurse into transitive workspace deps.</summary>
    public bool Recursive { get; set; }

    /// <summary>Only iterate the current worktree.</summary>
    public bool Worktree { get; set; }

    /// <summary>Verbose per-step output.</summary>
    public bool Verbose { get; set; }

    /// <summary>Run in parallel.</summary>
    public bool Parallel { get; set; }

    /// <summary>Interleave parallel output instead of buffering per workspace.</summary>
    public bool Interlaced { get; set; }

    /// <summary>Max parallel jobs.</summary>
    public int? Jobs { get; set; }

    /// <summary>Order by dependency graph (deps build before dependents).</summary>
    public bool Topological { get; set; }

    /// <summary>Topological order including dev dependencies.</summary>
    public bool TopologicalDev { get; set; }

    /// <summary>Workspace name globs to include.</summary>
    public List<string> Include { get; } = [];

    /// <summary>Workspace name globs to exclude.</summary>
    public List<string> Exclude { get; } = [];

    /// <summary>Skip private workspaces.</summary>
    public bool NoPrivate { get; set; }

    /// <summary>Restrict to workspaces touched since the last release.</summary>
    public bool Since { get; set; }

    /// <summary>Show the plan without running.</summary>
    public bool DryRun { get; set; }

    /// <summary>Command to run in each workspace. Required.</summary>
    public string? Command { get; set; }

    /// <summary>Positional args to the command.</summary>
    public List<string> CommandArgs { get; } = [];

    public YarnWorkspacesForeachSettings SetFrom(string? glob) { From = glob; return this; }
    public YarnWorkspacesForeachSettings SetAll(bool v = true) { All = v; return this; }
    public YarnWorkspacesForeachSettings SetRecursive(bool v = true) { Recursive = v; return this; }
    public YarnWorkspacesForeachSettings SetWorktree(bool v = true) { Worktree = v; return this; }
    public YarnWorkspacesForeachSettings SetVerbose(bool v = true) { Verbose = v; return this; }
    public YarnWorkspacesForeachSettings SetParallel(bool v = true) { Parallel = v; return this; }
    public YarnWorkspacesForeachSettings SetInterlaced(bool v = true) { Interlaced = v; return this; }
    public YarnWorkspacesForeachSettings SetJobs(int n) { Jobs = n; return this; }
    public YarnWorkspacesForeachSettings SetTopological(bool v = true) { Topological = v; return this; }
    public YarnWorkspacesForeachSettings SetTopologicalDev(bool v = true) { TopologicalDev = v; return this; }
    public YarnWorkspacesForeachSettings AddInclude(string glob) { Include.Add(glob); return this; }
    public YarnWorkspacesForeachSettings AddExclude(string glob) { Exclude.Add(glob); return this; }
    public YarnWorkspacesForeachSettings SetNoPrivate(bool v = true) { NoPrivate = v; return this; }
    public YarnWorkspacesForeachSettings SetSince(bool v = true) { Since = v; return this; }
    public YarnWorkspacesForeachSettings SetDryRun(bool v = true) { DryRun = v; return this; }
    public YarnWorkspacesForeachSettings SetCommand(string? command) { Command = command; return this; }
    public YarnWorkspacesForeachSettings AddCommandArg(string arg) { CommandArgs.Add(arg); return this; }

    protected override IEnumerable<string> BuildArguments()
    {
        if (string.IsNullOrEmpty(Command)) throw new InvalidOperationException("yarn workspaces foreach: Command is required.");
        yield return "workspaces";
        yield return "foreach";
        if (!string.IsNullOrEmpty(From)) { yield return "--from"; yield return From!; }
        if (All) yield return "-A";
        if (Recursive) yield return "-R";
        if (Worktree) yield return "-W";
        if (Verbose) yield return "-v";
        if (Parallel) yield return "-p";
        if (Interlaced) yield return "-i";
        if (Jobs is { } j) { yield return "-j"; yield return j.ToString(); }
        if (Topological) yield return "-t";
        if (TopologicalDev) yield return "--topological-dev";
        foreach (var g in Include) { yield return "--include"; yield return g; }
        foreach (var g in Exclude) { yield return "--exclude"; yield return g; }
        if (NoPrivate) yield return "--no-private";
        if (Since) yield return "--since";
        if (DryRun) yield return "-n";
        yield return Command!;
        foreach (var a in CommandArgs) yield return a;
    }
}

/// <summary>Settings for <c>yarn workspaces focus [workspace...]</c>.</summary>
public sealed class YarnWorkspacesFocusSettings : YarnSettingsBase
{
    /// <summary>Workspace names to focus on. Empty = the current workspace only.</summary>
    public List<string> Workspaces { get; } = [];

    /// <summary>Install in production mode (no devDependencies).</summary>
    public bool Production { get; set; }

    /// <summary>Focus across all workspaces.</summary>
    public bool All { get; set; }

    public bool Json { get; set; }

    public YarnWorkspacesFocusSettings AddWorkspace(string name) { Workspaces.Add(name); return this; }
    public YarnWorkspacesFocusSettings SetProduction(bool v = true) { Production = v; return this; }
    public YarnWorkspacesFocusSettings SetAll(bool v = true) { All = v; return this; }
    public YarnWorkspacesFocusSettings SetJson(bool v = true) { Json = v; return this; }

    protected override IEnumerable<string> BuildArguments()
    {
        yield return "workspaces";
        yield return "focus";
        if (Json) yield return "--json";
        if (Production) yield return "--production";
        if (All) yield return "-A";
        foreach (var w in Workspaces) yield return w;
    }
}
