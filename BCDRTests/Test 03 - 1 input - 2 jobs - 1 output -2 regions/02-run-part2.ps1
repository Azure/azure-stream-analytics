Import-Module -Name CosmosDB
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

