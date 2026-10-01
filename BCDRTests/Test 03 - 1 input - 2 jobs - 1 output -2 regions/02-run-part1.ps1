Import-Module -Name CosmosDB
######################################################################
#Gather necessary variables from previous terminal if necessary, and paste them into a new window

Write-Host "`
`$rgName = `"$rgName`"`
`$asaJobName1 = `"$asaJobName1`"`
`$asaJobName2 = `"$asaJobName2`"`
`$ehNamespace1 = `"$ehNamespace1`"`
`$ehNamespace2 = `"$ehNamespace2`"`
`$ehName1in = `"$ehName1in`"`
`$ehName2in = `"$ehName2in`"`
`$ehAuthorizationRuleName = `"$ehAuthorizationRuleName`"`
`$ehKey1 = `"$($ehKey1.PrimaryKey)`"`
`$ehConnectionString1 = `"$($ehKey1.PrimaryConnectionString)`"`
`$ehKey2 = `"$($ehKey2.PrimaryKey)`"`
`$ehConnectionString2 = `"$($ehKey2.PrimaryConnectionString)`"`
`$cosmosDBAccountKey = `"$($cosmosDBAccountKey.PrimaryMasterKey)`"`
`$cosmosDBAccountName = `"$cosmosDBAccountName`"`
`$cosmosDBDatabaseName = `"$cosmosDBDatabaseName`"`
`$cosmosDBContainerName = `"$cosmosDBContainerName`""

######################################################################
#Starting jobs
Start-AzStreamAnalyticsJob -ResourceGroupName $rgName -Name $asaJobName1
Start-AzStreamAnalyticsJob -ResourceGroupName $rgName -Name $asaJobName2

######################################################################
#Emitting records

##this will block the current terminal
##if necessary, install https://www.powershellgallery.com/packages/Azure.EventHub/

$s1 = Get-AzureEHSASToken `
-URI "$ehNamespace1.servicebus.windows.net/$ehName1in" `
-AccessPolicyName $ehAuthorizationRuleName `
-AccessPolicyKey $ehKey1

$s2 = Get-AzureEHSASToken `
-URI "$ehNamespace2.servicebus.windows.net/$ehName2in" `
-AccessPolicyName $ehAuthorizationRuleName `
-AccessPolicyKey $ehKey2

while ($True) {
    $Datagram = '{"DeviceId": '+(Get-Random -Maximum 8)+',"readingTimestamp": "'+(Get-Date -Format o)+'", "readingNum":'+(Get-Random -Maximum 1024)+'}'

    Send-AzureEHDatagram `
    -URI "$ehNamespace1.servicebus.windows.net/$ehName1in" `
    -SASToken $s1 `
    -Datagram $Datagram

    Send-AzureEHDatagram `
    -URI "$ehNamespace2.servicebus.windows.net/$ehName2in" `
    -SASToken $s2 `
    -Datagram $Datagram
}


