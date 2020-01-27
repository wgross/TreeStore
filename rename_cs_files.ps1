filter rename_filename {
    $newName = $_ -replace "kosmograph","TreeStore"
    git mv $($_.FullName) $newname
}

Set-Location $PSScriptRoot
$csFiles = Get-ChildItem *.cs -Recurse | Where-Object Name -like "*Kosmograph*"
$csFiles|rename_filename
