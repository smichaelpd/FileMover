# FileMover

FileMover is a Windows Service that watches a staging directory and moves completed `.ord` files into an output directory. It checks periodically, copies each eligible file to the destination (replacing a file with the same name), and then deletes the source file.

## How it works

- On startup, creates the input and output directories if they do not already exist.
- Checks the input directory every 1.5 seconds for top-level files matching `*.ord`.
- Waits until a file has not been modified for more than 3 seconds before processing it.
- Processes the oldest eligible file by creation time on each check.
- Copies the file to the output directory, overwriting an existing file with the same name, then removes the source.
- Attempts to write copy errors to the Windows Event Log.

The service does not watch subdirectories, and it only handles `.ord` files.

## Default paths and configuration

The current settings are defined as fields in `FileMoverService/Service1.cs`:

| Setting | Default |
| --- | --- |
| Input directory | `D:\Shares\Sales_Export\Staging` |
| Output directory | `D:\Shares\Sales_Export` |
| File pattern | `*.ord` |
| Poll interval | 1.5 seconds |
| Minimum file age | 3 seconds |

These settings are **not** read from `App.config`. To change them, edit the corresponding fields in `Service1.cs`, rebuild, and reinstall or update the service.

## Requirements

- Windows
- Visual Studio 2022 or compatible MSBuild tooling
- .NET Framework 4.7.2 targeting pack
- For the MSI installer project: .NET SDK and WiX Toolset SDK 5.0.2 (declared by the installer project)

The service must have permission to read and delete files in the input directory and create or overwrite files in the output directory. If using network shares, configure the Windows service to run under an account that has access to those shares; the installer uses the default service account unless changed.

## Build

Open `FileMoverService.sln` in Visual Studio and build the desired configuration. The Windows Service project can also be built independently with MSBuild:

```powershell
msbuild .\FileMoverService\FileMoverService.csproj /p:Configuration=Release
```

To build the WiX MSI installer project:

```powershell
dotnet build .\FileMoverServiceInstaller\FileMoverServiceInstaller.wixproj --configuration Release
```

## Install and run

The recommended approach is to build and install the MSI produced by `FileMoverServiceInstaller`. From an elevated PowerShell or Command Prompt:

```powershell
msiexec /i .\path\to\FileMoverServiceInstaller.msi
```

The installer registers the service for automatic startup and starts it after installation. To remove it:

```powershell
msiexec /x .\path\to\FileMoverServiceInstaller.msi
```

For manual installation of the service executable, run the .NET Framework `InstallUtil.exe` as an administrator:

```powershell
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe .\path\to\FileMoverService.exe
```

Use `/u` before the executable path to uninstall it. When using the manual method, start or stop **FileMoverService** through the Windows Services console or `sc.exe`.

## Important behavior

Files are copied before the source is deleted. If a copy fails, the source is retained and the service attempts to log the error. A same-named destination file is overwritten; use separate output directories if existing destination files must be preserved. The service processes files independently and does not provide a queue, configurable retry policy, or content validation.

## Repository layout

- `FileMoverService/` — Windows Service source and project
- `FileMoverServiceInstaller/` — WiX MSI installer project
- `FileMoverService.sln` — Visual Studio solution

See [LICENSE](LICENSE) for license information.
