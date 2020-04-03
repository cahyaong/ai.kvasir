ECHO ON

git submodule -q update --init --recursive

cd .\Source
dotnet restore

cd ..\External\cop.olympus
.\appveyor_prebuild.cmd