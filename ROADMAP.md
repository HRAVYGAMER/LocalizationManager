# LocalizationManager v0.7.0 - Development Roadmap

**Target Release:** v0.7.0
**Estimated Timeline:** 12 weeks
**Start Date:** 2025-01-15
**Architecture:** ASP.NET Core Web API + Blazor WebAssembly

---

## 🎯 Major Features

### 1. Variable/Placeholder Validation
**Status:** ✅ **COMPLETED**
**Priority:** High
**Description:** Detect and validate format strings ({0}, %s, etc.) are preserved in translations

- [x] PlaceholderDetector implementation
- [x] PlaceholderValidator implementation
- [x] Integration with ResourceValidator
- [x] CLI command support (validate command)
- [x] TUI integration (F6)
- [x] Unit tests (PlaceholderDetectorTests, PlaceholderValidatorTests)
- [x] Integration tests
- [x] Documentation

---

### 2. Enhanced Backup System with Versioning + Diff View
**Status:** ✅ **COMPLETED** (Basic Implementation)
**Priority:** High
**Description:** Version history with automatic rotation and visual diff comparison

**Implemented:**
- [x] BackupVersionManager with simple rotation (keeps last 10 versions)
- [x] Manifest system (JSON metadata)
- [x] BackupDiffService (compare versions)
- [x] BackupDiffFormatter (text/JSON/HTML)
- [x] BackupRestoreService with preview
- [x] CLI backup commands (list, create, restore, diff, info, prune)
- [x] TUI Backup Manager (F7)
- [x] TUI Diff Viewer window
- [x] Unit tests
- [x] Integration tests
- [x] Documentation

**Not Implemented (Future Enhancement):**
- [ ] Smart rotation with configurable retention policies
- [ ] Backup configuration (currently hardcoded: 10 versions max)
- [ ] API endpoints
- [ ] Blazor BackupHistory page

---

### 3. Web API
**Status:** Not Started
**Priority:** High
**Description:** ASP.NET Core Web API backend for resource management

- [ ] Create project structure
- [ ] ResourcesController
- [ ] ValidationController
- [ ] TranslationController
- [ ] BackupController
- [ ] StatsController
- [ ] TranslationProgressHub (SignalR)
- [ ] ValidationHub (SignalR)
- [ ] CORS configuration
- [ ] Swagger/OpenAPI setup
- [ ] Middleware (exception, logging)
- [ ] API integration tests

---

### 4. Blazor WASM UI
**Status:** Not Started
**Priority:** High
**Description:** Browser-based editor with full feature parity

**Blazor WASM (LocalizationManager.Web):**
- [ ] Create project structure
- [ ] Index.razor (Dashboard)
- [ ] Editor.razor (Main editor)
- [ ] Validation.razor
- [ ] Translation.razor
- [ ] BackupHistory.razor
- [ ] Settings.razor

**Shared Components:**
- [ ] ResourceGrid.razor
- [ ] KeyEditor.razor
- [ ] SearchBar.razor
- [ ] LanguagePicker.razor
- [ ] ProgressIndicator.razor
- [ ] DiffViewer.razor
- [ ] Timeline.razor

**API Clients:**
- [ ] ResourceApiClient
- [ ] ValidationApiClient
- [ ] TranslationApiClient
- [ ] BackupApiClient
- [ ] SignalR integration

**Other:**
- [ ] CLI web command
- [ ] Responsive design
- [ ] Component tests (bUnit)
- [ ] E2E tests
- [ ] Documentation

---

### 5. Simple CLI Chaining
**Status:** Not Started
**Priority:** Low
**Description:** Run multiple LRM commands sequentially in one invocation

- [ ] ChainCommand implementation
- [ ] Full argument support for each step
- [ ] Command parsing (double-dash separator, complex args)
- [ ] Progress display
- [ ] `--stop-on-error` flag (default: true)
- [ ] `--continue-on-error` flag
- [ ] `--dry-run` support
- [ ] Exit code propagation
- [ ] Shell completion
- [ ] Unit tests
- [ ] Integration tests
- [ ] Documentation

**Examples:**
- `lrm chain validate --format json -- translate --only-missing -- export -o output.csv`
- `lrm chain validate -- scan -- backup create`
- `lrm chain import file.csv -- validate -- translate --provider google -- export`

