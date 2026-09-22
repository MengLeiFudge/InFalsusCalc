<#
.SYNOPSIS
计算停止后，把有效本地进度保存为可提交的仓库快照，不执行 Git 操作。
.PARAMETER ReportFile
选择需要保留的配队库；默认优先本地完整结果，其次仓库完整报告。
#>
[CmdletBinding()]
param([string]$ReportFile)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$state = Join-Path $root 'state'
if (-not $ReportFile) {
    $ReportFile = Join-Path $root 'results/report.json'
    if (-not (Test-Path -LiteralPath $ReportFile)) {
        $ReportFile = Join-Path $root 'reports/report.json'
    }
}
$report = Get-Content -LiteralPath $ReportFile -Raw -Encoding UTF8 | ConvertFrom-Json
if ($report.library -notmatch '^[a-f0-9]{24}$') {
    throw '结果中缺少有效的配队库指纹。'
}
$catalog = Get-Content -LiteralPath (Join-Path $root 'Calculator/Data/catalog.json') -Raw -Encoding UTF8 | ConvertFrom-Json
if ($report.catalog.id -ne $catalog.id) {
    throw '结果与当前游戏资源不兼容。'
}
$lease = [IO.File]::Open((Join-Path $state 'calculation.lock'), 'OpenOrCreate', 'ReadWrite', 'None')
$stage = Join-Path $state "checkpoint-snapshot-$PID"
try {
    if (Test-Path -LiteralPath $stage) {
        throw "存在未处理的快照暂存目录：$stage"
    }
    New-Item -ItemType Directory -Path (Join-Path $stage 'key-recipes') | Out-Null
    foreach ($recipe in $catalog.recipes) {
        $name = '{0:00}.json' -f [int]$recipe.Id.Value
        $source = Join-Path $state "key-recipes/$name"
        $saved = Get-Content -LiteralPath $source -Raw -Encoding UTF8 | ConvertFrom-Json
        if ($saved.recipe -ne $recipe.Id.Value -or $saved.snapshot -ne $catalog.id) {
            throw "检查点不兼容：$source"
        }
        Copy-Item -LiteralPath $source -Destination (Join-Path $stage "key-recipes/$name")
    }
    # 旧布局仍由 RestoreBounds 复核后用作候选，不把它们当作旧最优证明。
    if (Test-Path -LiteralPath (Join-Path $state 'recipes')) {
        New-Item -ItemType Directory -Path (Join-Path $stage 'recipes') | Out-Null
        Get-ChildItem -LiteralPath (Join-Path $state 'recipes') -Filter '*.json' -File |
            Copy-Item -Destination (Join-Path $stage 'recipes')
    }
    $decks = Join-Path $stage "decks/$($report.library)"
    New-Item -ItemType Directory -Path $decks -Force | Out-Null
    foreach ($encounter in $catalog.encounters) {
        foreach ($limit in 0..2) {
            $name = '{0:000}-p{1}.json' -f [int]$encounter.id, $limit
            $source = Join-Path $state "decks/$($report.library)/$name"
            $deck = Get-Content -LiteralPath $source -Raw -Encoding UTF8 | ConvertFrom-Json
            if ($deck.library -ne $report.library -or $deck.encounter -ne $encounter.id -or $deck.max_strikes -ne $limit) {
                throw "配队检查点不兼容：$source"
            }
            Copy-Item -LiteralPath $source -Destination (Join-Path $decks $name)
        }
    }
    $destination = Join-Path $root 'checkpoints'
    $backup = Join-Path $root ('.codex/trash/checkpoints-' + (Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
    if (Test-Path -LiteralPath $destination) {
        New-Item -ItemType Directory -Path (Split-Path $backup -Parent) -Force | Out-Null
        Move-Item -LiteralPath $destination -Destination $backup
    }
    try {
        Move-Item -LiteralPath $stage -Destination $destination
    } catch {
        if (Test-Path -LiteralPath $backup) {
            Move-Item -LiteralPath $backup -Destination $destination
        }
        throw
    }
    Write-Host "检查点快照已准备：$destination；未提交或推送 Git。"
} finally {
    $lease.Dispose()
    if (Test-Path -LiteralPath $stage) {
        $failed = Join-Path $root ('.codex/trash/incomplete-snapshot-' + (Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
        New-Item -ItemType Directory -Path (Split-Path $failed -Parent) -Force | Out-Null
        Move-Item -LiteralPath $stage -Destination $failed
    }
}
