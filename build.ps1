# Build script for generating NuGet packages

if (Test-Path -Path "output")
{
  Remove-Item output\* -Recurse -Force
}

# Clean both debug and release
dotnet clean
dotnet clean --configuration Release

# build package for release
dotnet build --configuration Release

## TODO: Build project and set version to 1.2.3.4
# dotnet build -p:Version=1.2.3.4
