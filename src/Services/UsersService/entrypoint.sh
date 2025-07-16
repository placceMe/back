#!/bin/sh
set -e

dotnet ef database update --no-build --project UsersService.csproj
exec dotnet UsersService.dll