---

### 6. Debian Package Distribution (.deb + PPA)
**Status:** Not Started
**Priority:** High
**Description:** Native Debian/Ubuntu package distribution via apt and PPA

**Package Variants:**
- [ ] `lrm` - Framework-dependent package (~200KB, requires dotnet-runtime-9.0)
- [ ] `lrm-standalone` - Self-contained package (~72MB, no dependencies)

**Debian Packaging:**
- [ ] Create debian/ directory structure
- [ ] debian/control (package metadata for both variants)
- [ ] debian/changelog (Debian-format changelog)
- [ ] debian/rules (build script)
- [ ] debian/install files (lrm.install, lrm-standalone.install)
- [ ] debian/copyright (MIT license in machine-readable format)
- [ ] Man page (docs/lrm.1)
- [ ] Shell completion integration

**Build Infrastructure:**
- [ ] build-deb.sh script (build .deb packages)
- [ ] build-source-package.sh (create source packages for PPA)
- [ ] Update build.sh with --deb flag
- [ ] GitHub Actions integration (.deb build on release)
- [ ] Upload .deb files to GitHub Releases

**PPA Distribution:**
- [ ] Launchpad PPA setup (ppa:nikolaos-protopapas/lrm)
- [ ] GPG key generation and configuration
- [ ] Source package building
- [ ] Automated PPA uploads (optional GitHub Actions workflow)

**Documentation:**
- [ ] Update docs/INSTALLATION.md (apt installation methods)
- [ ] Create docs/PACKAGING.md (maintainer guide)
- [ ] Update README.md (apt as primary installation method)

**Testing:**
- [ ] Test .deb installation (both variants)
- [ ] Test PPA installation on Ubuntu
- [ ] Test on Debian-based distros
- [ ] Verify man page and completions

**Installation Examples:**
- `sudo apt install ./lrm_0.6.12_amd64.deb` (GitHub download)
- `sudo add-apt-repository ppa:nikolaos-protopapas/lrm && sudo apt install lrm` (PPA)
- `sudo apt install lrm-standalone` (self-contained variant)

---

## 📅 Implementation Phases

### Phase 1: Foundation & Backup System (Week 1-2)
**Status:** ✅ **COMPLETED**
**Dates:** Started 2025-01-15, Completed 2025-01-16

- [x] Create LocalizationManager.Shared project
  - [x] Create project structure
  - [x] Add shared DTOs (ResourceDTO, ValidationResultDTO, etc.)
  - [x] Add shared models
  - [x] Add shared enums

- [x] Implement Enhanced Backup System
  - [x] Create LocalizationManager.Core/Backup/ directory
  - [x] Implement BackupVersionManager.cs
    - [x] Backup creation with metadata
    - [x] Manifest.json management
    - [x] Version numbering
    - [x] Simple rotation (keeps last 10 versions)
    - [x] Automatic cleanup of old backups
  - [ ] ~~Implement BackupRotationPolicy.cs~~ (code exists but not used)
    - [ ] ~~Smart rotation algorithm~~ (not implemented)
    - [ ] ~~Configurable retention rules~~ (not implemented)
  - [x] Implement BackupDiffService.cs
    - [x] Compare two backup versions
    - [x] Compare current vs backup
    - [x] Generate structured diff
    - [x] Preview restore (shows current → backup diff)
  - [x] Implement BackupDiffFormatter.cs
    - [x] Text format output
    - [x] JSON format output
    - [x] HTML format output
    - [x] Console display with Spectre.Console
  - [x] Implement BackupRestoreService.cs
    - [x] Full restore
    - [x] Partial restore (selective keys)
    - [x] Preview before restore
    - [x] Validation

- [x] CLI Integration
  - [x] Create Commands/Backup/ directory
  - [x] Implement BackupListCommand.cs
  - [x] Implement BackupCreateCommand.cs
  - [x] Implement BackupRestoreCommand.cs
  - [x] Implement BackupDiffCommand.cs
  - [x] Implement BackupInfoCommand.cs
  - [x] Implement BackupPruneCommand.cs
  - [x] Register backup branch in Program.cs
  - [x] Update shell completions (_lrm, lrm-completion.bash)

