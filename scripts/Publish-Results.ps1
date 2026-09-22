<#
.SYNOPSIS
从完整报告构建精简的 Pages 数据，保留仓库中的格式化报告，不执行 Git 操作。
.PARAMETER InputFile
可选的新计算结果；默认优先 results/report.json，否则重建 reports/report.json。
#>
[CmdletBinding()]
param([string]$InputFile)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
if (-not $InputFile) {
    $InputFile = Join-Path $root 'results/report.json'
    if (-not (Test-Path -LiteralPath $InputFile)) {
        $InputFile = Join-Path $root 'reports/report.json'
    }
}
$node = Get-Command node -ErrorAction Stop
& $node.Source (Join-Path $PSScriptRoot 'build-site.mjs') $InputFile
if ($LASTEXITCODE -ne 0) {
    throw '网页数据构建失败，请检查上面的错误。'
}
Write-Host '需要备份续算进度时另运行 Save-Checkpoints.ps1。本脚本未提交或推送 Git。'
