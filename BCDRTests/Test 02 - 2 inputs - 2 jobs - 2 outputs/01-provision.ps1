$location = "Canada Central"
$runId = Get-Date -Format "yyyyMMddHHmmss"
$tempFolder = ".\"

######################################################################
# Resource Group
$rgName = "rg-asa-bcdr2-$runId"
New-AzResourceGroup -Name $rgName -Location $location

######################################################################
# Event hub
$ehNamespace = "eh-asa-bcdr2-$runId"
$ehNamespaceSku = "Standard"
$ehNamespaceCapacity = 1
$ehName1in = "eh-input-1"
$ehName2in = "eh-input-2"
$ehName1out = "eh-output-1"
$ehName2out = "eh-output-2"
$ehPartitionCount = 2
$ehAuthorizationRuleName = "eh-asa-bcdr2-$runId"
$ehConsumerGroupName = "cg"

New-AzEventHubNamespace -ResourceGroupName $rgName -Name $ehNamespace -Location $location -SkuName $ehNamespaceSku -SkuCapacity $ehNamespaceCapacity
New-AzEventHubAuthorizationRule -ResourceGroupName $rgName -NamespaceName $ehNamespace -AuthorizationRuleName $ehAuthorizationRuleName -Rights @("Listen","Send","Manage")
$ehKey = Get-AzEventHubKey -ResourceGroupName $rgName -Namespace $ehNamespace -AuthorizationRuleName $ehAuthorizationRuleName

New-AzEventHub -ResourceGroupName $rgName -NamespaceName $ehNamespace -Name $ehName1in -PartitionCount $ehPartitionCount
New-AzEventHub -ResourceGroupName $rgName -NamespaceName $ehNamespace -Name $ehName2in -PartitionCount $ehPartitionCount
New-AzEventHub -ResourceGroupName $rgName -NamespaceName $ehNamespace -Name $ehName1out -PartitionCount $ehPartitionCount
New-AzEventHub -ResourceGroupName $rgName -NamespaceName $ehNamespace -Name $ehName2out -PartitionCount $ehPartitionCount
New-AzEventHubConsumerGroup -ResourceGroupName $rgName -NamespaceName $ehNamespace -EventHubName $ehName1in -Name $ehConsumerGroupName
New-AzEventHubConsumerGroup -ResourceGroupName $rgName -NamespaceName $ehNamespace -EventHubName $ehName2in -Name $ehConsumerGroupName
New-AzEventHubConsumerGroup -ResourceGroupName $rgName -NamespaceName $ehNamespace -EventHubName $ehName1out -Name $ehConsumerGroupName
New-AzEventHubConsumerGroup -ResourceGroupName $rgName -NamespaceName $ehNamespace -EventHubName $ehName2out -Name $ehConsumerGroupName

######################################################################
# Stream Analytics Jobs 1 and 2
$asaJobName1 = "asa-asa-bcdr2-1-$runId"
$asaJobName2 = "asa-asa-bcdr2-2-$runId"
$asaJobSKU = "Standard"
$asaInputName = "ehInput"
$asaOutputName = "ehOutput"
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
                serviceBusNamespace = $ehNamespace
                sharedAccessPolicyName = $ehAuthorizationRuleName
                sharedAccessPolicyKey = $ehKey.PrimaryKey
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

$asaOutputProperties1 = @{
    properties = @{
        datasource = @{
            type = "Microsoft.ServiceBus/EventHub"
            properties = @{
                serviceBusNamespace = $ehNamespace
                sharedAccessPolicyName = $ehAuthorizationRuleName
                sharedAccessPolicyKey = $ehKey.PrimaryKey
                eventHubName = $ehName1out
                partitionKey = "partitionId"
            }
        }
        serialization = @{
            type = "Json"
            properties = @{
                encoding = "UTF8"
                format = "Array"
            }
        }
    }
}
$asaOutputProperties1 | ConvertTo-JSON -depth 4 | Out-File -FilePath "$($tempFolder)tempOutput1.json"
New-AzStreamAnalyticsOutput  -ResourceGroupName $rgName -JobName $asaJobName1 -Name $asaOutputName -File "$($tempFolder)tempOutput1.json"
Remove-Item -Path "$($tempFolder)tempOutput1.json"

$asaOutputProperties2 = @{
    properties = @{
        datasource = @{
            type = "Microsoft.ServiceBus/EventHub"
            properties = @{
                serviceBusNamespace = $ehNamespace
                sharedAccessPolicyName = $ehAuthorizationRuleName
                sharedAccessPolicyKey = $ehKey.PrimaryKey
                eventHubName = $ehName2out
                partitionKey = "partitionId"
            }
        }
        serialization = @{
            type = "Json"
            properties = @{
                encoding = "UTF8"
                format = "Array"
            }
        }
    }
}
$asaOutputProperties2 | ConvertTo-JSON -depth 4 | Out-File -FilePath "$($tempFolder)tempOutput2.json"
New-AzStreamAnalyticsOutput  -ResourceGroupName $rgName -JobName $asaJobName2 -Name $asaOutputName -File "$($tempFolder)tempOutput2.json"
Remove-Item -Path "$($tempFolder)tempOutput2.json"

New-AzStreamAnalyticsTransformation -ResourceGroupName $rgName -JobName $asaJobName1 -Name $asaJobName1 -StreamingUnit $asaJobStreamingUnit -Query $asaJobQuery1
New-AzStreamAnalyticsTransformation -ResourceGroupName $rgName -JobName $asaJobName2 -Name $asaJobName2 -StreamingUnit $asaJobStreamingUnit -Query $asaJobQuery2

######################################################################
#Gather necessary variables, paste results into next session

Write-Host "`
`$rgName = `"$rgName`"`
`$asaJobName1 = `"$asaJobName1`"`
`$asaJobName2 = `"$asaJobName2`"`
`$ehNamespace = `"$ehNamespace`"`
`$ehName1in = `"$ehName1in`"`
`$ehName2in = `"$ehName2in`"`
`$ehName1out = `"$ehName1out`"`
`$ehName2out = `"$ehName2out`"`
`$ehAuthorizationRuleName = `"$ehAuthorizationRuleName`"`
`$ehKey = `"$($ehKey.PrimaryKey)`"`
`$ehConnectionString = `"$($ehKey.PrimaryConnectionString)`""
