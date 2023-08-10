param (
  [Parameter(Mandatory)] [string] $job,
  [Parameter(Mandatory)] [string] $subscriptionId,
  [string] $eventsPerMinute = $(Read-Host -prompt "eventsPerMinute (default: 60)")
)

$app = $job.Split("-")[0]

if (-not (Test-Path -Path $PSScriptRoot\$app)) {
  $allApps = (Get-ChildItem $PSScriptRoot -Directory | ForEach-Object { $_.BaseName }) -join ', '
  "Application $app not found, you can pick one: $allApps."
  return
}

if (-not (Test-Path -Path $PSScriptRoot\$app\$job)) {
  $allJobs = (Get-ChildItem $PSScriptRoot\$app -Directory | ForEach-Object { $_.BaseName } | Where-Object { -not $_.Contains("DataGenerator") }) -join ', '
  "Job $job not found, you can pick one: $allJobs."
  return
}

if ('' -eq $eventsPerMinute) {
  $eventsPerMinute = 60
}
if (-not ($eventsPerMinute -match '^[0-9]+$')) {
  "Invalid eventsPerMinute $eventsPerMinute, it must be number."
  return
}
$eventsPerMinute = [int] $eventsPerMinute
if ($eventsPerMinute -lt 1) {
  "Invalid eventsPerMinute $eventsPerMinute, it must greater than 0."
  return
}

$isInstallAz = [bool](Get-Command -Name Set-AzContext -ErrorAction SilentlyContinue)
if (-not $isInstallAz) {
  Write-Host "Az Powershell is not installed, please follow the guide to install Az Powershell."
  return
}

if ($null -eq (Get-AzContext)) {
  Write-Host "You are not login Azure, please login by 'Connect-AzAccount'."
  return
}

"Will create Storage Account, Azure Function, Event Hub at subscription: $subscriptionId, and generate $eventsPerMinute events to Event Hub per minute."

$region = "Central US"
$uniqPostfix = (New-Guid).ToString().Replace("-", "").Substring(0, 10)
$resourceGroupName = "$job-rg-$uniqPostfix"
$storageAccountName = "clickstreamsa$uniqPostfix"
$azureFunctionName = "clickstreamfunc$uniqPostfix"
$eventHubName = "clickstreameh$uniqPostfix"

Set-AzContext -Subscription $subscriptionId -ErrorAction Stop | Out-Null
Write-Host "Using subscription:" ((Get-AzContext).Subscription.Name) "Id:" (Get-AzContext).Subscription.Id

try {
  Write-Host -NoNewline "(1/5) Creating Resource Group $resourceGroupName..."
  New-AzResourceGroup -Name $resourceGroupName -Location $region -ErrorAction Stop | Out-Null
  Write-Host -ForegroundColor Green " Done"

  Write-Host -NoNewline "(2/5) Creating Event Hub $eventHubName..."
  New-AzEventHubNamespace -ResourceGroupName $resourceGroupName -Location $region -Name $eventHubName -SkuName Basic -WarningAction Ignore -ErrorAction Stop | Out-Null
  New-AzEventHub -ResourceGroupName $resourceGroupName -NamespaceName $eventHubName -Name click-stream-events -MessageRetentionInDays 1 -PartitionCount 1 -WarningAction Ignore -ErrorAction Stop | Out-Null
  Write-Host -ForegroundColor Green " Done"

  Write-Host -NoNewline "(3/5) Creating Storage Account $storageAccountName..."
  New-AzStorageAccount -ResourceGroupName $resourceGroupName -Name $storageAccountName -SkuName Standard_LRS -Location $region -ErrorAction Stop | Out-Null
  Write-Host -ForegroundColor Green " Done"

  Write-Host -NoNewline "(4/5) Creating Azure Function $azureFunctionName..."
  $eventHubKey = (Get-AzEventHubKey -ResourceGroupName $resourceGroupName -NamespaceName $eventHubName -AuthorizationRuleName RootManageSharedAccessKey -WarningAction Ignore -ErrorAction Stop)
  $appSettings = @{
    "eventsPerMinute" = $eventsPerMinute
    "eventHubConnectionString" = $eventHubKey.PrimaryConnectionString
  }
  New-AzFunctionApp -Name $azureFunctionName -ResourceGroupName $resourceGroupName -StorageAccount $storageAccountName -Runtime dotnet -OSType Windows -FunctionsVersion 4 -RuntimeVersion 6 -Location $region -AppSetting $appSettings -DisableApplicationInsights | Out-Null
  Set-AzResource -ResourceId /subscriptions/$subscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.Web/sites/$azureFunctionName/basicPublishingCredentialsPolicies/ftp -Properties @{"allow" = $true} -Force | Out-Null
  Set-AzResource -ResourceId /subscriptions/$subscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.Web/sites/$azureFunctionName/basicPublishingCredentialsPolicies/scm -Properties @{"allow" = $true} -Force | Out-Null
  Publish-AzWebApp -ResourceGroupName $resourceGroupName -Name $azureFunctionName -ArchivePath (Get-Item $PSScriptRoot\$app\ClickStreamDataGenerator\CodePackage.zip).FullName -Force -ErrorAction Stop | Out-Null
  Write-Host -ForegroundColor Green " Done"

  Write-Host -NoNewline "(5/5) Creating Azure Stream Analytics job '$job'..."
  $storageAccountKey = (Get-AzStorageAccountKey -ResourceGroupName $resourceGroupName -Name $storageAccountName -ErrorAction Stop).Value[0]
  $ctx = New-AzStorageContext -StorageAccountName $storageAccountName -StorageAccountKey $storageAccountKey -ErrorAction Stop
  New-AzStorageContainer -Name job-output -Context $ctx -ErrorAction Stop | Out-Null
  if (Test-Path $PSScriptRoot/$app/$job/user-info.csv) {
    New-AzStorageContainer -Name reference-data -Context $ctx -ErrorAction Stop | Out-Null
    Set-AzStorageBlobContent -Container reference-data -File $PSScriptRoot/$app/$job/user-info.csv -ClientTimeoutPerRequest 10 -Context $ctx -Force -ErrorAction Stop | Out-Null
  }
  $jobParameters = @{
    "InputEventHubName" = $eventHubName
    "InputEventHubKey" = $eventHubKey.PrimaryKey
    "OutputAccountName" = $storageAccountName
    "OutputAccountKey" = $storageAccountKey
    "Region" = $region
  }
  New-AzResourceGroupDeployment -ResourceGroupName $resourceGroupName -TemplateFile $PSScriptRoot\$app\$job\JobARMTemplate.json -TemplateParameterObject $jobParameters -ErrorAction Stop | Out-Null
  Write-Host -ForegroundColor Green " Done"
  Write-Host -ForegroundColor Green "All resources were deployed successfully. Opening Azure portal in browser."

  Start-Process "https://portal.azure.com/#@tenant/resource/subscriptions/$subscriptionId/resourceGroups/$resourceGroupName/overview"
} catch {
  try {
    Write-Host
    Write-Host -ForegroundColor Red "Create resources failed, reason:" $_
    Write-Host -ForegroundColor Red "Will delete resource group '$resourceGroupName'."
    Remove-AzResourceGroup -Name $resourceGroupName -ErrorAction Stop | Out-Null
  } catch {
    Write-Host -ForegroundColor Red "Resource group '$resourceGroupName' delete failed, reason:" $_
  }
}
