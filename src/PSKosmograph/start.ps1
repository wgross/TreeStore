Start-Process `
    -FilePath "C:\Program Files\PowerShell\7-preview\pwsh.exe" `
    -ArgumentList @(
        "-noprofile"
    )
	 
# & "C:\Program Files\PowerShell\7-preview\pwsh.exe" -c "Import-Module ./PsKosmograph.Dll"