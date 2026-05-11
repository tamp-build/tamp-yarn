namespace Tamp.Yarn.V4;

/// <summary>
/// Common knobs shared by every <c>yarn</c> verb's settings class:
/// working directory of the spawned process, additional environment
/// variables, and the npm auth token (Yarn reads it from
/// <c>NPM_CONFIG_TOKEN</c> or <c>YARN_NPM_AUTH_TOKEN</c>).
/// </summary>
/// <remarks>
/// Auth design choice: Yarn's CLI doesn't take registry tokens as
/// flags — auth lives in <c>.yarnrc.yml</c> or in env vars at run
/// time. Tamp passes the token via <see cref="Environment"/> /
/// <see cref="NpmAuthToken"/>; the spawned process picks it up; the
/// runner's redaction table covers it.
/// </remarks>
public abstract class YarnSettingsBase
{
    /// <summary>Working directory of the spawned <c>yarn</c> process. Typically the workspace root.</summary>
    public string? WorkingDirectory { get; set; }

    /// <summary>Per-invocation environment variables on top of the inherited environment.</summary>
    public Dictionary<string, string> EnvironmentVariables { get; } = new();

    /// <summary>
    /// npm-registry auth token. Set as <c>NPM_CONFIG_TOKEN</c> (the
    /// var the underlying npm-protocol code reads) on the spawned
    /// process and registered with the runner's redaction table.
    /// </summary>
    public Secret? NpmAuthToken { get; set; }

    /// <summary>Subclasses build the per-verb argument list. The verb token(s) come first.</summary>
    protected abstract IEnumerable<string> BuildArguments();

    /// <summary>
    /// Subclasses extend the secret list (e.g. <c>yarn npm publish</c>
    /// adds OTP). Default yields just <see cref="NpmAuthToken"/> when
    /// set; override to add more.
    /// </summary>
    protected virtual IEnumerable<Secret> CollectSecrets()
    {
        if (NpmAuthToken is not null) yield return NpmAuthToken;
    }

    internal CommandPlan ToCommandPlan(Tool tool)
    {
        var env = new Dictionary<string, string>(EnvironmentVariables);
        if (NpmAuthToken is { } t) env["NPM_CONFIG_TOKEN"] = t.Reveal();

        return new CommandPlan
        {
            Executable = tool.Executable.Value,
            Arguments = BuildArguments().ToList(),
            Environment = env,
            WorkingDirectory = WorkingDirectory ?? tool.WorkingDirectory,
            Secrets = CollectSecrets().ToList(),
        };
    }
}

/// <summary>Fluent setters for the common knobs — generic so subclass chains stay typed.</summary>
public static class YarnSettingsBaseExtensions
{
    public static T SetWorkingDirectory<T>(this T s, string? cwd) where T : YarnSettingsBase { s.WorkingDirectory = cwd; return s; }
    public static T SetEnvironmentVariable<T>(this T s, string name, string value) where T : YarnSettingsBase { s.EnvironmentVariables[name] = value; return s; }
    public static T SetNpmAuthToken<T>(this T s, Secret token) where T : YarnSettingsBase { s.NpmAuthToken = token; return s; }
}
