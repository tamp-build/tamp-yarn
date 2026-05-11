namespace Tamp.Yarn.V4;

/// <summary>npm package access for <c>yarn npm publish</c>.</summary>
public enum YarnNpmAccess
{
    Public,
    Restricted,
}

/// <summary>Settings for <c>yarn npm publish</c>.</summary>
public sealed class YarnNpmPublishSettings : YarnSettingsBase
{
    /// <summary>Dist-tag for the published version. Maps to <c>--tag</c>. Defaults to <c>latest</c>.</summary>
    public string? Tag { get; set; }

    /// <summary>Access level. Maps to <c>--access</c>.</summary>
    public YarnNpmAccess? Access { get; set; }

    /// <summary>OTP for 2FA-protected accounts. Maps to <c>--otp</c>. Pass as <see cref="Secret"/>.</summary>
    public Secret? Otp { get; set; }

    /// <summary>Tolerate duplicate-version errors from the registry (CI re-run safety).</summary>
    public bool TolerateRepublish { get; set; }

    public YarnNpmPublishSettings SetTag(string? tag) { Tag = tag; return this; }
    public YarnNpmPublishSettings SetAccess(YarnNpmAccess access) { Access = access; return this; }
    public YarnNpmPublishSettings SetOtp(Secret otp) { Otp = otp; return this; }
    public YarnNpmPublishSettings SetTolerateRepublish(bool v = true) { TolerateRepublish = v; return this; }

    protected override IEnumerable<string> BuildArguments()
    {
        yield return "npm";
        yield return "publish";
        if (!string.IsNullOrEmpty(Tag)) { yield return "--tag"; yield return Tag!; }
        if (Access is { } a)
        {
            yield return "--access";
            yield return a switch
            {
                YarnNpmAccess.Public => "public",
                YarnNpmAccess.Restricted => "restricted",
                _ => throw new ArgumentOutOfRangeException(nameof(a), a, "Unknown access."),
            };
        }
        if (Otp is { } otp) { yield return "--otp"; yield return otp.Reveal(); }
        if (TolerateRepublish) yield return "--tolerate-republish";
    }

    protected override IEnumerable<Secret> CollectSecrets()
    {
        foreach (var s in base.CollectSecrets()) yield return s;
        if (Otp is not null) yield return Otp;
    }
}

/// <summary>Settings for <c>yarn npm tag add &lt;package@version&gt; &lt;tag&gt;</c>.</summary>
public sealed class YarnNpmTagAddSettings : YarnSettingsBase
{
    /// <summary>Package + version (e.g. <c>@scope/pkg@1.2.3</c>). Required.</summary>
    public string? PackageVersion { get; set; }

    /// <summary>Tag name (e.g. <c>latest</c>, <c>next</c>, <c>beta</c>). Required.</summary>
    public string? Tag { get; set; }

    public YarnNpmTagAddSettings SetPackageVersion(string? pkgver) { PackageVersion = pkgver; return this; }
    public YarnNpmTagAddSettings SetTag(string? tag) { Tag = tag; return this; }

    protected override IEnumerable<string> BuildArguments()
    {
        if (string.IsNullOrEmpty(PackageVersion)) throw new InvalidOperationException("yarn npm tag add: PackageVersion is required.");
        if (string.IsNullOrEmpty(Tag)) throw new InvalidOperationException("yarn npm tag add: Tag is required.");
        yield return "npm";
        yield return "tag";
        yield return "add";
        yield return PackageVersion!;
        yield return Tag!;
    }
}

/// <summary>Settings for <c>yarn npm tag remove &lt;package&gt; &lt;tag&gt;</c>.</summary>
public sealed class YarnNpmTagRemoveSettings : YarnSettingsBase
{
    /// <summary>Package name (no version). Required.</summary>
    public string? Package { get; set; }

    /// <summary>Tag name to remove. Required.</summary>
    public string? Tag { get; set; }

    public YarnNpmTagRemoveSettings SetPackage(string? pkg) { Package = pkg; return this; }
    public YarnNpmTagRemoveSettings SetTag(string? tag) { Tag = tag; return this; }

    protected override IEnumerable<string> BuildArguments()
    {
        if (string.IsNullOrEmpty(Package)) throw new InvalidOperationException("yarn npm tag remove: Package is required.");
        if (string.IsNullOrEmpty(Tag)) throw new InvalidOperationException("yarn npm tag remove: Tag is required.");
        yield return "npm";
        yield return "tag";
        yield return "remove";
        yield return Package!;
        yield return Tag!;
    }
}

/// <summary>Settings for <c>yarn npm whoami</c>.</summary>
public sealed class YarnNpmWhoamiSettings : YarnSettingsBase
{
    /// <summary>Scope to query (<c>--scope</c>).</summary>
    public string? Scope { get; set; }

    /// <summary>Registry URL override (<c>--publish</c> for the publish-config registry).</summary>
    public bool Publish { get; set; }

    public YarnNpmWhoamiSettings SetScope(string? scope) { Scope = scope; return this; }
    public YarnNpmWhoamiSettings SetPublish(bool v = true) { Publish = v; return this; }

    protected override IEnumerable<string> BuildArguments()
    {
        yield return "npm";
        yield return "whoami";
        if (!string.IsNullOrEmpty(Scope)) { yield return "--scope"; yield return Scope!; }
        if (Publish) yield return "--publish";
    }
}
