# Tamp.Yarn

Yarn Berry CLI wrapper for [Tamp](https://github.com/tamp-build/tamp).

| Package | Yarn | Status |
|---|---|---|
| [`Tamp.Yarn.V4`](src/Tamp.Yarn.V4) | 4.x (Berry) | preview |

Requires `Tamp.Core ≥ 1.0.3`. Auth via `Secret` (`NPM_CONFIG_TOKEN`
env var on the spawned process); OTP for 2FA is a `Secret` CLI arg.

## Why a separate repo

Yarn Berry ships every few weeks and the major has bumped in
recent memory (3 → 4 in 2024). Per the satellite-repo convention,
this tracks Berry's release cadence independently of `tamp` core.

## Install

In your build script's `Directory.Packages.props`:

```xml
<PackageVersion Include="Tamp.Yarn.V4" Version="0.1.0" />
```

In `build/Build.csproj`:

```xml
<PackageReference Include="Tamp.Yarn.V4" />
```

## Sub-facades

| Sub-facade | Verbs |
|---|---|
| `Yarn` | `Install`, `Run`, `Dlx`, `Pack`, `Dedupe`, `Exec`, `Raw` (escape hatch) |
| `YarnWorkspaces` | `List`, `Foreach`, `Focus` |
| `YarnNpm` | `Publish`, `TagAdd`, `TagRemove`, `Whoami` |

## Quick example — HoldFast-shaped frontend build

```csharp
using Tamp;
using Tamp.Yarn.V4;

class Build : TampBuild
{
    public static int Main(string[] args) => Execute<Build>(args);

    [NuGetPackage("yarn", UseSystemPath = true)]
    readonly Tool Yarn = null!;

    [Secret("npm auth token", EnvironmentVariable = "NPM_CONFIG_TOKEN")]
    readonly Secret NpmAuthToken = null!;

    AbsolutePath Frontend => RootDirectory / "src" / "frontend";

    Target FrontendInstall => _ => _
        .Executes(() => Tamp.Yarn.V4.Yarn.Install(Yarn, s => s
            .SetWorkingDirectory(Frontend)
            .SetImmutable()));

    Target FrontendBuild => _ => _
        .DependsOn(nameof(FrontendInstall))
        .Executes(() => YarnWorkspaces.Foreach(Yarn, s => s
            .SetWorkingDirectory(Frontend)
            .SetFrom("@holdfast-io/frontend")
            .SetRecursive()
            .SetTopological()
            .SetParallel()
            .SetJobs(4)
            .SetCommand("run")
            .AddCommandArg("build:fast")));

    Target PublishBrowserSdk => _ => _
        .Requires(() => NpmAuthToken != null)
        .Executes(() => YarnNpm.Publish(Yarn, s => s
            .SetWorkingDirectory(RootDirectory / "sdk" / "client" / "browser")
            .SetTag("latest")
            .SetAccess(YarnNpmAccess.Public)
            .SetNpmAuthToken(NpmAuthToken)
            .SetTolerateRepublish()));
}
```

## Auth design

`SetNpmAuthToken(Secret)` registers the token with the runner's
redaction table AND sets it as `NPM_CONFIG_TOKEN` on the spawned
process — Yarn reads it from env. The token never lands on the
command line, so it stays out of the OS process table.

For 2FA-protected publishes, `YarnNpmPublishSettings.SetOtp(Secret)`
takes the one-time code as a `Secret` CLI argument (Yarn's
`--otp <code>` flag — there's no env-var path for OTP). The OTP is
registered with redaction and revealed only at spawn time.

## See also

- [tamp](https://github.com/tamp-build/tamp) — the core framework
- [Yarn Berry docs](https://yarnpkg.com/) — CLI reference, configuration

## License

[MIT](LICENSE) — same as `tamp` core. (Yarn itself is BSD-2-Clause.)
