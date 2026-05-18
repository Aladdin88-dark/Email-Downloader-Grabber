@echo off
setlocal

REM Build/publish WPF app into .\BuildOutput
set SCRIPT_DIR=%~dp0
pushd "%SCRIPT_DIR%"

if exist "BuildOutput" (
  rmdir /s /q "BuildOutput"
)

echo Publishing to BuildOutput...
dotnet publish "Auto_Uploader.csproj" -c Release -o "BuildOutput"
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

REM External content is copied by project metadata.
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
popd
endlocal
