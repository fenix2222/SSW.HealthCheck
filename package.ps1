$OutputDirectory = "NugetPackages\$Version"
$NugetExe = "$PSScriptRoot\.nuget\nuget.exe"

# Get Version
$Version = "1.0.0.0"
$assemblyPattern = "[0-9]+(\.([0-9]+|\*)){1,3}"  
$assemblyVersionPattern = 'AssemblyVersion\("([0-9]+(\.([0-9]+|\*)){1,3})"\)'
$Version = Get-Content "VersionInfo.cs" | select-string -pattern $assemblyVersionPattern | select -first 1 | % { $_.Matches[0].Groups[1].Value }
$count = [Regex]::Matches($Version,"\.").Count
for ($i = $count; $i -lt 3; $i++) 
{
    $Version += ".0"
}

Write-Verbose "Version: $Version"

# Prepare output directory
if (Test-Path $OutputDirectory) {

    Remove-Item $OutputDirectory -Recurse -Force
}
New-Item $OutputDirectory -Type directory -Force

# Package
&($NugetExe) pack ".\SSW.HealthCheck.Infrastructure\SSW.HealthCheck.Infrastructure.csproj" -OutputDirectory $OutputDirectory -Prop Configuration=Release -Build
&($NugetExe) pack ".\SSW.HealthCheck.MVC4\SSW.HealthCheck.Mvc4.csproj"                     -OutputDirectory $OutputDirectory -Prop Configuration=Release -Build
&($NugetExe) pack ".\SSW.HealthCheck\SSW.HealthCheck.Mvc5.csproj"                          -OutputDirectory $OutputDirectory -Prop Configuration=Release -Build
&($NugetExe) pack ".\SSW.HealthCheck.SQLDeploy\SSW.HealthCheck.SQLDeploy.csproj"           -OutputDirectory $OutputDirectory -Prop Configuration=Release -Build
&($NugetExe) pack ".\SSW.HealthCheck.SqlVerify\SSW.HealthCheck.SqlVerify.csproj"           -OutputDirectory $OutputDirectory -Prop Configuration=Release -Build
&($NugetExe) pack ".\SSW.HealthCheck.ExtendedTests\SSW.HealthCheck.ExtendedTests.csproj"           -OutputDirectory $OutputDirectory -Prop Configuration=Release -Build

# Publish
foreach ($pkg in ls "$OutputDirectory\*.nupkg")
{
    $pkgPath = $pkg.FullName
    &($NugetExe) push $pkgPath
}
