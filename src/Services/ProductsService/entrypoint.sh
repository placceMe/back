#!/bin/sh
set -e

dotnet ef database update --no-build --project ProductsService.csproj
exec dotnet ProductsService.dll
