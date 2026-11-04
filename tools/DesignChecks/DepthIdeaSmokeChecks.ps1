# Standalone design-model checks. Does not load Unity or exercise gameplay code.
# The four-unit budget, opportunities and encounter states are proposed fixtures.
$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$script:passed = 0

function Assert-DesignCheck {
    param([bool]$Condition, [string]$Description)
    if (-not $Condition) { throw "FAIL: $Description" }
    $script:passed++
    Write-Output "PASS: $Description"
}

function Read-AuthoredNumber {
    param([string]$RelativePath, [string]$Field)
    $body = Get-Content -LiteralPath (Join-Path $projectRoot $RelativePath) -Raw
    $match = [regex]::Match($body, '(?m)^  ' + [regex]::Escape($Field) + ': ([0-9.]+)\r?$')
    if (-not $match.Success) { throw "Missing numeric field $Field in $RelativePath" }
    return [double]::Parse($match.Groups[1].Value, [Globalization.CultureInfo]::InvariantCulture)
}

function Test-CommitmentFits {
    param([array]$LearnedDevelopments, [int]$Capacity)
    $spent = 0
    foreach ($development in $LearnedDevelopments) { $spent += $development.Cost }
    # This proposed policy counts learned commitments even while inactive.
    return $spent -le $Capacity
}

function Try-ConsumeOpportunity {
    param([hashtable]$Opportunity, [bool]$ActionAccepted, [double]$Now,
        [string]$Target, [int]$Revision)
    if (-not $ActionAccepted -or $Opportunity.Consumed -or $Now -ge $Opportunity.ExpiresAt) { return $false }
    if ($Target -ne $Opportunity.Target -or $Revision -ne $Opportunity.Revision) { return $false }
    $Opportunity.Consumed = $true
    return $true
}

$deepA = @{ Cost = 3; Active = $true }
$support = @{ Cost = 1; Active = $true }
$deepB = @{ Cost = 3; Active = $true }
Assert-DesignCheck ((Test-CommitmentFits @($deepA, $support) 4) -and
    -not (Test-CommitmentFits @($deepA, $deepB) 4)) 'Four-unit fixture permits deep + support, but not two peaks'

$deepA.Active = $false
Assert-DesignCheck (-not (Test-CommitmentFits @($deepA, $deepB) 4)) 'Deactivating a learned peak does not free its commitment'

$opportunity = @{ Target = 'enemy-a'; Revision = 1; ExpiresAt = 2.5; Consumed = $false }
$rejectedPress = Try-ConsumeOpportunity $opportunity $false 1.1 'enemy-a' 1
$preserved = -not $opportunity.Consumed
$acceptedPress = Try-ConsumeOpportunity $opportunity $true 1.2 'enemy-a' 1
$duplicate = Try-ConsumeOpportunity $opportunity $true 1.3 'enemy-a' 1
Assert-DesignCheck (-not $rejectedPress -and $preserved -and $acceptedPress -and -not $duplicate) 'Rejected input preserves opportunity; successful acceptance consumes it once'

$staleOpportunity = @{ Target = 'enemy-a'; Revision = 1; ExpiresAt = 2.5; Consumed = $false }
$wrongTarget = Try-ConsumeOpportunity $staleOpportunity $true 1.1 'enemy-b' 1
$resetTarget = Try-ConsumeOpportunity $staleOpportunity $true 1.2 'enemy-a' 2
$expired = Try-ConsumeOpportunity $staleOpportunity $true 2.6 'enemy-a' 1
Assert-DesignCheck (-not $wrongTarget -and -not $resetTarget -and -not $expired -and
    -not $staleOpportunity.Consumed) 'Wrong target, reset generation and expiry reject the contextual benefit'

$lightWindup = Read-AuthoredNumber 'Assets/Game/Combat/Data/Light.asset' 'Windup'
$lightPoise = Read-AuthoredNumber 'Assets/Game/Combat/Data/Light.asset' 'PoiseDamage'
$breakerWindup = Read-AuthoredNumber 'Assets/Game/Combat/Data/Techniques/Breaker.asset' 'Windup'
$breakerPoise = Read-AuthoredNumber 'Assets/Game/Combat/Data/Techniques/Breaker.asset' 'PoiseDamage'

# Idealized fixtures: both attacks are in reach, unobstructed, and target is not staggered.
# Meeting time + poise conditions is not a claim that an action is optimal in gameplay.
$remainingPoise = 60 - 48
$shortWindow = 0.30
$lightCanBreakInTime = $lightPoise -ge $remainingPoise -and $lightWindup -lt $shortWindow
$breakerCanBreakInTime = $breakerPoise -ge $remainingPoise -and $breakerWindup -lt $shortWindow
Assert-DesignCheck ($lightCanBreakInTime -and -not $breakerCanBreakInTime) 'At 48/60 poise with 0.30 s left, Light meets the break deadline; Breaker does not'

$remainingPoise = 60
$longWindow = 0.80
$lightCanBreakInTime = $lightPoise -ge $remainingPoise -and $lightWindup -lt $longWindow
$breakerCanBreakInTime = $breakerPoise -ge $remainingPoise -and $breakerWindup -lt $longWindow
Assert-DesignCheck ($breakerCanBreakInTime -and -not $lightCanBreakInTime) 'Against fresh 60 poise with 0.80 s left, Breaker can break in one contact; Light cannot'

Write-Output "Completed $script:passed basic design checks. Unity was not invoked."