- [x] TUI Integration
  - [x] Add F7 keyboard shortcut
  - [x] Implement basic backup list view (integrated in ResourceEditorWindow)
  - [x] Update all TUI operations to use BackupVersionManager
  - [x] Create dedicated UI/BackupManagerWindow.cs (enhanced version)
  - [x] Implement diff viewer (UI/BackupDiffWindow.cs)
  - [x] Add restore functionality in TUI (full restore with preview)
  - [x] Interactive backup management (list, restore, diff, delete, prune)

- [ ] ~~Configuration~~ (not implemented - backups controlled by --no-backup flag only)
  - [ ] ~~Add backup section to lrm.json schema~~ (removed - not used)
  - [ ] ~~Update Configuration classes~~ (removed - not used)

- [x] Testing
  - [x] BackupVersionManagerTests.cs (comprehensive unit tests)
  - [x] BackupRotationPolicyTests.cs (all rotation scenarios)
  - [x] BackupDiffServiceTests.cs (comparison logic)
  - [x] BackupRestoreServiceTests.cs (restore and preview)
  - [x] Integration tests (BackupSystemIntegrationTests.cs)
  - [x] All 41 backup tests passing (100% pass rate)
  - [x] Full test suite: 408 tests passing

- [x] Documentation
  - [x] Create docs/BACKUP.md (comprehensive 500+ line guide)
  - [x] Update README.md (features section + documentation table)
  - [x] Update COMMANDS.md (complete backup command reference)

---

### Phase 2: Variable Validation (Week 2-3)
**Status:** ✅ **COMPLETED**
**Dates:** Started 2025-01-16, Completed 2025-01-16

- [x] Create LocalizationManager.Core/Validation/ directory structure

- [x] Implement PlaceholderDetector.cs
  - [x] Regex patterns for .NET format strings ({0}, {name})
  - [x] Regex patterns for printf-style (%s, %d, %1$s)
  - [x] Regex patterns for ICU MessageFormat
  - [x] Regex patterns for template literals (${var})
  - [x] Pattern detection method
  - [x] Handle escaped characters (printf %%)

- [x] Implement PlaceholderValidator.cs
  - [x] Compare source vs translation placeholders
  - [x] Validate placeholder count matches
  - [x] Validate placeholder types match
  - [x] Validate placeholder order (positional)
  - [x] Generate validation errors
  - [x] GetSummary() method
  - [x] ValidateBatch() method

- [x] Integration
  - [x] Update ResourceValidator.cs with placeholder validation
  - [x] Add PlaceholderMismatches to ValidationResult model
  - [x] Update validate command (enabled by default - no flag needed)
  - [x] Update ValidateCommand display methods (Table, JSON, Simple)
  - [x] Update TUI validation panel (ResourceEditorWindow)

- [x] Testing
  - [x] PlaceholderDetectorTests.cs (all 4 placeholder formats)
    - [x] .NET format strings tests
    - [x] Printf-style tests
    - [x] ICU MessageFormat tests
    - [x] Template literal tests
    - [x] Mixed placeholder tests
    - [x] GetNormalizedIdentifier tests
  - [x] PlaceholderValidatorTests.cs
    - [x] Valid placeholder tests
    - [x] Missing placeholder tests
    - [x] Extra placeholder tests
    - [x] Type mismatch tests
    - [x] Count mismatch tests
    - [x] Batch validation tests
  - [x] Integration tests with real .resx files

- [x] Documentation
  - [x] Create docs/PLACEHOLDERS.md (comprehensive guide)
  - [x] Add examples
  - [x] Update README.md (features section + documentation table)

---

### Phase 3: Debian Package Distribution (Week 3)
**Status:** Not Started
**Dates:** TBD

