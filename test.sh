#!/usr/bin/env sh
set -eu

dotnet test -c Debug --collect:"XPlat Code Coverage"
dotnet reportgenerator -reports:"./**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
