
<#
.SYNOPSIS
    Generated publish file for github.com/wgross dev workflow in 'dotnet-nearest'.
.PARAMETER Target
    Specifies what to publish (optional)
.PARAMETER Config
    Specifies publish configuration (optional)
.PARAMETER Destination
    Specifies publish destination (optional)
.PARAMETER FromPath
    Specifies the path from where it is called (mandatory)
#>
param(
    [Parameter(Position=0)]
    [string]$Target,

    [Parameter()]
    [ValidateRange("Debug","Release")]
    [string]$Config,

    [Parameter()]
    $Destination,

    [Parameter(Mandatory)]
    [ValidateScript({$_|Test-Path -PathType Container})]
    $FromPath
)    

# make the dotnet cmdlets available by default
Import-Module -Name dotnet

# react to the publish target
switch($Target) {
    default {
        Invoke-DotNetPublish -Project "$PSScriptRoot\src\TreeStore.PsModule\TreeStore.PsModule.csproj"
    }
}
        
