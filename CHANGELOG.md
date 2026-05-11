# Changelog

All notable changes to **Tamp.Yarn** are recorded here.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/);
versions follow [SemVer](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.1] - 2026-05-11

### Added

- Object-init overloads on every Yarn wrapper (TAM-161 satellite fanout).

### Fixed

- Collapsed duplicate `<Version>` element in `Directory.Build.props` (TAM-81 canonical entry was being shadowed by a trailing `0.0.x-alpha` block under MSBuild last-wins). Single source of truth restored.
