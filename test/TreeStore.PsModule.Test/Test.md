# Start shell

D:\src\github\PowerShell\src\powershell-win-core\bin\Debug\netcoreapp3.1\win7-x64\publish\pwsh.exe

ipmo .\TreeStore.PsModule.dll

$PID

New-psdrive -name "tree" -PsProvider "Kosmograph" -Root "c:\tmp"

cd tree:

New-Item .\Tags\t
