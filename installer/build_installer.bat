@echo off
setlocal

rem Locate MSBuild
where msbuild >nul 2>&1
if errorlevel 1 (
  if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD="%ProgramFiles(x86)%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
  ) else (
    echo MSBuild not found. Install Visual Studio or Build Tools.
    exit /b 1
  )
) else (
  set MSBUILD=msbuild
)

rem Build Release
%MSBUILD% "..\SWAutoAttributes.sln" /restore /p:Configuration=Release
if errorlevel 1 (
  echo Build failed.
  exit /b 1
)

rem Build installer with Inno Setup
if not exist "%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe" (
  echo Inno Setup 6 not found. Please install from https://jrsoftware.org/isinfo.php
  exit /b 1
)

"%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe" "SWAutoAttributes.iss"
if errorlevel 1 (
  echo Installer build failed.
  exit /b 1
)

echo Done. Output: installer\SW_Auto_Attributes_Setup.exe
endlocal
