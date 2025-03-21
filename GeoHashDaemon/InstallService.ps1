$exePath = "C:\Users\se-gorrose-01\source\labb\dotnetlab\GeoHash\GeoHashDaemon\GeoHashDaemon\bin\Debug\netcoreapp3.1\GeoHashDaemon.exe"
$account = "sveacorp\se-gorrose-01"
$acl = Get-Acl $exePath
$aclRuleArgs = $account, "Read,Write,ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($aclRuleArgs)
$acl.SetAccessRule($accessRule)
$acl | Set-Acl $exePath

New-Service -Name GeoHashDaemon -BinaryPathName $exePath -Credential $account -Description "GeoHashDaemon" -DisplayName "GeoHashDaemon" -StartupType Automatic
