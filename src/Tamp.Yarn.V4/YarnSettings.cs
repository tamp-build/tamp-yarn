namespace Tamp.Yarn.V4;

/// <summary>Settings for <c>yarn install</c>.</summary>
public sealed class YarnInstallSettings : YarnSettingsBase
{
    /// <summary>Abort with non-zero exit if the lockfile would be modified. THE CI flag.</summary>
    public bool Immutable { get; set; }

    /// <summary>Abort if the cache folder would be modified.</summary>
    public bool ImmutableCache { get; set; }

    /// <summary>Re-fetch package metadata into the lockfile.</summary>
    public bool RefreshLockfile { get; set; }

    /// <summary>Always re-fetch packages and verify checksums.</summary>
    public bool CheckCache { get; set; }

    /// <summary>Validate package-resolution coherence.</summary>
    public bool CheckResolutions { get; set; }

    /// <summary>Verbose build output for dependencies.</summary>
    public bool InlineBuilds { get; set; }

    /// <summary>NDJSON output mode.</summary>
    public bool Json { get; set; }

    /// <summary>Artifact mode override (see <c>--mode</c> in Yarn docs, e.g. <c>skip-build</c>, <c>update-lockfile</c>).</summary>
    public string? Mode { get; set; }

    public YarnInstallSettings SetImmutable(bool v = true) { Immutable = v; return this; }
    public YarnInstallSettings SetImmutableCache(bool v = true) { ImmutableCache = v; return this; }
    public YarnInstallSettings SetRefreshLockfile(bool v = true) { RefreshLockfile = v; return this; }
    public YarnInstallSettings SetCheckCache(bool v = true) { CheckCache = v; return this; }
    public YarnInstallSettings SetCheckResolutions(bool v = true) { CheckResolutions = v; return this; }
    public YarnInstallSettings SetInlineBuilds(bool v = true) { InlineBuilds = v; return this; }
    public YarnInstallSettings SetJson(bool v = true) { Json = v; return this; }
    public YarnInstallSettings SetMode(string? mode) { Mode = mode; return this; }

    protected override IEnumerable<string> BuildArguments()
    {
        yield return "install";
        if (Immutable) yield return "--immutable";
        if (ImmutableCache) yield return "--immutable-cache";
        if (RefreshLockfile) yield return "--refresh-lockfile";
        if (CheckCache) yield return "--check-cache";
        if (CheckResolutions) yield return "--check-resolutions";
        if (InlineBuilds) yield return "--inline-builds";
        if (Json) yield return "--json";
        if (!string.IsNullOrEmpty(Mode)) { yield return "--mode"; yield return Mode!; }
    }
}

/// <summary>Settings for <c>yarn run &lt;script&gt; [args...]</c> — or <c>yarn &lt;script&gt;</c> (the shorthand, which the wrapper still emits as the explicit <c>run</c> form).</summary>
public sealed class YarnRunSettings : YarnSettingsBase
{
    /// <summary>The script name in <c>package.json</c>'s <c>scripts</c> block. Required.</summary>
    public string? Script { get; set; }

    /// <summary>Positional arguments to pass to the script.</summary>
    public List<string> ScriptArgs { get; } = [];

    /// <summary>Override the working-tree binary lookup (rare).</summary>
    public bool BinariesOnly { get; set; }

    public YarnRunSettings SetScript(string? name) { Script = name; return this; }
    public YarnRunSettings AddScriptArg(string arg) { ScriptArgs.Add(arg); return this; }
    public YarnRunSettings AddScriptArgs(IEnumerable<string> args) { ScriptArgs.AddRange(args); return this; }
    public YarnRunSettings SetBinariesOnly(bool v = true) { BinariesOnly = v; return this; }

    protected override IEnumerable<string> BuildArguments()
    {
        if (string.IsNullOrEmpty(Script)) throw new InvalidOperationException("yarn run: Script is required.");
        yield return "run";
        if (BinariesOnly) yield return "--binaries-only";
        yield return Script!;
        foreach (var a in ScriptArgs) yield return a;
    }
}

/// <summary>Settings for <c>yarn dlx [-p &lt;pkg&gt;]... &lt;command&gt; [args...]</c>.</summary>
public sealed class YarnDlxSettings : YarnSettingsBase
{
    /// <summary>Packages to install for the run. Repeated as <c>-p &lt;pkg&gt;</c>. The first positional is the command to invoke.</summary>
    public List<string> Packages { get; } = [];

    /// <summary>The command name (typically a bin from one of the installed packages). Required.</summary>
    public string? Command { get; set; }

    /// <summary>Positional args after the command.</summary>
    public List<string> CommandArgs { get; } = [];

    /// <summary>Suppress output (only the command's own output remains).</summary>
    public bool Quiet { get; set; }

