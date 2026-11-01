<#
.SYNOPSIS
    Benchmarks the working tree against one or more baseline commits, rotating the builds round by round.

.DESCRIPTION
    Exports every baseline ref with git archive and copies this benchmark project into each, so all
    builds run the same benchmark code against their own AngleSharp.Xml. Each build runs with
    `dotnet run` from inside its own source tree: BenchmarkDotNet rebuilds the benchmark project
    it finds from the current directory, so running every build from one tree would measure one
    library several times. Every setup writes the module id of the AngleSharp.Xml it loaded, and
    each round fails if a build's logs name another build's library, or none.

    Before measuring, --check runs every benchmark's setup and body once on every build. Each round
    starts with a different build, so drift over the run does not favour one; each round keeps
    BenchmarkDotNet's results apart under the output directory, and summary.md compares the
    median of the rounds per case. On Windows the machine is kept awake for the whole run.

    Benchmarks need an otherwise idle machine. Pin them to fast, homogeneous cores with -Affinity
    on hybrid CPUs.

.EXAMPLE
    ./Compare-Builds.ps1 -BaselineRef e314d1b -BuildOnly
    ./Compare-Builds.ps1 -BaselineRef e314d1b, 771dafb -Rounds 3 -Affinity <mask> -Filter '*TextContent*'
