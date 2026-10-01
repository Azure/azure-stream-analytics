$location = "Canada Central"
$runId = Get-Date -Format "yyyyMMddHHmmss"
$tempFolder = ".\"

######################################################################
# Resource Group
$rgName = "rg-asa-bcdr-$runId"
New-AzResourceGroup -Name $rgName -Location $location

######################################################################
# Event hub
$ehNamespace = "eh-asa-bcdr-$runId"
$ehNamespaceSku = "Standard"
$ehNamespaceCapacity = 1
$ehName = "eh-input"
$ehPartitionCount = 2
$ehAuthorizationRuleName = "eh-asa-bcdr-$runId"
$ehConsumerGroupName1 = "cg1"
$ehConsumerGroupName2 = "cg2"

New-AzEventHubNamespace -ResourceGroupName $rgName -Name $ehNamespace -Location $location -SkuName $ehNamespaceSku -SkuCapacity $ehNamespaceCapacity
New-AzEventHubAuthorizationRule -ResourceGroupName $rgName -NamespaceName $ehNamespace -AuthorizationRuleName $ehAuthorizationRuleName -Rights @("Listen","Send")
$ehKey = Get-AzEventHubKey -ResourceGroupName $rgName -Namespace $ehNamespace -AuthorizationRuleName $ehAuthorizationRuleName

New-AzEventHub -ResourceGroupName $rgName -NamespaceName $ehNamespace -Name $ehName -PartitionCount $ehPartitionCount
New-AzEventHubConsumerGroup -ResourceGroupName $rgName -NamespaceName $ehNamespace -EventHubName $ehName -Name $ehConsumerGroupName1
New-AzEventHubConsumerGroup -ResourceGroupName $rgName -NamespaceName $ehNamespace -EventHubName $ehName -Name $ehConsumerGroupName2

######################################################################
# Cosmos DB
$cosmosDBAccountName = "cosmos-asa-bcdr-$runId"
$cosmosDBDatabaseName = "cosmosDB-asa-bcdr-$runId"
$cosmosDBContainerName = "container"
$cosmosDBPartitionKey = "/deviceid"
$cosmosDBPartitionKeyKind = "Hash"
$cosmosDBUniqueKey = "/deviceid_minute"

New-AzCosmosDBAccount -ResourceGroupName $rgName -Name $cosmosDBAccountName -Location $location
$cosmosDBAccountKey = Get-AzCosmosDBAccountKey -ResourceGroupName $rgName -Name $cosmosDBAccountName


New-AzCosmosDBSqlDatabase -ResourceGroupName $rgName -AccountName $cosmosDBAccountName -Name $cosmosDBDatabaseName
$cosmosDBUniqueKeyPolicy = New-AzCosmosDBSqlUniqueKeyPolicy -UniqueKey (New-AzCosmosDBSqlUniqueKey -Path $cosmosDBUniqueKey)
New-AzCosmosDBSqlContainer -ResourceGroupName $rgName -AccountName $cosmosDBAccountName -DatabaseName $cosmosDBDatabaseName -Name $cosmosDBContainerName -PartitionKeyPath $cosmosDBPartitionKey -PartitionKeyKind $cosmosDBPartitionKeyKind -UniqueKeyPolicy $cosmosDBUniqueKeyPolicy

######################################################################
# Stream Analytics Jobs
$asaJobName1 = "asa-asa-bcdr-1-$runId"
$asaJobName2 = "asa-asa-bcdr-2-$runId"
$asaJobSKU = "Standard"
$asaInputName = "ehInput"
$asaOutputName = "cosmosOutput"
$asaJobStreamingUnit = 3
$asaJobQuery1 = "SELECT CAST(DeviceId AS NVARCHAR(MAX)) AS deviceid, COUNT(*) AS count15, SYSTEM.TIMESTAMP() AS windowend, CONCAT(CAST(DeviceId AS NVARCHAR(MAX)),'_',SUBSTRING(SYSTEM.TIMESTAMP(),1,16)) AS deviceid_minute, '$asaJobName1' AS jobname INTO $asaOutputName FROM $asaInputName GROUP BY DeviceId, HOPPING(second,60,10)"
$asaJobQuery2 = "SELECT CAST(DeviceId AS NVARCHAR(MAX)) AS deviceid, COUNT(*) AS count15, SYSTEM.TIMESTAMP() AS windowend, CONCAT(CAST(DeviceId AS NVARCHAR(MAX)),'_',SUBSTRING(SYSTEM.TIMESTAMP(),1,16)) AS deviceid_minute, '$asaJobName2' AS jobname INTO $asaOutputName FROM $asaInputName GROUP BY DeviceId, HOPPING(second,60,10)"


