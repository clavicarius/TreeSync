@echo off
setlocal
pushd "%~dp0"

set "TREESYNC_BIN=%~dp0TreeSync.exe"
set "SOURCEPATH=%~1"
set "TARGETPATH=%~2"
set "CONFIGFILE=%~dp0treesync-config.json"
set "IGNOREFILE=%~dp0treesync-ignore.txt"

if "%SOURCEPATH%"=="" (
  echo Usage: %~nx0 ^<sourcePath^> ^<targetPath^> [additional TreeSync args...]
  exit /b 1
)

if "%TARGETPATH%"=="" (
  echo Usage: %~nx0 ^<sourcePath^> ^<targetPath^> [additional TreeSync args...]
  exit /b 1
)

"%TREESYNC_BIN%" --source "%SOURCEPATH%" --target "%TARGETPATH%" --config "%CONFIGFILE%" --ignore "%IGNOREFILE%" %3 %4 %5 %6 %7 %8 %9

popd
endlocal
