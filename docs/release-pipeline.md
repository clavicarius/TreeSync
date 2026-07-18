# Release Pipeline

Dieses Dokument beschreibt den Release-Prozess für TreeSync.

## Überblick

TreeSync verwendet GitHub Actions für CI und Release-Erstellung. Releases sind tag-getrieben:

```powershell
git tag v1.2.3
git push origin v1.2.3
```

Ein Tag im Format `vMAJOR.MINOR.PATCH` erstellt automatisch einen GitHub Draft Release.

## Projektanpassungen

Für die GitHub-Pipeline verwendet TreeSync die moderne .NET-Toolchain:

- `TreeSync.Cli.csproj`, `TreeSync.Core.csproj` und `TreeSync.Tests.csproj` sind SDK-style-Projekte
- Ziel-Framework ist `net10.0`
- Assembly-Metadaten liegen in den Projektdateien
- `AssemblyVersion` und `FileVersion` bleiben stabil auf `1.0.0.0`
- Die Release-Version wird erst beim Publish über `InformationalVersion` gesetzt
- `App.config` wird nicht benötigt
- `Properties/AssemblyInfo.cs` wird nicht benötigt

Die Release-Pipeline erzeugt drei Distributionsvarianten:

- `win-x64` als self-contained Single-File-EXE
- `linux-x64` als self-contained Single-File-Binary
- `.NET` als framework-dependent Paket für Systeme mit installierter .NET 10 Runtime

## Workflow-Dateien

Die Pipeline besteht aus zwei GitHub Actions Workflows:

```text
.github/workflows/ci.yml
.github/workflows/release.yml
```

`ci.yml` prüft jede Änderung auf `main` und in Pull Requests. `release.yml` erstellt ausschließlich für Versionstags einen GitHub Draft Release.

## CI

Der CI-Workflow läuft bei jedem Push auf `main` und bei Pull Requests gegen `main`.

Ablauf:

1. Repository auschecken
2. .NET 10 SDK installieren
3. `dotnet restore TreeSync.sln`
4. `dotnet build TreeSync.sln --configuration Release`
5. `dotnet test TreeSync.sln --configuration Release`
6. Windows-x64-Artefakt veröffentlichen
7. Smoke-Test mit `TreeSync.exe --help`
8. Linux-x64-Artefakt veröffentlichen
9. Smoke-Test mit `./TreeSync --help`
10. framework-dependent `.NET`-Artefakt veröffentlichen
11. Smoke-Test mit `dotnet TreeSync.dll --help`

Die CI erstellt keinen GitHub Release.

Die Publish-Schritte in der CI dienen nur dem Smoke-Test. Dadurch wird geprüft, ob alle späteren Release-Artefakte grundsätzlich gebaut und gestartet werden können.

## Release

Der Release-Workflow läuft nur bei Tags:

```text
v1.0.0
v1.1.0
v2.0.0
```

Ablauf:

1. Tag wird gepusht
2. GitHub Actions startet den Release-Workflow
3. TreeSync wird für `win-x64`, `linux-x64` und `.NET` veröffentlicht
4. Die Windows- und Linux-Artefakte werden self-contained und als Single-File-Binaries gebaut
5. Die Linux- und `.NET`-Artefakte werden mit `--help` gestartet
6. Windows-EXE und `.NET`-Paket werden als ZIP gebaut, das Linux-Binary als `tar.gz`
7. GitHub Release wird als Draft erstellt
8. Alle Distributionsdateien werden an den Draft Release angehängt
9. Release Notes werden manuell anhand von `CHANGELOG.md` geprüft und veröffentlicht

## Artefakt

Für Tag `v1.2.3` entstehen:

```text
TreeSync-1.2.3-win-x64.zip
TreeSync-1.2.3-linux-x64.tar.gz
TreeSync-1.2.3-dotnet.zip
```

`TreeSync-1.2.3-win-x64.zip` enthält:

- `TreeSync.exe`
- `README.md`

`TreeSync-1.2.3-linux-x64.tar.gz` enthält:

- `TreeSync`
- `README.md`

`TreeSync-1.2.3-dotnet.zip` enthält die veröffentlichte CLI inklusive `TreeSync.dll`, `TreeSync.runtimeconfig.json`, `TreeSync.deps.json` und `README.md`.

