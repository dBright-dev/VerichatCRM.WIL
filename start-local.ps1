#requires -Version 5.1
<#
Starts the two .NET tiers of Verichat CRM on a development machine, in the order that
means a sign-in can never fail with "connection refused (localhost:5184)": the API first,
and the web app only once the API reports Healthy.

Both tiers are launched from their own launch profiles, which is what pins the LocalDB
connection string for the API and http://localhost:5184 for the web proxy - so nothing here
needs an environment variable, a user secret, or the hosted API being reachable.

  .\start-local.ps1            API on http://localhost:5184, web on http://localhost:5253
  .\start-local.ps1 -Lan       also lets a phone or emulator on your network reach the API
  .\start-local.ps1 -Stop      stop both tiers from another terminal

-Lan rebinds Kestrel to 0.0.0.0, which exposes the development API to your local network.
Use it only while you are testing, on a network you trust.

Stop everything with Ctrl+C in this window; both tiers are closed with it. If a previous run
was killed in a way that skipped that cleanup, an orphaned app host can still be holding
:5184 and the next start fails to bind - which reads like a database or credential problem
and is neither. Run -Stop to clear it.
#>
[CmdletBinding()]
param(
    [switch]$Lan,
    [switch]$Stop,
    [int]$HealthTimeoutSec = 300
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$healthUrl = 'http://localhost:5184/api/health'
$logDir = Join-Path $root 'logs'
if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Path $logDir | Out-Null }

function Start-Tier {
    param([string]$Name, [string]$Project, [string]$Profile, [string[]]$ExtraArgs = @())

    # `$(if ... else { @() })` yields $null, not an empty array, and Start-Process rejects a
    # null element in ArgumentList - so drop empties here rather than at every call site.
    $cliArgs = @('run', '--project', $Project, '--launch-profile', $Profile) +
        @($ExtraArgs | Where-Object { $_ })

    # Do not wrap these in single quotes and pass one string: the Windows command-line parser
    # strips only double quotes, so dotnet.exe receives the verb literally as 'run' and exits
    # with "dotnet-'run' does not exist". The array form lets Start-Process do the quoting.
    return Start-Process -FilePath 'dotnet' -ArgumentList $cliArgs `
        -WorkingDirectory $root -PassThru -WindowStyle Hidden `
        -RedirectStandardOutput (Join-Path $logDir "$Name.log") `
        -RedirectStandardError (Join-Path $logDir "$Name.err.log")
}

function Read-TierError {
    param([string]$Name)

    foreach ($leaf in @("$Name.err.log", "$Name.log")) {
        $text = Get-Content (Join-Path $logDir $leaf) -Tail 25 -ErrorAction SilentlyContinue
        if ($text) { return $text }
    }
    return $null
}

function Stop-Pids {
    param([int[]]$Pids)

    $targets = @($Pids | Where-Object { $_ } | Select-Object -Unique)
    if ($targets.Count -eq 0) { return }

    # `dotnet run` starts the real app as a direct child, and killing only the CLI leaves the
    # app holding the port. Collect the children before killing the parents, or they are
    # orphaned and can no longer be found by parent id - and match on pid, not on the image
    # name, so an API you started separately from Visual Studio is left alone.
    $children = @()
    foreach ($tid in $targets) {
        $children += @(Get-CimInstance Win32_Process -Filter "ParentProcessId=$tid" `
            -ErrorAction SilentlyContinue | ForEach-Object { $_.ProcessId })
    }

    foreach ($id in ($targets + $children | Where-Object { $_ } | Select-Object -Unique)) {
        Stop-Process -Id $id -Force -ErrorAction SilentlyContinue
    }
}

if ($Stop) {
    # Escape hatch for the case where Ctrl+C never reached this script's finally block: a
    # leftover app host holding :5184 makes the next start fail to bind, which looks like the
    # database or the credentials are broken and is neither. Match only the `dotnet run`
    # commands for these two projects, so unrelated .NET processes survive.
    $owned = @(Get-CimInstance Win32_Process -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -eq 'dotnet.exe' -and $_.CommandLine -match
            'VerichatCRM\.(API|Web).*--launch-profile' })
    if ($owned.Count -eq 0) {
        Write-Host 'Nothing started by this script is running.' -ForegroundColor Cyan
        exit 0
    }
    $owned | ForEach-Object { Write-Host "  stopping pid $($_.ProcessId): $($_.CommandLine)" -ForegroundColor Yellow }
    Stop-Pids -Pids $owned.ProcessId
    Write-Host 'Stopped.' -ForegroundColor Green
    exit 0
}

if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Path $logDir | Out-Null }

function Get-ApiHealth {
    # /api/health answers 503 while Degraded, and Invoke-RestMethod throws on any non-2xx
    # body - which would hide the one field that says why. curl.exe prints the body anyway.
    $body = & curl.exe -s --max-time 5 $healthUrl 2>$null
    if ([string]::IsNullOrWhiteSpace($body)) { return $null }
    try { return $body | ConvertFrom-Json } catch { return $null }
}

Write-Host 'Starting the API (VerichatCRM.API)...' -ForegroundColor Cyan
$apiExtra = @()
if ($Lan) { $apiExtra = @('--urls', 'http://0.0.0.0:5184') }
$api = Start-Tier -Name 'api' -Project 'VerichatCRM.API' -Profile 'http' -ExtraArgs $apiExtra