#>
[CmdletBinding()]
param(
    # The commits the working tree is compared against.
    [String[]] $BaselineRef = @('HEAD'),
    [Int32] $Rounds = 3,
    [String[]] $Filter = @('*'),
    [String] $Framework = 'net8.0',
    # AngleSharp package version for every build; empty keeps each project's default.
    [String] $AngleSharpVersion = '',
    # BenchmarkDotNet --affinity mask; empty runs unpinned.
    [String] $Affinity = '',
    [String] $OutputDirectory = (Join-Path ([IO.Path]::GetTempPath()) ('anglesharp-xml-benchmarks-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))),
    # Build every copy and run --check, without measuring.
    [Switch] $BuildOnly
)

$ErrorActionPreference = 'Stop'
$repository = (git -C $PSScriptRoot rev-parse --show-toplevel).Trim()
$benchmarkProject = 'src/AngleSharp.Xml.Benchmarks'
$markerPattern = '// AngleSharp\.Xml module ([0-9a-f-]{36})'
$currentBenchmarks = Join-Path $repository $benchmarkProject

New-Item -ItemType Directory -Force $OutputDirectory | Out-Null

$projects = [ordered]@{}

# Baselines: each ref's sources plus the current benchmark project
foreach ($ref in $BaselineRef) {
    $name = 'baseline-' + ($ref -replace '[^A-Za-z0-9._-]', '_')
    $source = Join-Path $OutputDirectory "$name-source"
    New-Item -ItemType Directory -Force $source | Out-Null
    $archive = Join-Path $OutputDirectory "$name.tar"
    git -C $repository archive --format=tar -o $archive $ref src
    if ($LASTEXITCODE -ne 0) { throw "git archive of $ref failed." }
    tar -xf $archive -C $source
    Remove-Item $archive

    $benchmarks = Join-Path $source $benchmarkProject
    Remove-Item $benchmarks -Recurse -Force -ErrorAction SilentlyContinue
    Get-ChildItem $currentBenchmarks -Recurse -File |
        Where-Object { $_.FullName.Substring($currentBenchmarks.Length) -notmatch '[\\/](bin|obj|BenchmarkDotNet\.Artifacts)[\\/]' } |
        ForEach-Object {
            $target = Join-Path $benchmarks $_.FullName.Substring($currentBenchmarks.Length)
            New-Item -ItemType Directory -Force (Split-Path $target) | Out-Null
            Copy-Item $_.FullName $target
        }

    $projects[$name] = $benchmarks
}

$projects['candidate'] = $currentBenchmarks
$names = @($projects.Keys)
$modules = @{}

# MSBuild reads environment variables as properties, so this also reaches the project
# BenchmarkDotNet generates and builds for every benchmark run
$previousAngleSharpVersion = $env:AngleSharpVersion
if ($AngleSharpVersion) { $env:AngleSharpVersion = $AngleSharpVersion }

function Invoke-Benchmarks([String] $name, [String[]] $arguments) {
    Push-Location $projects[$name]
    try {
        $output = dotnet run -c Release -f $Framework -- @arguments 2>&1 | ForEach-Object { Write-Host $_; "$_" }
        if ($LASTEXITCODE -ne 0) { throw "The $name build failed: dotnet run $($arguments -join ' ') (exit code $LASTEXITCODE)." }
        return $output
    }
    finally {
        Pop-Location
    }
}

function Format-Time([Double] $nanoseconds) {
    if ($nanoseconds -ge 1e6) { return '{0:N2} ms' -f ($nanoseconds / 1e6) }
    if ($nanoseconds -ge 1e3) { return '{0:N2} us' -f ($nanoseconds / 1e3) }
    return '{0:N1} ns' -f $nanoseconds
}

function Get-Median([Double[]] $values) {
    $sorted = $values | Sort-Object
    return $sorted[[Int32][Math]::Floor($sorted.Count / 2)]
}

# The CSV report writes every mean in ns and every allocation in B, with invariant separators
function Write-Summary {
    $rows = foreach ($csv in Get-ChildItem $OutputDirectory -Recurse -Filter *-report.csv) {
        $build = $csv.Directory.Parent.Name

        foreach ($row in Import-Csv $csv.FullName) {
            # A joined run writes ? for a parameter the benchmark class does not have
            $parameters = @($row.PSObject.Properties | Where-Object { $_.Name -in @('Shape', 'Doctype') -and $_.Value -and $_.Value -ne '?' } | ForEach-Object Value) -join ' '
            [PSCustomObject]@{
                Build = $build
                Case = ("$($row.Type -replace 'Benchmark$', '') $parameters").Trim()
                Mean = [Double]($row.Mean -replace '[^\d.]', '')
                Allocated = [Double]($row.Allocated -replace '[^\d.]', '')
            }
        }
    }

    $lines = @(
        "# Benchmark comparison",
        "",
        "Median of $Rounds rounds per build; ratio = build / candidate; spread = (max - min) / median of the rounds.",
        "",
        ("| Case | " + (($names | ForEach-Object { "$_ | ratio | spread | allocated" }) -join ' | ') + " |"),
        ("|---|" + (($names | ForEach-Object { '---:|---:|---:|---:' }) -join '|') + "|")
    )

    foreach ($case in $rows | Select-Object -ExpandProperty Case -Unique) {
        $candidate = Get-Median @($rows | Where-Object { $_.Case -eq $case -and $_.Build -eq 'candidate' } | ForEach-Object Mean)
        $cells = foreach ($name in $names) {
            $measured = @($rows | Where-Object { $_.Case -eq $case -and $_.Build -eq $name })
            $median = Get-Median @($measured | ForEach-Object Mean)
            $spread = (($measured.Mean | Measure-Object -Maximum).Maximum - ($measured.Mean | Measure-Object -Minimum).Minimum) / $median * 100
            '{0} | {1:N3} | {2:N1}% | {3:N0} B' -f (Format-Time $median), ($median / $candidate), $spread, (Get-Median @($measured | ForEach-Object Allocated))
        }

        $lines += "| $case | " + ($cells -join ' | ') + " |"
    }

    $summary = Join-Path $OutputDirectory 'summary.md'
    Set-Content $summary $lines
    Write-Host "Summary in $summary"
}

$keepAwake = $IsWindows -or $env:OS -eq 'Windows_NT'

try {
    foreach ($name in $names) {
        Write-Host "Building and checking $name ..."
        $output = Invoke-Benchmarks $name @('--check')
        $match = $output | Select-String $markerPattern | Select-Object -First 1
        if (-not $match) { throw "The $name --check output names no AngleSharp.Xml module." }
        $modules[$name] = $match.Matches[0].Groups[1].Value
        Write-Host "$name measures AngleSharp.Xml module $($modules[$name])"
    }

    foreach ($name in $names) {
        $same = @($names | Where-Object { $_ -ne $name -and $modules[$_] -eq $modules[$name] })
        if ($same) { throw "$name and $($same -join ', ') load the same AngleSharp.Xml module ($($modules[$name])); there is nothing to compare." }
    }

    if ($BuildOnly) {
        Write-Host "Built and checked every copy under $OutputDirectory"
        return
    }

    if ($keepAwake) {
        Add-Type -Namespace Benchmarks -Name Power -MemberDefinition '[DllImport("kernel32.dll")] public static extern uint SetThreadExecutionState(uint flags);'
        # ES_CONTINUOUS | ES_SYSTEM_REQUIRED
        [Benchmarks.Power]::SetThreadExecutionState([uint32]'0x80000001') | Out-Null
    }

    for ($round = 1; $round -le $Rounds; $round++) {
        # Rotate who goes first, so drift over the run does not favour one build
        $order = for ($i = 0; $i -lt $names.Count; $i++) { $names[($i + $round - 1) % $names.Count] }

        foreach ($name in $order) {
            $artifacts = Join-Path $OutputDirectory "round-$round/$name"
            $arguments = @('--filter') + $Filter + @('--artifacts', $artifacts, '--join')

            if ($Affinity) {
                $arguments += @('--affinity', $Affinity)
            }

            Write-Host "Round $round of ${Rounds}: $name"
            Invoke-Benchmarks $name $arguments | Out-Null

            # Prove from the logs which library this run measured
            $seen = @(Get-ChildItem $artifacts -Recurse -Filter *.log | Select-String $markerPattern | ForEach-Object { $_.Matches[0].Groups[1].Value } | Sort-Object -Unique)

            if (-not $seen) { throw "Round $round of ${name}: the logs name no AngleSharp.Xml module." }

            foreach ($other in $names | Where-Object { $_ -ne $name }) {
                if ($seen -contains $modules[$other]) { throw "Round $round of ${name} measured the $other build's AngleSharp.Xml ($($modules[$other]))." }
            }
        }
    }

    Write-Summary
    Write-Host "Results under $OutputDirectory (round-N/<build>/results)"
}
finally {
    if ($keepAwake -and ('Benchmarks.Power' -as [Type])) {
        # ES_CONTINUOUS alone clears the request
        [Benchmarks.Power]::SetThreadExecutionState([uint32]'0x80000000') | Out-Null
    }

    $env:AngleSharpVersion = $previousAngleSharpVersion
}
