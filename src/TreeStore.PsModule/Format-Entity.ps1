$entityExample = [PsCustomObject]@{
    Id = (new-guid)
    Name = "name"
    Properties = @("a","b","c")
    "t" = [PSCustomObject]@{
        p = "test"
        long_property = 1
        datetime_property = (Get-Date)
        decimal_property = ([decimal]2)
    }
    long_tag_name = [PSCustomObject]@{
        very_long_string = "PsModulePath: C:\Users\msn\Documents\PowerShell\Modules;C:\Program Files\PowerShell\Modules;c:\program files\powershell\7\Modules;C:\Program Files\WindowsPowerShell\Modules;C:\WINDOWS\system32\WindowsPowerShell\v1.0\Modules;C:\Users\msn\Documents\PowerShell\Modules;C:\Users\msn\Documents\WindowsPowerShellModules;c:\Users\msn\.vscode\extensions\ms-vscode.powershell-2020.3.0\modules;C:\Users\msn\Documents\PowerShell\Modules;C:\Users\msn\Documents\WindowsPowerShellModules"
        p = "name"
    }
}

if($PSEdition -eq "Core") {
    $hiOn = [System.Management.Automation.VTUtility]::GetEscapeSequence([System.ConsoleColor]::White)
    $hiOff = [System.Management.Automation.VTUtility]::GetEscapeSequence([System.ConsoleColor]::Gray)
} else {
    $hiOn = $hiOff = [string]::Empty
}

function Show-Entity($entity) {
    begin{
        $builder = [System.Text.StringBuilder]::new()
    }
    process{
        # start with the entity name
        $builder = $builder.Append($hiOn).Append($entity.Name).AppendLine($hiOff)
        
        # evaluate the max length of the property names
        $entityProperties = $entity.PSObject.Properties
        $maxEntityPropertyLength = 0
        foreach($eProp in $entityProperties) { 
            $maxEntityPropertyLength = [Math]::Max($maxEntityPropertyLength, $eProp.Name.Length)
        }

        # print all properties indented
        $entityIndent = "  "
        foreach($eProp in $entityProperties) {
            $builder = $builder.Append($entityIndent).Append($hiOn).Append($eProp.Name.PadRight($maxEntityPropertyLength)).Append($hiOff)
            if($eProp.Value -is [Guid]) {

                # guids seem to be special
                $builder = $builder.Append(" : ").AppendLine($eProp.Value)

            } elseif($eProp.Value -is [psobject]) {

                # for tag end the line after the name
                $builder = $builder.AppendLine()

                # for tag properties format nicely with more indent
                $tagProperties = $eProp.Value.PSObject.Properties 
                $maxEntityPropertyLength = 0
                foreach($tProp in $tagProperties) {
                    $maxTagPropertyLength = [Math]::Max($maxTagPropertyLength, $tProp.Name.Length)
                } 
                $tagIndent = "      "
                foreach($tProp in $tagProperties) {
                    # convert to string safely
                    $value = $null; 
                    if([System.Management.Automation.LanguagePrimitives]::TryConvertTo($tProp.Value, [string], [ref]$value)) {
                        $builder = $builder.Append($tagIndent).Append($hiOn).Append($tProp.Name.PadRight($maxTagPropertyLength)).Append($hiOff).Append(" : ").AppendLine($value)
                    } else {
                        # if conversion isn't possible, pront placeholder
                        $builder = $builder.Append($tagIndent).Append($hiOn).Append($tProp.Name.PadRight($maxTagPropertyLength)).Append($hiOff).Append(" : ").AppendLine("???")
                    }
                }
                $builder = $builder.AppendLine()
            } else {

                # for non-tag properties just write the value
                $builder = $builder.Append(" : ").AppendLine($eProp.Value)
            }
        }
        $builder.ToString()
    }
}

Show-Entity $entityExample
