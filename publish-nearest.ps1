<#
.SYNOPSIS
    Generated publish file for github.com/wgross dev workflow in 'dotnet-nearest'.
.PARAMETER Target
    Specifies what to publish (optional)
.PARAMETER BuildConfiguration
    Specifies publish BuildConfigurationuration (optional)
.PARAMETER Destination
    Specifies publish destination (optional)
.PARAMETER FromPath
    Specifies the path from where it is called (mandatory)
#>
[CmdletBinding()]
param(
    [Parameter(Position=0)]
    [ValidateSet($null,"psgallery")]
    [string]$Target,

    [Parameter()]
    [ValidateSet("Debug","Release")]
    [string]$BuildConfiguration = "Debug",

    [Parameter()]
    $Destination,

    [Parameter(Mandatory)]
    [ValidateScript({$_|Test-Path -PathType Container})]
    $FromPath
)    

# make the dotnet cmdlets available by default
Import-Module -Name dotnet-cli-publish
Import-Module -Name PowerShellGet

# react to the publish target
"Target: $Target"|Write-Verbose
switch($Target) {
    "psgallery" {
        if($null -eq $NuGetApiKey) { throw "Nuget api key missing" }

        PowerShellGet\Publish-Module -Path $PSScriptRoot\TreeStore -NuGetApiKey $NuGetApiKey -Verbose
    }
        
    default {
        # publish the module
        $params =@{
            FullName = "$PSScriptRoot\src\TreeStore.PsModule\TreeStore.PsModule.csproj"
            Destination = "$PSScriptRoot\TreeStore"
            PublishConfiguration = $BuildConfiguration
        }
        dotnet-cli-publish\Invoke-DotNetPublish @params -SelectProperties
        
        # create a file catalog from the publishing destination content
        # New-FileCatalog -CatalogFilePath "$PSScriptRoot\TreeStore\TreeStore.cat" -Path "$PSScriptRoot\TreeStore"
    }
}
        
