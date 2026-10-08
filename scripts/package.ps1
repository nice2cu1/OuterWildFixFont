param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version
)

$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$project = Join-Path $repo 'OuterWildFixFont'
# 每次使用独立目录，避免将上次构建产物或用户配置装入发布包
$work = Join-Path $repo ('artifacts/' + [Guid]::NewGuid().ToString('N'))
$output = Join-Path $work 'build'
$package = Join-Path $work 'package'
New-Item -ItemType Directory -Force $package | Out-Null

dotnet build (Join-Path $project 'OuterWildFixFont.csproj') --configuration Release --output $output "-p:Version=$Version" "-p:AssemblyVersion=$Version.0" "-p:FileVersion=$Version.0" -p:ContinuousIntegrationBuild=true
if ($LASTEXITCODE -ne 0) { throw 'Release build failed.' }

foreach ($file in @('OuterWildFixFont.dll', 'default-config.json', 'manifest.json', 'Fonts/GameFont.ttf'))
{
    $source = Join-Path $output $file
    if (!(Test-Path -LiteralPath $source -PathType Leaf)) { throw "Missing package file: $file" }
    $destination = Join-Path $package $file
    New-Item -ItemType Directory -Force (Split-Path $destination -Parent) | Out-Null
    Copy-Item -LiteralPath $source -Destination $destination
}
$manifestPath = Join-Path $package 'manifest.json'
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$manifest.version = $Version
$manifest | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $manifestPath -Encoding UTF8
Copy-Item -LiteralPath (Join-Path $repo 'LICENSE') -Destination $package
$archive = Join-Path $work "OuterWildFixFont-$Version.zip"
# ZIP 根目录直接包含 manifest.json，供 OWML 安装
Compress-Archive -Path (Join-Path $package '*') -DestinationPath $archive
$hash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
"$hash  $(Split-Path $archive -Leaf)" | Set-Content -LiteralPath "$archive.sha256" -Encoding ASCII
if ($env:GITHUB_OUTPUT)
{
    "archive=$archive" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
    "checksum=$archive.sha256" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
}
Write-Host "Package: $archive"
