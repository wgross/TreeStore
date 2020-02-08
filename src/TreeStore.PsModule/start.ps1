function installed_shell {
    Start-Process `
        -FilePath "C:\Program Files\PowerShell\7-preview\pwsh.exe" `
        -ArgumentList @(
            "-noprofile"
        )
}

function compiled_shell {
    Start-Process `
    -FilePath "D:\src\github\PowerShell\src\powershell-win-core\bin\Debug\netcoreapp3.1\win7-x64\publish\pwsh.exe" `
    -ArgumentList @(
        "-noprofile"
    )
}

# & "C:\Program Files\PowerShell\7-preview\pwsh.exe" -c "Import-Module ./TreeStore.PsModule.Dll"

compiled_shell
