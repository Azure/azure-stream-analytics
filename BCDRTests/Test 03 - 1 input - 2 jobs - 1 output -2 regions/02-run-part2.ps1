Import-Module -Name CosmosDB
######################################################################
#Observing records in Cosmos DB

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

