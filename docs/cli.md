# docs/cli.md

## CLI Interface

## Grundsyntax

```text
TreeSync --source <path> --target <path> [options]
```

## Parameter

| Parameter | Pflicht | Beschreibung | Standard |
| --- | --- | --- | --- |
| `--source <path>` | ja | Pfad zum Quellverzeichnis | - |
| `--target <path>` | ja | Pfad zum Zielverzeichnis | - |
| `--config <file>` | nein | Pfad zur Konfigurationsdatei | `<source>/config.json` |
| `--ignore <file>` | nein | Pfad zur Ignore-Datei | `<source>/.treesyncignore` |
| `--log <file>` | nein | Pfad zur Logdatei | `treesync.log` im aktuellen Arbeitsverzeichnis |
| `--log-level <level>` | nein | Logging-Level (`error`, `info`, `debug`) | Wert aus Konfiguration |
| `--dry-run` | nein | Simuliert alle Aktionen ohne Dateisystem-Änderungen | `false` |
| `--help`, `-h`, `/?` | nein | Zeigt die Hilfe an | `false` |

## Beispielaufrufe

Windows (EXE):

```powershell
.\TreeSync.exe `
  --source "C:\pfad\zur\quelle" `
  --target "C:\pfad\zum\ziel" `
  --config "C:\pfad\zur\quelle\config.json" `
  --ignore "C:\pfad\zur\quelle\.treesyncignore" `
  --log "treesync.log" `
  --log-level info
```

Linux (self-contained):

```bash
./publish/linux-x64/TreeSync \
  --source ./src \
  --target /var/www/app \
  --config ./config.json \
  --ignore ./.treesyncignore \
  --log ./treesync.log \
  --log-level info
```

Framework-dependent (.NET Runtime installiert):

```bash
dotnet ./publish/dotnet/TreeSync.dll \
  --source ./src \
  --target /var/www/app
```

Dry Run:

```bash
TreeSync --source ./src --target /var/www/app --dry-run
```

## Exitcodes

- `0`: Erfolg
- `1`: CLI- oder Konfigurationsfehler
- `2`: Sicherheitsprüfung fehlgeschlagen
- `3`: Unerwarteter Laufzeitfehler

## Standarddateien

Wenn nicht anders angegeben, werden folgende Dateien im Root der Quelle erwartet:

- `config.json`
- `.treesyncignore`

## Publish

Windows-x64-EXE:

```powershell
dotnet publish .\src\TreeSync.Cli\TreeSync.Cli.csproj `
  -c Release `
  -p:PublishProfile=win-x64-folder
```

Linux-x64 (self-contained):

```bash
dotnet publish ./src/TreeSync.Cli/TreeSync.Cli.csproj \
  -c Release \
  --runtime linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:EnableCompressionInSingleFile=true \
  -p:DebugType=none \
  -p:DebugSymbols=false \
  --output ./publish/linux-x64
```

Framework-dependent `.NET`:

```bash
dotnet publish ./src/TreeSync.Cli/TreeSync.Cli.csproj \
  -c Release \
  --self-contained false \
  -p:UseAppHost=false \
  --output ./publish/dotnet
```

`dotnet build` ist für Entwicklung und Tests gedacht. Für verteilbare Artefakte muss `dotnet publish` verwendet werden.