- [ ] Create debian/ Directory Structure
  - [ ] debian/control - Package metadata for both variants
    - [ ] Package: lrm (framework-dependent)
    - [ ] Package: lrm-standalone (self-contained)
    - [ ] Dependencies: dotnet-runtime-9.0 (for lrm only)
    - [ ] Architecture: amd64, arm64
    - [ ] Maintainer, description, homepage
  - [ ] debian/changelog - Debian-format changelog
    - [ ] Convert git history to Debian changelog format
    - [ ] Follow Debian versioning (0.6.12-1)
  - [ ] debian/rules - Build script using dh
    - [ ] Framework-dependent build target
    - [ ] Self-contained build target
    - [ ] Clean, build, install targets
  - [ ] debian/install - File mappings
    - [ ] lrm.install - Framework-dependent file list
    - [ ] lrm-standalone.install - Self-contained file list
  - [ ] debian/copyright - Machine-readable MIT license
  - [ ] debian/compat - Debhelper compatibility level

- [ ] Create Man Page
  - [ ] docs/lrm.1 - Manual page in man format
    - [ ] NAME, SYNOPSIS, DESCRIPTION sections
    - [ ] COMMANDS section (all lrm commands)
    - [ ] OPTIONS section (global options)
    - [ ] EXAMPLES section (common workflows)
    - [ ] FILES section (config locations)
    - [ ] SEE ALSO, AUTHOR, COPYRIGHT sections
  - [ ] Test man page rendering: `man ./docs/lrm.1`
  - [ ] Include in debian/install for both packages

- [ ] Shell Completion Integration
  - [ ] Move completions to debian/bash-completion/lrm
  - [ ] Move completions to debian/zsh-completion/_lrm
  - [ ] Update debian/install to install to system locations
    - [ ] /usr/share/bash-completion/completions/lrm
    - [ ] /usr/share/zsh/site-functions/_lrm

- [ ] Build Scripts
  - [ ] Create build-deb.sh
    - [ ] Parse version from .csproj
    - [ ] Build framework-dependent variant (dotnet publish without --self-contained)
    - [ ] Build self-contained variant (existing approach)
    - [ ] Create debian packages with dpkg-deb
    - [ ] Build for both amd64 and arm64
    - [ ] Output to publish/lrm_VERSION_ARCH.deb
  - [ ] Create build-source-package.sh
    - [ ] Create .orig.tar.gz from source
    - [ ] Create .debian.tar.xz from debian/ directory
    - [ ] Generate .dsc and .changes files
    - [ ] Sign with GPG key (debsign)
    - [ ] Ready for dput upload to PPA
  - [ ] Update build.sh
    - [ ] Add --deb flag to build .deb packages
    - [ ] Add --source flag to build source packages

- [ ] GitHub Actions Integration
  - [ ] Update .github/workflows/release.yml
    - [ ] Install dpkg-dev, debhelper dependencies
    - [ ] Run build-deb.sh for all architectures
    - [ ] Upload 4 .deb files to GitHub Release:
      - [ ] lrm_VERSION_amd64.deb
      - [ ] lrm_VERSION_arm64.deb
      - [ ] lrm-standalone_VERSION_amd64.deb
      - [ ] lrm-standalone_VERSION_arm64.deb

- [ ] PPA Setup (Manual Configuration)
  - [ ] Create Launchpad account
  - [ ] Generate GPG key for package signing
    - [ ] gpg --full-generate-key
    - [ ] Upload public key to Launchpad
    - [ ] Upload to Ubuntu keyserver
  - [ ] Create PPA: ppa:nikolaos-protopapas/lrm
  - [ ] Configure dput for PPA uploads
  - [ ] Document PPA setup process

- [ ] PPA Workflow (Optional Automation)
  - [ ] Create .github/workflows/ppa-release.yml
    - [ ] Trigger on version tags
    - [ ] Build source package
    - [ ] Sign with GPG key (from GitHub secrets)
    - [ ] Upload to Launchpad PPA with dput
  - [ ] Add GPG_PRIVATE_KEY to GitHub secrets
  - [ ] Test automated PPA upload

- [ ] Documentation
  - [ ] Update docs/INSTALLATION.md
    - [ ] Add "Installation via APT" section at top
    - [ ] GitHub .deb download method
    - [ ] PPA installation method
    - [ ] Explain package variants (lrm vs lrm-standalone)
    - [ ] Update existing manual installation sections
  - [ ] Create docs/PACKAGING.md
    - [ ] Debian packaging overview
    - [ ] Building .deb packages locally
    - [ ] PPA upload process
    - [ ] Maintainer release checklist
    - [ ] Troubleshooting common issues
  - [ ] Update README.md
    - [ ] Add apt installation as primary method
    - [ ] Add PPA instructions
    - [ ] Update installation section
    - [ ] Add .deb to distribution badges/shields
  - [ ] Update COMMANDS.md
    - [ ] Reference man page
    - [ ] Link to `man lrm` for full documentation

