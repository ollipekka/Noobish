#!/usr/bin/env sh
set -eu

dotnet test -c Debug --collect:"XPlat Code Coverage"

rm -rf coveragereport

dotnet reportgenerator -reports:"./src/Noobish.Test/TestResults/**/coverage.cobertura.xml;./src/Noobish.MonoGame.Test/TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html "-assemblyfilters:+Noobish*" "-filefilters:-*Noobish.Test*;-*Noobish.MonoGame.Test*"
