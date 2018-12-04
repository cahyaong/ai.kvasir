ECHO ON

git submodule -q update --init --recursive

cd .\Source
.\.paket\paket.exe restore

cd ..\External\cop.core
.\appveyor_prebuild.cmd