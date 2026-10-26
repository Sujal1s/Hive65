$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
$source = Join-Path $PSScriptRoot 'source\HiveScreenRGB.cs'
$output = Join-Path $PSScriptRoot 'HiveScreenRGB.exe'
& $compiler /nologo /target:winexe /platform:anycpu /optimize+ "/out:$output" /reference:System.dll /reference:System.Core.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll $source
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed' }
Write-Host "Built $output"