Write-Host 'Waiting for the API to bind, migrate and report Healthy...' -ForegroundColor Cyan
$deadline = (Get-Date).AddSeconds($HealthTimeoutSec)
$health = $null
$lastDetail = ''
while ((Get-Date) -lt $deadline) {
    $health = Get-ApiHealth
    if ($health) {
        $detail = $health.checks.database.detail
        if ($detail -ne $lastDetail) {
            Write-Host ("  {0}: {1} ({2} ms)" -f $health.checks.database.status, $detail,
                $health.checks.database.latencyMs) -ForegroundColor Yellow
            $lastDetail = $detail
        }
        if ($health.status -eq 'Healthy' -and $health.checks.jwt.status -eq 'Healthy') { break }
    }
    Start-Sleep -Seconds 2
    if ($api.HasExited) { break }
}

if (-not $health -or $health.status -ne 'Healthy') {
    Write-Host ''
    if ($api.HasExited) {
        Write-Host "The API process exited (pid $($api.Id)). Its own output says why:" -ForegroundColor Red
    }
    else {
        Write-Host "The API is still not Healthy after $HealthTimeoutSec seconds (status: $($health.status)). Its output says why:" -ForegroundColor Red
    }
    $reason = Read-TierError -Name 'api'
    if ($reason) { $reason | ForEach-Object { Write-Host "  $_" -ForegroundColor DarkYellow } }
    else { Write-Host '  (no output captured - the process died before writing anything)' -ForegroundColor DarkYellow }

    Write-Host ''
    Write-Host 'The three causes worth checking, in order:' -ForegroundColor Yellow
    Write-Host '  1. LocalDB not running   ->  sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT 1"'
    Write-Host '  2. A stale user secret   ->  dotnet user-secrets remove "ConnectionStrings:DefaultConnection" --project VerichatCRM.API'
    Write-Host '  3. Port 5184 in use      ->  netstat -ano | findstr :5184'
    Write-Host 'DEPLOYMENT.md 7.1, 8 and 9 cover each of these.' -ForegroundColor Yellow
    Stop-Pids -Pids @($api.Id)
    exit 1
}

Write-Host ("  API ready: database {0}, jwt {1}" -f $health.checks.database.status,
    $health.checks.jwt.status) -ForegroundColor Green

Write-Host 'Starting the staff web app (VerichatCRM.Web) - a cold build takes a minute...' -ForegroundColor Cyan
$web = Start-Tier -Name 'web' -Project 'VerichatCRM.Web' -Profile 'http' -ExtraArgs @()

# A cold `dotnet run` spends the first minute building, so the fixed sleep used here printed
# the sign-in URL long before anything was listening on :5253. Wait for the real thing.
$webDeadline = (Get-Date).AddSeconds($HealthTimeoutSec)
$webStatus = '000'
while ((Get-Date) -lt $webDeadline) {
    if ($web.HasExited) { break }
    $webStatus = & curl.exe -s -o /dev/null -w '%{http_code}' --max-time 5 'http://localhost:5253/Account/Login' 2>$null
    if ($webStatus -eq '200') { break }
    Start-Sleep -Seconds 3
}

if ($webStatus -ne '200') {
    Write-Host "The web app never served the sign-in page (last status $webStatus). Its output:" -ForegroundColor Red
    $reason = Read-TierError -Name 'web'
    if ($reason) { $reason | ForEach-Object { Write-Host "  $_" -ForegroundColor DarkYellow } }
    Write-Host 'Check that :5253 is free - netstat -ano | findstr :5253' -ForegroundColor Yellow
    Stop-Pids -Pids @($api.Id, $web.Id)
    exit 1
}

Write-Host '  Web app ready.' -ForegroundColor Green
Write-Host ''
Write-Host '  Sign in at   http://localhost:5253/Account/Login' -ForegroundColor Green
if ($Lan) {
    $ips = (Get-NetIPAddress -AddressFamily IPv4 -ErrorAction SilentlyContinue |
        Where-Object { $_.IPAddress -notlike '169.*' -and $_.IPAddress -ne '127.0.0.1' }).IPAddress -join ', '
    Write-Host "  On the network: http://<your-ip>:5184   (candidates: $ips)" -ForegroundColor Green
    Write-Host '  Android emulator reaches the host as http://10.0.2.2:5184' -ForegroundColor Green
}
Write-Host '  Development accounts: the table in README.md, "Local development".' -ForegroundColor Green
Write-Host "  Live logs: $logDir\api.log and $logDir\web.log" -ForegroundColor DarkGray
Write-Host ''
Write-Host 'Press Ctrl+C here to stop both tiers.' -ForegroundColor DarkGray

try {
    while ($true) {
        Start-Sleep -Seconds 2
        if ($api.HasExited) {
            Write-Host ''
            Write-Host 'The API stopped. Its last output:' -ForegroundColor Red
            $reason = Read-TierError -Name 'api'
            if ($reason) { $reason | ForEach-Object { Write-Host "  $_" -ForegroundColor DarkYellow } }
            break
        }
    }
}
finally {
    Stop-Pids -Pids @($api.Id, $web.Id)
    Write-Host 'Stopped both tiers.' -ForegroundColor Cyan
}
