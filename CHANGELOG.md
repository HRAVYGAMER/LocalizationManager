# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Fixed
- ResourceFileParser now preserves original file order when writing .resx files
- ResourceFileParser no longer removes XML schema declarations
- Write operations now perform in-place updates, minimizing git diffs

### Added
- Demo GIF showcasing all LRM features
- 5 new comprehensive tests for file preservation (Write_ShouldPreserveOriginalOrder, etc.)
- Demo script (demo.sh) with automatic backup/restore mechanism
- Demo GIF creation guide in CONTRIBUTING.md
- Assets folder for media files

### Changed
- ResourceFileParser.Write() now updates entries in-place instead of recreating all elements

## [0.6.2] - 2025-01-09

### Added
- GitHub Actions CI/CD workflows for automated releases
- Version management scripts (`bump-version.sh` and `get-version.sh`)
- Status badges to README (CI, Release, Version, License, .NET, Platform)
- Repository topics and improved description

### Changed
- Build script now uses dynamic version extraction from `.csproj`
- Removed hardcoded VERSION variable from `build.sh` for safety
- Workflow only modifies `.csproj` and `README.md` (build scripts untouched)

### Fixed
- Build script now explicitly publishes main project only (excludes test project)

## [0.6.0] - 2025-11-09

### Added
- Initial release of Localization Resource Manager
- **CLI Commands:**
  - `validate` - Validate resource files for missing translations, duplicates, and empty values
  - `stats` - Display translation coverage statistics with charts
  - `view` - Display specific key details in table, JSON, or simple format
  - `add` - Add new localization keys to all languages
  - `update` - Modify existing key values with preview
  - `delete` - Remove keys from all languages
  - `export` - Export translations to CSV format
  - `import` - Import translations from CSV with conflict resolution
  - `edit` - Launch interactive TUI editor
- **Interactive TUI Editor:**
  - Real-time search and filter
  - Multi-column table view for all languages
  - Add, edit, delete keys with keyboard shortcuts
  - Automatic validation (F6)
  - Unsaved changes tracking
  - Keyboard shortcuts help (F1)
- **Core Features:**
  - Auto-discovery of `.resx` files and languages
  - Dynamic language support (no hardcoded languages)
  - Language validation with helpful errors
  - Automatic backup system before modifications
  - Multi-platform build support (Linux/Windows x64/ARM64)
  - Shell completion scripts (bash and zsh)
- **Testing:**
  - 21 passing unit and integration tests
  - Test coverage for core logic, CRUD operations, and validation
- **Documentation:**
  - Comprehensive README with usage examples
  - EXAMPLES.md with real-world scenarios
  - INSTALLATION.md with platform-specific instructions
  - BUILDING.md with build and distribution guide
- **Build System:**
  - Automated multi-platform build script (`build.sh`)
  - Self-contained executables (no .NET runtime required)
  - Distribution archives ready for release
- **License:**
  - MIT License with copyright headers in all source files

[Unreleased]: https://github.com/nickprotop/LocalizationManager/compare/v0.6.2...HEAD
[0.6.2]: https://github.com/nickprotop/LocalizationManager/compare/v0.6.0...v0.6.2
[0.6.0]: https://github.com/nickprotop/LocalizationManager/releases/tag/v0.6.0