- [ ] Testing
  - [ ] Test framework-dependent package
    - [ ] Build on Ubuntu 24.04
    - [ ] Install: `sudo apt install ./lrm_VERSION_amd64.deb`
    - [ ] Verify dotnet-runtime-9.0 dependency check
    - [ ] Test binary at /usr/bin/lrm
    - [ ] Test man page: `man lrm`
    - [ ] Test bash completion
    - [ ] Test zsh completion
    - [ ] Uninstall: `sudo apt remove lrm`
  - [ ] Test self-contained package
    - [ ] Build on Ubuntu 24.04
    - [ ] Install: `sudo apt install ./lrm-standalone_VERSION_amd64.deb`
    - [ ] Verify no dependencies required
    - [ ] Test binary works without .NET runtime
    - [ ] Test all commands work correctly
  - [ ] Test PPA installation
    - [ ] Upload to test PPA
    - [ ] Install on clean Ubuntu VM
    - [ ] Test: `sudo add-apt-repository ppa:user/lrm-test`
    - [ ] Test: `sudo apt update && sudo apt install lrm`
    - [ ] Test package updates work correctly
  - [ ] Test on Debian
    - [ ] Test .deb installation on Debian 12
    - [ ] Verify compatibility
  - [ ] Test ARM64 packages
    - [ ] Build ARM64 .deb
    - [ ] Test on ARM64 system (if available) or skip

---

### Phase 4: Simple CLI Chaining (Week 4)
**Status:** Not Started
**Dates:** TBD

- [ ] Create Commands/ChainCommand.cs
  - [ ] Command argument parsing (double-dash separator)
  - [ ] Split chain by " -- " separator into individual commands
  - [ ] Parse each command into command name + arguments
  - [ ] Support for commands with arguments (e.g., validate --format json)
  - [ ] Support for commands without arguments (e.g., validate)
  - [ ] Support for complex arguments with flags and options

- [ ] Implement Command Execution Engine
  - [ ] Create CommandApp instance programmatically
  - [ ] Execute each command in sequence
  - [ ] Capture exit codes from each command
  - [ ] Pass context between commands

- [ ] Error Handling Modes
  - [ ] Implement --stop-on-error (default behavior)
  - [ ] Implement --continue-on-error flag
  - [ ] Error reporting and logging
  - [ ] Exit code propagation to shell

- [ ] Progress Display
  - [ ] Step-by-step progress UI using Spectre.Console
  - [ ] Show current command being executed
  - [ ] Show completed/pending/failed steps
  - [ ] Summary at completion

- [ ] Dry Run Support
  - [ ] Implement --dry-run flag
  - [ ] Display commands that would be executed
  - [ ] No actual command execution in dry-run mode

- [ ] Shell Integration
  - [ ] Update lrm-completion.bash with chain command
  - [ ] Update _lrm (zsh) with chain command
  - [ ] Add command examples to completions

- [ ] Testing
  - [ ] Create ChainCommandTests.cs
  - [ ] Test command parsing (double-dash separator)
  - [ ] Test commands with and without arguments
  - [ ] Test complex argument parsing (flags, options, values)
  - [ ] Test error handling modes
  - [ ] Test exit code propagation
  - [ ] Integration tests (full chain execution)
  - [ ] Test dry-run mode

- [ ] Documentation
  - [ ] Add chain command to COMMANDS.md
  - [ ] Add examples to README.md
  - [ ] Common workflow examples:
    - [ ] Translation pipeline: `lrm chain import translations.csv -- validate -- translate --only-missing --provider google -- export -o output.csv`
    - [ ] Validation workflow: `lrm chain validate --format json -- scan --strict`
    - [ ] Backup workflow: `lrm chain backup create -- update SaveButton --lang default:"Save" -- validate`
  - [ ] Add help text to command

---

### Phase 5: Web API (Week 5-6)
**Status:** Not Started
**Dates:** TBD

