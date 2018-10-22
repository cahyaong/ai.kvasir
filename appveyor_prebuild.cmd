ECHO OFF

SETLOCAL
SET PATH=%PATH%;%~dp0Tools\NuGet

ECHO ON

git submodule -q update --init --recursive

nuget restore External\cop.core\Source\nGratis.Cop.Core.sln