New-AzStreamAnalyticsJob -ResourceGroupName $rgName -Name $asaJobName1 -Location $location -SkuName $asaJobSKU
New-AzStreamAnalyticsJob -ResourceGroupName $rgName -Name $asaJobName2 -Location $location -SkuName $asaJobSKU

$asaInputProperties1 = @{
    properties = @{
        type = "Stream"
        serialization = @{
            type = "Json"
            properties = @{
                encoding = "UTF8"
            }
        }
        compression = @{
            type = "None"
        }
        datasource = @{
            type = "Microsoft.EventHub/EventHub"
            properties = @{
                serviceBusNamespace = $ehNamespace
                sharedAccessPolicyName = $ehAuthorizationRuleName
                sharedAccessPolicyKey = $ehKey.PrimaryKey
                authenticationMode = "ConnectionString"
                eventHubName = $ehName
                consumerGroupName = $ehConsumerGroupName1
            }
        }
    }
}
$asaInputProperties1 | ConvertTo-JSON -depth 4 | Out-File -FilePath "$($tempFolder)tempInput1.json"
New-AzStreamAnalyticsInput -ResourceGroupName $rgName -JobName $asaJobName1 -Name $asaInputName -File "$($tempFolder)tempInput1.json"
Remove-Item -Path "$($tempFolder)tempInput1.json"

$asaInputProperties2 = @{
    properties = @{
        type = "Stream"
        serialization = @{
            type = "Json"
            properties = @{
                encoding = "UTF8"
            }
        }
        compression = @{
            type = "None"
        }
        datasource = @{
            type = "Microsoft.EventHub/EventHub"
            properties = @{
                serviceBusNamespace = $ehNamespace
                sharedAccessPolicyName = $ehAuthorizationRuleName
                sharedAccessPolicyKey = $ehKey.PrimaryKey
                authenticationMode = "ConnectionString"
                eventHubName = $ehName
                consumerGroupName = $ehConsumerGroupName2
            }
        }
    }
}
$asaInputProperties2 | ConvertTo-JSON -depth 4 | Out-File -FilePath "$($tempFolder)tempInput2.json"
New-AzStreamAnalyticsInput -ResourceGroupName $rgName -JobName $asaJobName2 -Name $asaInputName -File "$($tempFolder)tempInput2.json"
Remove-Item -Path "$($tempFolder)tempInput2.json"


$asaOutputProperties = @{
    properties = @{
        datasource = @{
            type = "Microsoft.Storage/DocumentDB"
            properties = @{
                accountId = $cosmosDBAccountName
                accountKey = $cosmosDBAccountKey.PrimaryMasterKey
                collectionNamePattern = $cosmosDBContainerName
                database = $cosmosDBDatabaseName
                documentId = $cosmosDBUniqueKey.replace("/","")
                partitionKey = $cosmosDBPartitionKey.replace("/","")
            }
        }
    }
}

$asaOutputProperties | ConvertTo-JSON -depth 4 | Out-File -FilePath "$($tempFolder)tempOutput.json"

New-AzStreamAnalyticsOutput  -ResourceGroupName $rgName -JobName $asaJobName1 -Name $asaOutputName -File "$($tempFolder)tempOutput.json"
New-AzStreamAnalyticsOutput  -ResourceGroupName $rgName -JobName $asaJobName2 -Name $asaOutputName -File "$($tempFolder)tempOutput.json"
Remove-Item -Path "$($tempFolder)tempOutput.json"

New-AzStreamAnalyticsTransformation -ResourceGroupName $rgName -JobName $asaJobName1 -Name $asaJobName1 -StreamingUnit $asaJobStreamingUnit -Query $asaJobQuery1
New-AzStreamAnalyticsTransformation -ResourceGroupName $rgName -JobName $asaJobName2 -Name $asaJobName2 -StreamingUnit $asaJobStreamingUnit -Query $asaJobQuery2

######################################################################
#Gather necessary variables, paste results into next session

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
