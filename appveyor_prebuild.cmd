ECHO OFF

SETLOCAL
SET PATH=%PATH%;%~dp0Tools\NuGet

ECHO ON

git submodule -q update --init --recursive

.\Source\.paket\paket.exe restore
nuget restore External\cop.core\Source\nGratis.Cop.Core.sln