- [ ] Create LocalizationManager.Api Project
  - [ ] ASP.NET Core Web API project
  - [ ] Project structure
  - [ ] appsettings.json
  - [ ] Program.cs setup

- [ ] Implement Controllers
  - [ ] ResourcesController.cs
    - [ ] GET /api/resources (list)
    - [ ] GET /api/resources/{fileName}
    - [ ] GET /api/resources/{fileName}/keys
    - [ ] POST /api/resources
    - [ ] PUT /api/resources/{fileName}/keys/{keyName}
    - [ ] DELETE /api/resources/{fileName}/keys/{keyName}
  - [ ] ValidationController.cs
    - [ ] POST /api/validation/validate
    - [ ] POST /api/validation/placeholders
    - [ ] GET /api/validation/rules
  - [ ] TranslationController.cs
    - [ ] POST /api/translation/translate
    - [ ] GET /api/translation/providers
    - [ ] POST /api/translation/estimate
  - [ ] BackupController.cs
    - [ ] GET /api/backups/{fileName}
    - [ ] POST /api/backups/{fileName}/create
    - [ ] GET /api/backups/{fileName}/diff
    - [ ] POST /api/backups/{fileName}/restore
  - [ ] StatsController.cs
    - [ ] GET /api/stats

- [ ] Implement SignalR Hubs
  - [ ] TranslationProgressHub.cs
  - [ ] ValidationHub.cs

- [ ] Middleware & Configuration
  - [ ] CORS configuration
  - [ ] Exception handling middleware
  - [ ] Logging middleware
  - [ ] Swagger/OpenAPI setup

- [ ] Service Registration
  - [ ] Register Core services
  - [ ] DI configuration

- [ ] Testing
  - [ ] Controller tests
  - [ ] Integration tests
  - [ ] API endpoint tests

- [ ] Documentation
  - [ ] Swagger annotations
  - [ ] API documentation

---

### Phase 6: Blazor WASM UI (Week 7-10)
**Status:** Not Started
**Dates:** TBD

- [ ] Create LocalizationManager.Web Project
  - [ ] Blazor WebAssembly project
  - [ ] Project structure
  - [ ] Program.cs setup
  - [ ] wwwroot/index.html

- [ ] Implement API Clients
  - [ ] Create Services/ directory
  - [ ] ResourceApiClient.cs
  - [ ] ValidationApiClient.cs
  - [ ] TranslationApiClient.cs
  - [ ] BackupApiClient.cs
  - [ ] HttpClient configuration

- [ ] Implement Core Pages
  - [ ] Index.razor (Dashboard)
    - [ ] Recent files widget
    - [ ] Quick stats
    - [ ] Recent activity
    - [ ] Quick actions
  - [ ] Editor.razor (Main editor)
    - [ ] Multi-column grid
    - [ ] Inline editing
    - [ ] Real-time search
    - [ ] Bulk operations
    - [ ] Context menu
    - [ ] Keyboard shortcuts

- [ ] Implement Feature Pages
  - [ ] Validation.razor
    - [ ] Validation results display
    - [ ] Rule filtering
    - [ ] Fix suggestions
  - [ ] Translation.razor
    - [ ] Provider selection
    - [ ] Language picker
    - [ ] Progress tracking (SignalR)
    - [ ] Cost estimation
  - [ ] BackupHistory.razor
    - [ ] Timeline visualization
    - [ ] Diff viewer
    - [ ] Restore wizard
  - [ ] Settings.razor
    - [ ] API keys
    - [ ] Default settings
    - [ ] Theme selection

- [ ] Implement Shared Components
  - [ ] Components/ResourceGrid.razor
  - [ ] Components/KeyEditor.razor
  - [ ] Components/SearchBar.razor
  - [ ] Components/LanguagePicker.razor
  - [ ] Components/ProgressIndicator.razor
  - [ ] Components/DiffViewer.razor
  - [ ] Components/Timeline.razor

- [ ] SignalR Integration
  - [ ] HubConnection setup
  - [ ] Real-time updates
  - [ ] Auto-reconnect

- [ ] Styling & UX
  - [ ] CSS/SCSS styling
  - [ ] Responsive design
  - [ ] Loading states
  - [ ] Error handling
  - [ ] Accessibility

