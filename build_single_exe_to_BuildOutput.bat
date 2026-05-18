@echo off
setlocal

REM Publish as a single-file EXE into .\BuildOutput
REM Notes:
REM - True "one EXE" is limited if you use Playwright JS rendering (it downloads/uses browser binaries).
REM - This script makes the app itself a single-file exe.

set SCRIPT_DIR=%~dp0
pushd "%SCRIPT_DIR%"

if exist "BuildOutput" (
  rmdir /s /q "BuildOutput"
)

echo Publishing SINGLE-FILE EXE to BuildOutput...
dotnet publish "Auto_Uploader.csproj" -c Release -r win-x64 -o "BuildOutput" ^
  /p:PublishSingleFile=true ^
  /p:SelfContained=true ^
  /p:IncludeNativeLibrariesForSelfExtract=true ^
  /p:IncludeAllContentForSelfExtract=true ^
  /p:DebugType=None

if errorlevel 1 (
  echo.
  echo Build failed.
  popd
  exit /b 1
)

REM Ensure Result folder exists for IMAP/Web outputs
if not exist "BuildOutput\Result" (
  mkdir "BuildOutput\Result"
)

REM External content is copied by project metadata and kept outside the single-file bundle.
if not exist "BuildOutput\imap_servers.txt" (
  echo.
  echo WARNING: imap_servers.txt was not published next to the EXE.
)

if not exist "BuildOutput\appsettings.json" (
  echo.
  echo WARNING: appsettings.json was not published next to the EXE.
)

if not exist "BuildOutput\dictionaries" (
  echo.
  echo WARNING: dictionaries folder was not published next to the EXE.
)

echo.
echo Done. Output: %SCRIPT_DIR%BuildOutput
echo EXE: "%SCRIPT_DIR%BuildOutput\Email Grabber.exe"
popd
endlocal
