[CmdletBinding()]
param(
  [string]$AssemblyInfoPath = "Properties\AssemblyInfo.cs",
  [string]$InstallerPath = "installer\SWAutoAttributes.iss",
  [int]$UtcOffsetHours = 8
)

function Replace-Required {
  param(
    [Parameter(Mandatory = $true)][string]$Content,
    [Parameter(Mandatory = $true)][string]$Pattern,
    [Parameter(Mandatory = $true)][string]$Replacement,
    [Parameter(Mandatory = $true)][string]$Name
  )

  if ($Content -notmatch $Pattern) {
    throw "Cannot find $Name."
  }

  [regex]::Replace($Content, $Pattern, $Replacement, 1)
}

function Write-GitHubOutput {
  param(
    [Parameter(Mandatory = $true)][string]$Name,
    [Parameter(Mandatory = $true)][string]$Value
  )

  if ($env:GITHUB_OUTPUT) {
    "$Name=$Value" | Out-File -FilePath $env:GITHUB_OUTPUT -Encoding utf8 -Append
  }
}

$date = [DateTimeOffset]::UtcNow.ToOffset([TimeSpan]::FromHours($UtcOffsetHours))
$version = $date.ToString("yyyyMMdd")
$assemblyVersion = "{0}.{1}.{2}.0" -f $date.Year, $date.Month, $date.Day

$assemblyInfo = Get-Content $AssemblyInfoPath -Raw
$assemblyInfo = Replace-Required $assemblyInfo 'AssemblyVersion\("[^"]*"\)' "AssemblyVersion(`"$assemblyVersion`")" "AssemblyVersion"
$assemblyInfo = Replace-Required $assemblyInfo 'AssemblyFileVersion\("[^"]*"\)' "AssemblyFileVersion(`"$assemblyVersion`")" "AssemblyFileVersion"
$assemblyInfo = Replace-Required $assemblyInfo 'AssemblyInformationalVersion\("[^"]*"\)' "AssemblyInformationalVersion(`"$version`")" "AssemblyInformationalVersion"
Set-Content -Path $AssemblyInfoPath -Value $assemblyInfo -NoNewline

$installer = Get-Content $InstallerPath -Raw
$installer = Replace-Required $installer '#define AppVersion "[^"]*"' "#define AppVersion `"$version`"" "AppVersion"
$installer = Replace-Required $installer '#define AppFileVersion "[^"]*"' "#define AppFileVersion `"$assemblyVersion`"" "AppFileVersion"
Set-Content -Path $InstallerPath -Value $installer -NoNewline

Write-GitHubOutput "version" $version
Write-GitHubOutput "tag" $version
Write-GitHubOutput "release_name" $version

Write-Host "Build version: $version"
Write-Host "Assembly file version: $assemblyVersion"