- [ ] Testing
  - [ ] Component tests (bUnit)
  - [ ] Integration tests

- [ ] Documentation
  - [ ] Create docs/WEB-UI.md
  - [ ] User guide
  - [ ] Screenshots

---

### Phase 7: Integration & Polish (Week 11)
**Status:** Not Started
**Dates:** TBD

- [ ] CLI web Command
  - [ ] Create Commands/WebCommand.cs
  - [ ] Start API server
  - [ ] Serve Blazor WASM
  - [ ] Configuration options (port, bind, etc.)

- [ ] End-to-End Testing
  - [ ] Full workflow tests
  - [ ] CLI to API integration
  - [ ] API to Blazor integration

- [ ] Performance Testing
  - [ ] Large file handling (10k+ keys)
  - [ ] Concurrent users (API)
  - [ ] Memory profiling
  - [ ] Optimization

- [ ] Security Audit
  - [ ] API authentication (optional)
  - [ ] Input validation
  - [ ] XSS/CSRF protection

- [ ] Documentation
  - [ ] Update README.md
  - [ ] Update all docs/
  - [ ] Create migration guide
  - [ ] API examples

- [ ] Polish
  - [ ] Error messages
  - [ ] Help text
  - [ ] CLI output formatting
  - [ ] TUI improvements
  - [ ] Web UI improvements

---

### Phase 8: Release (Week 12)
**Status:** Not Started
**Dates:** TBD

- [ ] Version Management
  - [ ] Bump version to 0.7.0 in all projects
  - [ ] Update AssemblyInfo

- [ ] Changelog
  - [ ] Update CHANGELOG.md
  - [ ] Document all new features
  - [ ] Document breaking changes
  - [ ] Migration notes

- [ ] Release Notes
  - [ ] Write comprehensive release notes
  - [ ] Feature highlights
  - [ ] Screenshots/GIFs
  - [ ] Upgrade instructions

- [ ] Build & Package
  - [ ] Update build.sh
  - [ ] Build all platforms:
    - [ ] Linux x64
    - [ ] Linux ARM64
    - [ ] Windows x64
    - [ ] Windows ARM64
  - [ ] Package API project
  - [ ] Package Web project
  - [ ] Create release archives

- [ ] Testing
  - [ ] Final smoke tests on all platforms
  - [ ] Installation tests
  - [ ] Upgrade tests

- [ ] GitHub Release
  - [ ] Create release tag (v0.7.0)
  - [ ] Upload binaries
  - [ ] Publish release notes

- [ ] Documentation
  - [ ] Update online documentation
  - [ ] Update examples
  - [ ] Update CLI help

- [ ] Announcement
  - [ ] GitHub announcement
  - [ ] Social media
  - [ ] Community notification

---

## 🔧 Technical Decisions

