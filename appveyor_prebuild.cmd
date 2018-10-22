ECHO OFF

SETLOCAL
SET PATH=%PATH%;%~dp0Tools\NuGet

ECHO ON

git submodule -q update --init --recursive

cd .\Source
.\.paket\paket.exe restore
cd ..

nuget restore External\cop.core\Source\nGratis.Cop.Core.sln