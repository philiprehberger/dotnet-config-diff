# Changelog

## 0.1.0 (2026-03-10)

- Initial release
- `ConfigChange` record for key-level differences
- `ConfigDiffResult` record with Added, Removed, Changed, and Unchanged lists
- `ConfigComparer.Compare` — diff two JSON strings
- `ConfigComparer.CompareFiles` — diff two JSON files on disk
- JSON flattening to dot-notation with array index support
