
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
param(
    [Parameter(Position=0)]
    [string]$Target,

    [Parameter(Mandatory)]
    [ValidateScript({$_|Test-Path -PathType Container})]
    $FromPath
)

Import-Module -Name dotnet-files
Import-Module -Name dotnet-cli-test

 # add meta data to the pipe object
 filter add_test_script {
    $_|Add-Member -PassThru -NotePropertyMembers @{
        TestScript = $PSCommandPath
        FromPath = $FromPath
    }
}

# start build relative to the scripts directory
$PSScriptRoot|Push-Location
try {
    # react to the publish target
    switch($Target) {
        default {
            @(
                "test/TreeStore.LiteDb.Test"
                "test/TreeStore.Messaging.Test"
                "test/TreeStore.Model.Test"
                "test/TreeStore.PsModule.Test"
            ) | Get-DotNetProjectItem | Invoke-DotNetTest -SelectProperties
        }
    }
} finally {
    Pop-Location
}
        
