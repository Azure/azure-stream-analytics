Import-Module -Name CosmosDB
######################################################################
#Gather necessary variables from previous terminal if necessary, and paste them into a new window

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

######################################################################
#Starting jobs
Start-AzStreamAnalyticsJob -ResourceGroupName $rgName -Name $asaJobName1
Start-AzStreamAnalyticsJob -ResourceGroupName $rgName -Name $asaJobName2

######################################################################
#Emitting records

##this will block the current terminal
##if necessary, install https://www.powershellgallery.com/packages/Azure.EventHub/

$s1 = Get-AzureEHSASToken `
-URI "$ehNamespace.servicebus.windows.net/$ehName1in" `
-AccessPolicyName $ehAuthorizationRuleName `
-AccessPolicyKey $ehKey

$s2 = Get-AzureEHSASToken `
-URI "$ehNamespace.servicebus.windows.net/$ehName2in" `
-AccessPolicyName $ehAuthorizationRuleName `
-AccessPolicyKey $ehKey

while ($True) {
    $Datagram = '{"DeviceId": '+(Get-Random -Maximum 8)+',"readingTimestamp": "'+(Get-Date -Format o)+'", "readingNum":'+(Get-Random -Maximum 1024)+'}'

    Send-AzureEHDatagram `
    -URI "$ehNamespace.servicebus.windows.net/$ehName1in" `
    -SASToken $s1 `
    -Datagram $Datagram

    Send-AzureEHDatagram `
    -URI "$ehNamespace.servicebus.windows.net/$ehName2in" `
    -SASToken $s2 `
    -Datagram $Datagram
}

######################################################################
#Observing records in EH

##in Service Bus Explorer : https://github.com/paolosalvatori/ServiceBusExplorer
##start 2 consumer group listeners on ehName1out and ehName2out using ehConnectionString
##check that the data is duplicated on each event hub


######################################################################
#Stopping one job
#Stop-AzStreamAnalyticsJob -ResourceGroupName $rgName -Name $asaJobName1
