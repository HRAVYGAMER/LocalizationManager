# Building and Distribution Guide

## Quick Build

To build all platform targets with a single command:

```bash
./build.sh
```

This will:
1. Run all tests (must pass before building)
2. Build for 4 platforms (Linux x64/ARM64, Windows x64/ARM64)
3. Create distribution archives
4. Display build summary

## Build Output

After running `build.sh`, you'll find:

```
publish/
├── linux-x64/
│   ├── lrm              # Linux executable
│   └── README.txt
├── linux-arm64/
│   ├── lrm              # ARM Linux executable
│   └── README.txt
├── win-x64/
│   ├── lrm.exe          # Windows executable
│   └── README.txt
├── win-arm64/
│   ├── lrm.exe          # ARM Windows executable
│   └── README.txt
├── lrm-0.6.0-linux-x64.tar.gz
├── lrm-0.6.0-linux-arm64.tar.gz
├── lrm-0.6.0-win-x64.zip
└── lrm-0.6.0-win-arm64.zip
```

## Installation

### Linux - System-wide
```bash
sudo cp publish/linux-x64/lrm /usr/local/bin/
sudo chmod +x /usr/local/bin/lrm
lrm --version
```

### Linux - User-local
```bash
mkdir -p ~/.local/bin
cp publish/linux-x64/lrm ~/.local/bin/
chmod +x ~/.local/bin/lrm

# Add to PATH (add to ~/.bashrc or ~/.zshrc):
export PATH="$HOME/.local/bin:$PATH"

source ~/.bashrc  # or source ~/.zshrc
lrm --version
```

### Windows
1. Extract `lrm-0.6.0-win-x64.zip`
2. Move `lrm.exe` to a directory in your PATH
   - Or add the directory to PATH in System Environment Variables
3. Open Command Prompt or PowerShell:
   ```cmd
   lrm --version
   ```

## Manual Build

If you want to build manually without the script:

### Single Platform
```bash
dotnet publish \
  --configuration Release \
  --runtime linux-x64 \
  --self-contained true \
  --output publish/linux-x64 \
  /p:PublishSingleFile=true
```

### Available Runtimes
- `linux-x64` - Intel/AMD Linux
- `linux-arm64` - ARM Linux (Raspberry Pi, etc.)
- `win-x64` - Intel/AMD Windows
- `win-arm64` - ARM Windows
- `osx-x64` - Intel macOS
- `osx-arm64` - Apple Silicon macOS

## Version Management

Version is defined in `LocalizationManager.csproj`:

```xml
<Version>0.6.0</Version>
<AssemblyVersion>0.6.0.0</AssemblyVersion>
<FileVersion>0.6.0.0</FileVersion>
```

To release a new version:
1. Update version numbers in `.csproj`
2. Update `VERSION` in `build.sh`
3. Run `./build.sh`
4. Test the executables
5. Tag the release in git:
   ```bash
   git tag -a v0.6.0 -m "Release v0.6.0"
   git push origin v0.6.0
   ```

## Build Requirements

- .NET 9 SDK
- `zip` command (for Windows archive creation on Linux)
- `tar` command (standard on Linux/macOS)
- Bash shell

## Troubleshooting

### Tests Failing
```bash
cd LocalizationManager.Tests
dotnet test --verbosity detailed
```

### Build Errors
```bash
dotnet clean
dotnet restore
dotnet build --configuration Release
```

### Permission Denied (Linux)
```bash
chmod +x build.sh
chmod +x publish/linux-x64/lrm
```

## Distribution Checklist

Before releasing:
- [ ] All tests passing (`dotnet test`)
- [ ] Version updated in `.csproj` and `build.sh`
- [ ] Build script runs successfully (`./build.sh`)
- [ ] Linux executable tested (`./publish/linux-x64/lrm --version`)
- [ ] Command functionality verified (`lrm validate --path TestData`)
- [ ] Archives contain correct files
- [ ] README files generated correctly
- [ ] Git tag created
- [ ] Release notes prepared
