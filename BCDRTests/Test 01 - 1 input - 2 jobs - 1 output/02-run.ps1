Import-Module -Name CosmosDB
######################################################################
#Gather necessary variables from previous terminal if necessary, and paste them into a new window

Write-Host "`
`$rgName = `"$rgName`"`
`$asaJobName1 = `"$asaJobName1`"`
`$asaJobName2 = `"$asaJobName2`"`
`$ehNamespace = `"$ehNamespace`"`
`$ehName = `"$ehName`"`
`$ehAuthorizationRuleName = `"$ehAuthorizationRuleName`"`
`$ehKey = `"$($ehKey.PrimaryKey)`"`
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

$s = Get-AzureEHSASToken `
-URI "$ehNamespace.servicebus.windows.net/$ehName" `
-AccessPolicyName $ehAuthorizationRuleName `
-AccessPolicyKey $ehKey

while ($True) {
Send-AzureEHDatagram `
-URI "$ehNamespace.servicebus.windows.net/$ehName" `
-SASToken $s `
-Datagram ('{"DeviceId": '+(Get-Random -Maximum 8)+',"readingTimestamp": "'+(Get-Date -Format o)+'", "readingNum":'+(Get-Random -Maximum 1024)+'}')
}

######################################################################
#Observing records in Cosmos DB

##in a new terminal
##if necessary, install https://www.powershellgallery.com/packages/CosmosDB/

$primaryKey = ConvertTo-SecureString -String $cosmosDBAccountKey -AsPlainText -Force
$cosmosDbContext = New-CosmosDbContext -Account $cosmosDBAccountName -Database $cosmosDBDatabaseName -Key $primaryKey

$query = "SELECT * FROM customers c WHERE c.deviceid = '0' ORDER BY c.windowend DESC"
(Get-CosmosDbDocument -Context $cosmosDbContext -CollectionId $cosmosDBContainerName -Query $query) | Select-Object {$_.deviceid_minute,$_.jobname}

## We should obvserve a random alternance of jobnames over time

######################################################################
#Stopping one job

Stop-AzStreamAnalyticsJob -ResourceGroupName $rgName -Name $asaJobName1

## From that point forward, new records will only be associated to the remaining ASA job

$query = "SELECT * FROM customers c WHERE c.deviceid = '0' ORDER BY c.windowend DESC"
(Get-CosmosDbDocument -Context $cosmosDbContext -CollectionId $cosmosDBContainerName -Query $query) | Select-Object {$_.deviceid_minute,$_.jobname}

