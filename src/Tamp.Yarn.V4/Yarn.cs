namespace Tamp.Yarn.V4;

/// <summary>Top-level facade for <c>yarn</c> verbs.</summary>
/// <remarks>
/// <para>Resolve the tool via <c>[NuGetPackage(UseSystemPath = true)]</c> — yarn is
/// invoked on PATH (typically via corepack):</para>
/// <code>
/// [NuGetPackage("yarn", UseSystemPath = true)]
/// readonly Tool Yarn = null!;
/// </code>
/// </remarks>
public static class Yarn
{
    /// <summary><c>yarn install</c> — install / sync project dependencies.</summary>
    public static CommandPlan Install(Tool tool, Action<YarnInstallSettings>? configure = null)
        => Build<YarnInstallSettings>(tool, configure);

    /// <summary><c>yarn run &lt;script&gt;</c> — run an npm script defined in package.json.</summary>
    public static CommandPlan Run(Tool tool, Action<YarnRunSettings> configure)
        => Build<YarnRunSettings>(tool, configure);

    /// <summary><c>yarn dlx</c> — run a package in a temporary environment.</summary>
    public static CommandPlan Dlx(Tool tool, Action<YarnDlxSettings> configure)
        => Build<YarnDlxSettings>(tool, configure);

    /// <summary><c>yarn pack</c> — create a tarball from the current workspace.</summary>
    public static CommandPlan Pack(Tool tool, Action<YarnPackSettings>? configure = null)
        => Build<YarnPackSettings>(tool, configure);

    /// <summary><c>yarn dedupe</c> — collapse overlapping dependency ranges.</summary>
    public static CommandPlan Dedupe(Tool tool, Action<YarnDedupeSettings>? configure = null)
        => Build<YarnDedupeSettings>(tool, configure);

    /// <summary><c>yarn exec</c> — execute a binary in the yarn-managed environment.</summary>
    public static CommandPlan Exec(Tool tool, Action<YarnExecSettings> configure)
        => Build<YarnExecSettings>(tool, configure);

    /// <summary>Raw escape hatch for verbs we haven't typed yet.</summary>
    public static CommandPlan Raw(Tool tool, params string[] arguments)
    {
        if (tool is null) throw new ArgumentNullException(nameof(tool));
        if (arguments is null || arguments.Length == 0)
            throw new ArgumentException("Raw requires at least one argument.", nameof(arguments));
        var s = new YarnRawSettings();
        s.AddArgs(arguments);
        return s.ToCommandPlan(tool);
    }

    private static CommandPlan Build<T>(Tool tool, Action<T>? configure) where T : YarnSettingsBase, new()
    {
        if (tool is null) throw new ArgumentNullException(nameof(tool));
        var s = new T();
        configure?.Invoke(s);
        return s.ToCommandPlan(tool);
    }
}

/// <summary>Sub-facade for <c>yarn workspaces &lt;list|foreach|focus&gt;</c>.</summary>
public static class YarnWorkspaces
{
    public static CommandPlan List(Tool tool, Action<YarnWorkspacesListSettings>? configure = null)
        => Build<YarnWorkspacesListSettings>(tool, configure);

    public static CommandPlan Foreach(Tool tool, Action<YarnWorkspacesForeachSettings> configure)
        => Build<YarnWorkspacesForeachSettings>(tool, configure);

    public static CommandPlan Focus(Tool tool, Action<YarnWorkspacesFocusSettings>? configure = null)
        => Build<YarnWorkspacesFocusSettings>(tool, configure);

    private static CommandPlan Build<T>(Tool tool, Action<T>? configure) where T : YarnSettingsBase, new()
    {
        if (tool is null) throw new ArgumentNullException(nameof(tool));
        var s = new T();
        configure?.Invoke(s);
        return s.ToCommandPlan(tool);
    }
}

/// <summary>Sub-facade for <c>yarn npm &lt;publish|tag add|tag remove|whoami&gt;</c>.</summary>
public static class YarnNpm
{
    public static CommandPlan Publish(Tool tool, Action<YarnNpmPublishSettings>? configure = null)
        => Build<YarnNpmPublishSettings>(tool, configure);

    public static CommandPlan TagAdd(Tool tool, Action<YarnNpmTagAddSettings> configure)
        => Build<YarnNpmTagAddSettings>(tool, configure);

    public static CommandPlan TagRemove(Tool tool, Action<YarnNpmTagRemoveSettings> configure)
        => Build<YarnNpmTagRemoveSettings>(tool, configure);

    public static CommandPlan Whoami(Tool tool, Action<YarnNpmWhoamiSettings>? configure = null)
        => Build<YarnNpmWhoamiSettings>(tool, configure);

    private static CommandPlan Build<T>(Tool tool, Action<T>? configure) where T : YarnSettingsBase, new()
    {
        if (tool is null) throw new ArgumentNullException(nameof(tool));
        var s = new T();
        configure?.Invoke(s);
        return s.ToCommandPlan(tool);
    }
}