    public YarnDlxSettings AddPackage(string pkg) { Packages.Add(pkg); return this; }
    public YarnDlxSettings SetCommand(string? command) { Command = command; return this; }
    public YarnDlxSettings AddCommandArg(string arg) { CommandArgs.Add(arg); return this; }
    public YarnDlxSettings AddCommandArgs(IEnumerable<string> args) { CommandArgs.AddRange(args); return this; }
    public YarnDlxSettings SetQuiet(bool v = true) { Quiet = v; return this; }

    protected override IEnumerable<string> BuildArguments()
    {
        if (string.IsNullOrEmpty(Command)) throw new InvalidOperationException("yarn dlx: Command is required.");
        yield return "dlx";
        foreach (var p in Packages) { yield return "-p"; yield return p; }
        if (Quiet) yield return "--quiet";
        yield return Command!;
        foreach (var a in CommandArgs) yield return a;
    }
}

/// <summary>Settings for <c>yarn pack</c>.</summary>
public sealed class YarnPackSettings : YarnSettingsBase
{
    /// <summary>Output tarball path (without trailing extension). Maps to <c>--out</c>.</summary>
    public string? Out { get; set; }

    /// <summary>Generate the tarball but don't write it (returns the file list).</summary>
    public bool DryRun { get; set; }

    /// <summary>Include files matching this glob, even if otherwise excluded.</summary>
    public List<string> Filename { get; } = [];

    /// <summary>NDJSON output.</summary>
    public bool Json { get; set; }

    public YarnPackSettings SetOut(string? path) { Out = path; return this; }
    public YarnPackSettings SetDryRun(bool v = true) { DryRun = v; return this; }
    public YarnPackSettings AddFilename(string glob) { Filename.Add(glob); return this; }
    public YarnPackSettings SetJson(bool v = true) { Json = v; return this; }

    protected override IEnumerable<string> BuildArguments()
    {
        yield return "pack";
        if (!string.IsNullOrEmpty(Out)) { yield return "--out"; yield return Out!; }
        if (DryRun) yield return "--dry-run";
        foreach (var f in Filename) { yield return "--filename"; yield return f; }
        if (Json) yield return "--json";
    }
}

/// <summary>Settings for <c>yarn dedupe</c>.</summary>
public sealed class YarnDedupeSettings : YarnSettingsBase
{
    /// <summary>Strategy: <c>highest</c> (default) or <c>fewer</c>.</summary>
    public string? Strategy { get; set; }

    /// <summary>Exit non-zero if dedupe would change something (CI gate).</summary>
    public bool Check { get; set; }

    /// <summary>Patterns to limit dedupe to.</summary>
    public List<string> Patterns { get; } = [];

    public bool Json { get; set; }
    public string? Mode { get; set; }

    public YarnDedupeSettings SetStrategy(string? s) { Strategy = s; return this; }
    public YarnDedupeSettings SetCheck(bool v = true) { Check = v; return this; }
    public YarnDedupeSettings AddPattern(string p) { Patterns.Add(p); return this; }
    public YarnDedupeSettings SetJson(bool v = true) { Json = v; return this; }
    public YarnDedupeSettings SetMode(string? mode) { Mode = mode; return this; }

    protected override IEnumerable<string> BuildArguments()
    {
        yield return "dedupe";
        if (!string.IsNullOrEmpty(Strategy)) { yield return "--strategy"; yield return Strategy!; }
        if (Check) yield return "--check";
        if (Json) yield return "--json";
        if (!string.IsNullOrEmpty(Mode)) { yield return "--mode"; yield return Mode!; }
        foreach (var p in Patterns) yield return p;
    }
}

/// <summary>Settings for <c>yarn exec &lt;cmd&gt; [args...]</c>.</summary>
public sealed class YarnExecSettings : YarnSettingsBase
{
    public string? Command { get; set; }
    public List<string> CommandArgs { get; } = [];

    public YarnExecSettings SetCommand(string? command) { Command = command; return this; }
    public YarnExecSettings AddCommandArg(string arg) { CommandArgs.Add(arg); return this; }
    public YarnExecSettings AddCommandArgs(IEnumerable<string> args) { CommandArgs.AddRange(args); return this; }

    protected override IEnumerable<string> BuildArguments()
    {
        if (string.IsNullOrEmpty(Command)) throw new InvalidOperationException("yarn exec: Command is required.");
        yield return "exec";
        yield return Command!;
        foreach (var a in CommandArgs) yield return a;
    }
}

/// <summary>Escape-hatch settings for verbs we haven't typed yet.</summary>
public sealed class YarnRawSettings : YarnSettingsBase
{
    public List<string> Arguments { get; } = [];

    public YarnRawSettings AddArg(string arg) { Arguments.Add(arg); return this; }
    public YarnRawSettings AddArgs(IEnumerable<string> args) { Arguments.AddRange(args); return this; }
    public YarnRawSettings AddArgs(params string[] args) { Arguments.AddRange(args); return this; }

    protected override IEnumerable<string> BuildArguments()
    {
        if (Arguments.Count == 0) throw new InvalidOperationException("yarn raw: at least one argument is required.");
        foreach (var a in Arguments) yield return a;
    }
}