### Architecture
- **API Backend:** ASP.NET Core Web API (RESTful + SignalR)
- **Frontend:** Blazor WebAssembly (C# full-stack)
- **Backup Storage:** File-based with JSON manifests
- **Flow Execution:** In-memory pipeline

### Dependencies
- ✅ Microsoft.AspNetCore.SignalR.Client (real-time)
- ✅ Swashbuckle.AspNetCore (Swagger)
- ✅ bUnit (Blazor testing)

### Future Considerations
- 🔮 Plugin system (multi-format support) - deferred to v0.8.0+
- 🔮 Translation Memory - deferred
- 🔮 Mobile apps - deferred

---

## 📊 Progress Tracking

**Overall Progress:** ~25% (2/8 phases completed)

### Feature Completion
- [x] Variable/Placeholder Validation (100% ✅ - Complete: Core + CLI + TUI + Tests + Docs)
- [x] Enhanced Backup System + Diff View (100% ✅ - Complete: Core + CLI + TUI + Tests + Docs)
- [ ] Debian Package Distribution (.deb + PPA) (0%)
- [ ] Simple CLI Chaining (0%)
- [ ] Web API (0%)
- [ ] Blazor WASM UI (0%)

### Phase Completion
- [x] Phase 1: Foundation & Backup System (100% ✅ - COMPLETED 2025-01-16)
- [x] Phase 2: Variable Validation (100% ✅ - COMPLETED 2025-01-16)
- [ ] Phase 3: Debian Package Distribution (0%)
- [ ] Phase 4: Simple CLI Chaining (0%)
- [ ] Phase 5: Web API (0%)
- [ ] Phase 6: Blazor WASM UI (0%)
- [ ] Phase 7: Integration & Polish (0%)
- [ ] Phase 8: Release (0%)

---

## 📝 Notes & Blockers

### Current Blockers
None

### Important Decisions Made
1. Diff view integrated with backup system (not git-based)
2. Web UI will be Blazor WASM + ASP.NET Core API
3. Simple CLI chaining for command automation (lightweight alternative to complex flow system)
4. Plugin system deferred to future release (v0.8.0+)
5. Full flow system deferred to future release (v0.8.0+ if user demand exists)
6. Debian packaging with dual variants: framework-dependent (200KB) and self-contained (72MB)
7. Distribution via both GitHub Releases (.deb downloads) and Launchpad PPA (apt repository)

### Questions to Resolve
None

---

## 📅 Timeline

| Phase | Duration | Status | Start Date | End Date |
|-------|----------|--------|------------|----------|
| Phase 1: Foundation & Backup | 2 days | ✅ **Completed** | 2025-01-15 | 2025-01-16 |
| Phase 2: Variable Validation | 1 day | ✅ **Completed** | 2025-01-16 | 2025-01-16 |
| Phase 3: Debian Package Distribution | 1 week | Not Started | TBD | TBD |
| Phase 4: Simple CLI Chaining | 1 week | Not Started | TBD | TBD |
| Phase 5: Web API | 2 weeks | Not Started | TBD | TBD |
| Phase 6: Blazor WASM UI | 4 weeks | Not Started | TBD | TBD |
| Phase 7: Integration & Polish | 1 week | Not Started | TBD | TBD |
| Phase 8: Release | 1 week | Not Started | TBD | TBD |
| **Total** | **12 weeks** | **25%** | **2025-01-15** | **TBD** |

---

**Last Updated:** 2025-01-16
**Current Phase:** Phase 3 - Debian Package Distribution (Not Started)

**Phase 1 Completed (2025-01-16):**
- ✅ LocalizationManager.Shared project
- ✅ BackupVersionManager with smart rotation
- ✅ BackupRotationPolicy with configurable retention
- ✅ BackupDiffService with preview restore support
- ✅ BackupDiffFormatter (text/JSON/HTML)
- ✅ BackupRestoreService (full + selective)
- ✅ Configuration classes (BackupConfiguration, RetentionConfiguration, AutoBackupConfiguration)
- ✅ CLI backup commands (list, create, restore, diff, info, prune)
- ✅ Shell completion updates (_lrm, lrm-completion.bash)
- ✅ All CLI commands migrated to BackupVersionManager
- ✅ TUI operations updated to use BackupVersionManager
- ✅ BackupManagerWindow with interactive backup management (F7)
- ✅ BackupDiffWindow for visual diff comparison
- ✅ Complete test coverage (41 backup tests + full suite: 408 tests passing)
- ✅ Comprehensive documentation (BACKUP.md, README.md, COMMANDS.md)
- ✅ Old BackupManager.cs removed

**Phase 2 Completed (2025-01-16):**
- ✅ Core/Validation/ directory structure
- ✅ PlaceholderDetector.cs (supports .NET, printf, ICU, template literals)
- ✅ PlaceholderValidator.cs (validation logic + batch support)
- ✅ ValidationResult model updated with PlaceholderMismatches
- ✅ ResourceValidator.cs integration
- ✅ ValidateCommand updated (Table/JSON/Simple output)
- ✅ TUI validation panel updated (F6)
- ✅ PlaceholderDetectorTests.cs (comprehensive unit tests - 39 tests)
- ✅ PlaceholderValidatorTests.cs (comprehensive unit tests - 18 tests)
- ✅ PlaceholderValidationIntegrationTests.cs (integration tests - 10 tests)
- ✅ All 467 tests passing (59 new placeholder tests added)
- ✅ Build succeeds with 0 errors/warnings
- ✅ Comprehensive documentation (PLACEHOLDERS.md, README.md updated)

**Next Milestone:** Phase 3 - Debian Package Distribution (.deb + PPA)
