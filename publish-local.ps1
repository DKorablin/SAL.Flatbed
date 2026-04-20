<#
.SYNOPSIS
	Builds, signs, packs and publishes SAL.Flatbed to a local NuGet feed folder.

.PARAMETER Version
	Package version to publish. Defaults to the version in Directory.Build.props with a -local suffix.

.PARAMETER LocalFeed
	Absolute path to the local NuGet feed folder. Defaults to C:\LocalNuGet.

.EXAMPLE
	.\publish-local.ps1
	.\publish-local.ps1 -Version "1.2.13-beta1"
	.\publish-local.ps1 -Version "1.2.13-beta1" -LocalFeed "D:\MyFeed"
#>
param(
	[string]$Version,
	[string]$LocalFeed = "C:\LocalNuGet"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# -- Resolve version from Directory.Build.props if not supplied --
if(-not $Version)
{
	$propsFile = Join-Path $PSScriptRoot "Directory.Build.props"
	$xml = [xml](Get-Content $propsFile)
	$baseVersion = $xml.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
	$Version = "$baseVersion-local"
}

Write-Host "Version  : $Version"
Write-Host "Feed     : $LocalFeed"

# -- Ensure feed folder exists and is registered with NuGet --
$null = New-Item -ItemType Directory -Force -Path $LocalFeed
$sources = dotnet nuget list source 2>&1
if($sources -notmatch [regex]::Escape($LocalFeed))
{
	dotnet nuget add source $LocalFeed --name "local-sal"
	Write-Host "Registered '$LocalFeed' as 'local-sal' NuGet source."
}

# -- Remove any older local pre-release of this package to avoid cache confusion --
$oldPackages = @(Get-ChildItem -Path $LocalFeed -Filter "SAL.Flatbed.*.nupkg" -ErrorAction SilentlyContinue)
if($oldPackages.Count -gt 0)
{
	$oldPackages | Remove-Item -Force
	Write-Host "Removed $($oldPackages.Count) old package(s) from feed."
}

# -- Build, sign and pack --
$snkPath = "C:\Visual Studio Projects\C#\AlphaOmega (NoPwd).snk"
if(-not (Test-Path $snkPath))
{
	Write-Warning "Signing key not found at '$snkPath'. Assembly will not be signed."
	$snkPath = $null
}

$project = Join-Path $PSScriptRoot "SAL.Flatbed\SAL.Flatbed.csproj"
$signArgs = if($snkPath) { @("/p:SignAssembly=true", "/p:AssemblyOriginatorKeyFile=$snkPath") } else { @() }

dotnet pack $project `
	--configuration Release `
	/p:Version=$Version `
	--output $LocalFeed `
	@signArgs

if($LASTEXITCODE -ne 0) { throw "dotnet pack failed with exit code $LASTEXITCODE" }

Write-Host ""
Write-Host "Published SAL.Flatbed $Version to $LocalFeed"
Write-Host ""
Write-Host "In consuming solutions, reference it with:"
Write-Host "  <PackageReference Include=""SAL.Flatbed"" Version=""$Version"" />"
