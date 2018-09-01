function projects {
    mkdir Kosmograph|pusd
    dotnet new sln    
    mkdir src|Push-Location

    mkdir Kosmograph.Model|Push-Location                                                 
    dotnet new classlib
    Pop-Location
    
    mkdir Kosmograph.LiteDb|Push-Location                                                 
    dotnet new classlib
    Pop-Location

    Pop-Location
    mkdr test|Push-Location

    mkdir Kosmograph.LiteDb.Test|Push-Location                                                 
    dotnet new xunit                                                       
    dotnet add reference ..\..\src\Kosmograph.Model\Kosmograph.Model.csproj

    Pop-Location
    Pop-Location
    gci *.csproj -Recurse|Foreach-Object{ dotnet sln add $_.fullname}        
}

function git_init {
    git init .                                                             
    gci *.csproj -Recurse|Foreach-Object{ git add $_.fullname }                         
}

function git_ignore {
    @(
        "**/obj/*"
        "**/bin/*"
        ".vs*"
    ) | Out-File -FilePath $PsScriptRoot/.gitignore -Encoding Oem
}

git_ignore