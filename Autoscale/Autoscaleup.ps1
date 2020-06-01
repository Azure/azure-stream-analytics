$connectionName = "AzureRunAsConnection"

$subId = Get-AutomationVariable -Name 'subId'
$resourceGroupName = Get-AutomationVariable -Name 'resourceGroupName'
$jobName = Get-AutomationVariable -Name 'jobName'
$maxSU = Get-AutomationVariable -Name 'maxSU'
$targetSU = 0
"Executing with subscription id: $subId; resource group name: $resourceGroupName; job name: $jobName; target SU: $maxSU"

$ErrorActionPreference = 'Stop'
function Get-AzureRmCachedAccessToken()
{
    $ErrorActionPreference = 'Stop'
  
    if(-not (Get-Module AzureRm.Profile)) {
        Import-Module AzureRm.Profile
    }
    $azureRmProfileModuleVersion = (Get-Module AzureRm.Profile).Version
    # refactoring performed in AzureRm.Profile v3.0 or later
    if($azureRmProfileModuleVersion.Major -ge 3) {
        $azureRmProfile = [Microsoft.Azure.Commands.Common.Authentication.Abstractions.AzureRmProfileProvider]::Instance.Profile
        if(-not $azureRmProfile.Accounts.Count) {
            Write-Error "Ensure you have logged in before calling this function."    
        }
    } else {
        # AzureRm.Profile < v3.0
        $azureRmProfile = [Microsoft.WindowsAzure.Commands.Common.AzureRmProfileProvider]::Instance.Profile
        if(-not $azureRmProfile.Context.Account.Count) {
            Write-Error "Ensure you have logged in before calling this function."    
        }
    }
  
    $currentAzureContext = Get-AzureRmContext
    $profileClient = New-Object Microsoft.Azure.Commands.ResourceManager.Common.RMProfileClient($azureRmProfile)
    Write-Debug ("Getting access token for tenant" + $currentAzureContext.Tenant.TenantId)
    $token = $profileClient.AcquireAccessToken($currentAzureContext.Tenant.TenantId)
    $token.AccessToken
}

try
{
    # Get the connection "AzureRunAsConnection "
    $servicePrincipalConnection = Get-AutomationConnection -Name $connectionName         

    "Logging in to Azure..."
    Add-AzureRmAccount `
        -ServicePrincipal `
        -TenantId $servicePrincipalConnection.TenantId `
        -ApplicationId $servicePrincipalConnection.ApplicationId `
        -CertificateThumbprint $servicePrincipalConnection.CertificateThumbprint 

}
catch {
    if (!$servicePrincipalConnection)
    {
        $ErrorMessage = "Connection $connectionName not found."
        throw $ErrorMessage
    } else{
        Write-Error -Message $_.Exception
        throw $_.Exception
    }
}

$accessToken = Get-AzureRmCachedAccessToken

$headers = @{
    'Content-Type' = 'application/json'
    'Authorization' = 'Bearer ' + $accessToken
}

$getJobUri = "https://management.azure.com/subscriptions/$subId/resourceGroups/$resourceGroupName/providers/Microsoft.StreamAnalytics/streamingjobs/$jobName" + '?$expand=transformation&api-version=2017-04-01-preview'

$jobState = Invoke-RestMethod -Uri $getJobUri -Method Get -Headers $headers 
$curState = $jobState.properties.jobState
$curSU = $jobState.properties.transformation.properties.streamingUnits
$response = $jobState.properties.transformation.properties.validStreamingUnits | Sort-Object

$lengthArray = $response.Count
$index = $response.IndexOf($curSU)

if($index -lt $lengthArray-1) {
    #there are valid SU options job can scale up to
    if($response[$index+1] -le $maxSU){ $targetSU = $response[$index+1]}
    else {$targetSU = $curSU}
}
else {
    #no valid SU options job can scale up to
    $targetSU = $curSU
}
"Current Job state: $curState; Current SU: $curSU; List of valid SUs: $response; Max SU for autoscale: $maxSU; Target SU to scale up to now: $targetSU"

$scaleUri = "https://management.azure.com/subscriptions/$subId/resourceGroups/$resourceGroupName/providers/Microsoft.StreamAnalytics/streamingjobs/$jobName/scale?api-version=2017-04-01-preview"

$scaleForm = @{
    "streamingUnits" = $targetSU
}

$jsonBody = $scaleForm | ConvertTo-Json
"Sending scale request with new SU: $targetSU"
Invoke-RestMethod -Uri $scaleUri -Method Post -Headers $headers -Body $jsonBody

# Check scale result
Start-Sleep -Seconds 3

$pollingCount = 0
while ($pollingCount -ne 30)
{
  $pollingCount++
  $jobState = Invoke-RestMethod -Uri $getJobUri -Method Get -Headers $headers 
  if ($jobState.properties.jobState.equals("Running") -and $jobState.properties.transformation.properties.streamingUnits -eq $targetSU.ToString()) { break }
  "Waiting for the job to scale."
  Start-Sleep -Seconds 5
}

$jobState = Invoke-RestMethod -Uri $getJobUri -Method Get -Headers $headers 
$finalState = $jobState.properties.jobState
$finalSU = $jobState.properties.transformation.properties.streamingUnits
"Final Job state: $finalState; Final SU: $finalSU"

if ($finalState.equals("Running") -and $finalSU -eq $targetSU.ToString()) { "Scaling has completed." }
else { throw 'Failed to scale the job' } 
