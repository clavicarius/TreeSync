# docs/development.md

## Technologie

- Programmiersprache: C#
- Runtime/SDK: .NET 10
- Anwendungstyp: .NET Console App
- Default Namespace: `clausTrarius.TreeSync`

## Voraussetzungen

- .NET 10 SDK

Prüfen mit:

```bash
dotnet --list-sdks
```

Für Entwicklung, Build und Tests muss ein `10.x` SDK installiert sein, da alle Projekte `net10.0` targeten.

## Projektstruktur

Der Code folgt der Architektur aus `docs/architecture.md`:

- `src/TreeSync.Cli`: CLI-Parsing, Top-Level-Fehlerbehandlung und Exitcodes
- `src/TreeSync.Core`: Konfiguration, Logging, Ignore-Regeln, Scanning, Vergleich, Safety und Sync-Engine
- `tests/TreeSync.Tests`: xUnit-Tests für die zentralen Komponenten

## Build und Tests

```bash
dotnet restore TreeSync.sln
dotnet build TreeSync.sln --configuration Release
dotnet test TreeSync.sln --configuration Release
```

`dotnet build` ist für Entwicklung und Tests gedacht. Für verteilbare Windows-, Linux- oder `.NET`-Artefakte bitte `dotnet publish` verwenden (siehe `docs/cli.md`).

## Release

Releases werden über Git-Tags im Format `vMAJOR.MINOR.PATCH` erstellt:

```bash
git tag v1.2.3
git push origin v1.2.3
```

Der Tag-Push startet den GitHub Actions Release-Workflow. Details stehen in `docs/release-pipeline.md` und `docs/create-release.md`.

## VSCode/Cursor

Die Workspace-Konfiguration liegt in `.vscode/`:

- `launch.json`: Debug-Konfiguration `TreeSync CLI: Debug` mit Dry-Run
- `tasks.json`: Tasks für Restore, Build, Test und Publish
- `settings.json`: `TreeSync.sln` als Default-Solution
- `extensions.json`: empfohlene C#/.NET-Erweiterungen

Die Publish-Tasks erzeugen Windows-, Linux- und framework-dependent `.NET`-Artefakte im Ordner `publish`.
