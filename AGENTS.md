# Repository Rules

- All tracked text files use CRLF. `.gitattributes` and `.editorconfig` are authoritative.
- `requirements/` is private local material. Never stage, commit, package, or publish it.
- Any substantive public README change must update the English, Simplified Chinese, and Russian versions together.

## Release Build

- Build `Release|x64` with .NET Framework MSBuild, not `dotnet build`:
  `& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe' 'WinSuperResolution.sln' /t:Rebuild /p:Configuration=Release /p:Platform=x64 /nologo /v:minimal`
- After a release build, run `tests\WinSuperResolution.SmokeTests\bin\Release\WinSuperResolution.SmokeTests.exe` and require a zero exit code.
