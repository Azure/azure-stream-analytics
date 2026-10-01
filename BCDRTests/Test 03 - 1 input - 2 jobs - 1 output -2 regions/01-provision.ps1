$location1 = "Canada Central"
$location2 = "Canada East"
$runId = Get-Date -Format "yyyyMMddHHmmss"
$tempFolder = ".\"

######################################################################
# Resource Group
$rgName = "rg-asa-bcdr3-$runId"
New-AzResourceGroup -Name $rgName -Location $location1

######################################################################
# Event hub
$ehNamespace1 = "eh-asa-bcdr3-1-$runId"
$ehNamespace2 = "eh-asa-bcdr3-2-$runId"
$ehNamespaceSku = "Standard"
$ehNamespaceCapacity = 1
$ehName1in = "eh-input-1"
$ehName2in = "eh-input-2"
$ehPartitionCount = 2
$ehAuthorizationRuleName = "eh-asa-bcdr3-$runId"
$ehConsumerGroupName = "cg"

New-AzEventHubNamespace -ResourceGroupName $rgName -Name $ehNamespace1 -Location $location1 -SkuName $ehNamespaceSku -SkuCapacity $ehNamespaceCapacity
New-AzEventHubAuthorizationRule -ResourceGroupName $rgName -NamespaceName $ehNamespace1 -AuthorizationRuleName $ehAuthorizationRuleName -Rights @("Listen","Send","Manage")
$ehKey1 = Get-AzEventHubKey -ResourceGroupName $rgName -Namespace $ehNamespace1 -AuthorizationRuleName $ehAuthorizationRuleName

New-AzEventHub -ResourceGroupName $rgName -NamespaceName $ehNamespace1 -Name $ehName1in -PartitionCount $ehPartitionCount

New-AzEventHubNamespace -ResourceGroupName $rgName -Name $ehNamespace2 -Location $location2 -SkuName $ehNamespaceSku -SkuCapacity $ehNamespaceCapacity
New-AzEventHubAuthorizationRule -ResourceGroupName $rgName -NamespaceName $ehNamespace2 -AuthorizationRuleName $ehAuthorizationRuleName -Rights @("Listen","Send","Manage")
$ehKey2 = Get-AzEventHubKey -ResourceGroupName $rgName -Namespace $ehNamespace2 -AuthorizationRuleName $ehAuthorizationRuleName
New-AzEventHub -ResourceGroupName $rgName -NamespaceName $ehNamespace2 -Name $ehName2in -PartitionCount $ehPartitionCount

New-AzEventHubConsumerGroup -ResourceGroupName $rgName -NamespaceName $ehNamespace1 -EventHubName $ehName1in -Name $ehConsumerGroupName
New-AzEventHubConsumerGroup -ResourceGroupName $rgName -NamespaceName $ehNamespace2 -EventHubName $ehName2in -Name $ehConsumerGroupName

######################################################################
# Cosmos DB
$cosmosDBAccountName = "cosmos-asa-bcdr-$runId"
$cosmosDBDatabaseName = "cosmosDB-asa-bcdr-$runId"
$cosmosDBContainerName = "container"
$cosmosDBPartitionKey = "/deviceid"
$cosmosDBPartitionKeyKind = "Hash"
$cosmosDBUniqueKey = "/deviceid_minute"

New-AzCosmosDBAccount -ResourceGroupName $rgName -Name $cosmosDBAccountName -Location $location1
$cosmosDBAccountKey = Get-AzCosmosDBAccountKey -ResourceGroupName $rgName -Name $cosmosDBAccountName


New-AzCosmosDBSqlDatabase -ResourceGroupName $rgName -AccountName $cosmosDBAccountName -Name $cosmosDBDatabaseName
$cosmosDBUniqueKeyPolicy = New-AzCosmosDBSqlUniqueKeyPolicy -UniqueKey (New-AzCosmosDBSqlUniqueKey -Path $cosmosDBUniqueKey)
New-AzCosmosDBSqlContainer -ResourceGroupName $rgName -AccountName $cosmosDBAccountName -DatabaseName $cosmosDBDatabaseName -Name $cosmosDBContainerName -PartitionKeyPath $cosmosDBPartitionKey -PartitionKeyKind $cosmosDBPartitionKeyKind -UniqueKeyPolicy $cosmosDBUniqueKeyPolicy


######################################################################
# Stream Analytics Jobs 1 and 2
$asaJobName1 = "asa-asa-bcdr2-1-$runId"
$asaJobName2 = "asa-asa-bcdr2-2-$runId"
$asaJobSKU = "Standard"
$asaInputName = "ehInput"
$asaOutputName = "cosmosOutput"
$asaJobStreamingUnit = 3
$asaJobQuery1 = "SELECT CAST(DeviceId AS NVARCHAR(MAX)) AS deviceid, COUNT(*) AS count15, SYSTEM.TIMESTAMP() AS windowend, CONCAT(CAST(DeviceId AS NVARCHAR(MAX)),'_',SUBSTRING(SYSTEM.TIMESTAMP(),1,16)) AS deviceid_minute, '$asaJobName1' AS jobname INTO $asaOutputName FROM $asaInputName GROUP BY DeviceId, HOPPING(second,60,10)"
$asaJobQuery2 = "SELECT CAST(DeviceId AS NVARCHAR(MAX)) AS deviceid, COUNT(*) AS count15, SYSTEM.TIMESTAMP() AS windowend, CONCAT(CAST(DeviceId AS NVARCHAR(MAX)),'_',SUBSTRING(SYSTEM.TIMESTAMP(),1,16)) AS deviceid_minute, '$asaJobName2' AS jobname INTO $asaOutputName FROM $asaInputName GROUP BY DeviceId, HOPPING(second,60,10)"

New-AzStreamAnalyticsJob -ResourceGroupName $rgName -Name $asaJobName1 -Location $location1 -SkuName $asaJobSKU
New-AzStreamAnalyticsJob -ResourceGroupName $rgName -Name $asaJobName2 -Location $location2 -SkuName $asaJobSKU


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
                serviceBusNamespace = $ehNamespace1
                sharedAccessPolicyName = $ehAuthorizationRuleName
                sharedAccessPolicyKey = $ehKey1.PrimaryKey
                authenticationMode = "ConnectionString"
                eventHubName = $ehName1in
                consumerGroupName = $ehConsumerGroupName
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
                serviceBusNamespace = $ehNamespace2
                sharedAccessPolicyName = $ehAuthorizationRuleName
                sharedAccessPolicyKey = $ehKey2.PrimaryKey
                authenticationMode = "ConnectionString"
                eventHubName = $ehName2in
                consumerGroupName = $ehConsumerGroupName
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