Die Windows- und Linux-Binaries sind self-contained und benötigen auf dem Zielsystem keine installierte .NET Runtime. Das `.NET`-Paket setzt eine installierte .NET 10 Runtime voraus.

## Versionierung

Die Release-Version wird ausschließlich über den Git-Tag definiert:

```text
vMAJOR.MINOR.PATCH
```

Die führende `v` wird für Dateinamen und `InformationalVersion` entfernt. `AssemblyVersion` und `FileVersion` bleiben stabil, damit die Assembly-Kompatibilität nicht an Release-Tags gekoppelt ist.

Beispiel:

```text
Tag:                  v1.2.3
InformationalVersion: 1.2.3
ZIP-Datei:            TreeSync-1.2.3-win-x64.zip
Linux-Datei:          TreeSync-1.2.3-linux-x64.tar.gz
.NET-Datei:           TreeSync-1.2.3-dotnet.zip
```

## Lokale Validierung

Vor dem Commit oder vor einem Release sollten die gleichen Kernschritte lokal ausführbar sein:

```powershell
dotnet restore TreeSync.sln
dotnet build TreeSync.sln --configuration Release
dotnet test TreeSync.sln --configuration Release
dotnet publish src/TreeSync.Cli/TreeSync.Cli.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:EnableCompressionInSingleFile=true `
  -p:DebugType=none `
  -p:DebugSymbols=false
```

```bash
dotnet publish src/TreeSync.Cli/TreeSync.Cli.csproj \
  --configuration Release \
  --runtime linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:EnableCompressionInSingleFile=true \
  -p:DebugType=none \
  -p:DebugSymbols=false \
  --output publish/linux-x64
dotnet publish src/TreeSync.Cli/TreeSync.Cli.csproj \
  --configuration Release \
  --self-contained false \
  -p:UseAppHost=false \
  --output publish/dotnet
```

Optionaler Smoke-Test nach lokalem Publish:

```powershell
.\src\TreeSync.Cli\bin\Release\net10.0\win-x64\publish\TreeSync.exe --help
```

```bash
./publish/linux-x64/TreeSync --help
dotnet ./publish/dotnet/TreeSync.dll --help
```

## Changelog und Release Notes

`CHANGELOG.md` ist die menschlich gepflegte Quelle für Release Notes. Neue Änderungen werden zunächst unter `## [Unreleased]` gesammelt und vor dem Release in einen versionierten Abschnitt verschoben.

Der Release-Workflow veröffentlicht den GitHub Release nicht direkt. Er erstellt einen Draft Release mit Artefakt, damit die Release Notes vor der Veröffentlichung anhand von `CHANGELOG.md` geprüft und bei Bedarf angepasst werden können.

Details zum Changelog-Prozess stehen in [`docs/versioning.md`](versioning.md).

## Release-Erstellung

Eine detaillierte Schritt-für-Schritt-Anleitung zum Erstellen eines Releases findest du in [`docs/create-release.md`](create-release.md).

Kurzfassung: Ein Release wird lokal vorbereitet, indem der gewünschte Commit auf `main` getaggt wird:

```powershell
git tag v1.2.3
git push origin v1.2.3
```

Der Push des Tags startet den Release-Workflow. Der Workflow erzeugt die Release-Artefakte und hängt sie an den automatisch erstellten Draft Release an.

Da der Remote-Zugriff per SSH-Key passphrase-geschützt ist, müssen `git push`, `git pull` und `git fetch` in einer lokal freigegebenen Shell ausgeführt werden.

## Designentscheidungen

- Kein NuGet-Paket
- Distribution als CLI-Binaries plus framework-dependent `.NET`-Paket
- Windows- und Linux-x64-Release-Artefakte
- Self-contained Single-File-Binary für einfache Nutzung
- Tests plus Smoke-Tests gegen alle veröffentlichten Varianten
- Draft Release statt automatischer Veröffentlichung

## Reproduzierbarkeit

Releases werden bestimmt durch:

- Git-Tag
- .NET 10 SDK (`10.0.x`)
- GitHub Actions Runner `windows-latest` und `ubuntu-latest`
- Runtime Identifier `win-x64` und `linux-x64`
