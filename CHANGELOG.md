# Changelog

## 0.2.6 (2026-03-24)

- Add unit tests
- Add test step to CI workflow

## 0.2.5 (2026-03-23)

- Shorten package description to meet 120-character limit

## 0.2.4 (2026-03-22)

- Add dates to changelog entries

## 0.2.3 (2026-03-16)

- Add Development section to README
- Add GenerateDocumentationFile and RepositoryType to .csproj

## 0.2.2 (2026-03-16)

## 0.2.0 (2026-03-13)

### Added
- `ignorePaths` parameter for excluding specific paths from comparison (supports wildcards)
- `ConfigChange` record with `OldValue` and `NewValue` on changed entries
- `Compare(Stream, Stream)` overload for stream-based comparison

## 0.1.1 (2026-03-10)

- Fix README path in csproj so README displays on nuget.org

## 0.1.0 (2026-03-10)

- Initial release
- `ConfigChange` record for key-level differences
- `ConfigDiffResult` record with Added, Removed, Changed, and Unchanged lists
- `ConfigComparer.Compare` — diff two JSON strings
- `ConfigComparer.CompareFiles` — diff two JSON files on disk
- JSON flattening to dot-notation with array index support
