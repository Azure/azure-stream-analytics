# Input bindings are passed in via param block.
Param($Timer)

# Stop on error
$ErrorActionPreference = 'stop'

# Write an information log with the current time.
$currentUTCtime = (Get-Date).ToUniversalTime()
Write-Host "asaRobotPause - PowerShell timer trigger function is starting at time: $currentUTCtime"

# Set variables
$metricName = "InputEventsSourcesBacklogged"
$restartThresholdMinute = $env:restartThresholdMinute
$stopThresholdMinute = $env:stopThresholdMinute

$subscriptionId = $env:subscriptionId
$resourceGroupName = $env:resourceGroupName
$asaJobName = $env:asaJobName

$resourceId = "/subscriptions/$($subscriptionId )/resourceGroups/$($resourceGroupName)/providers/Microsoft.StreamAnalytics/streamingjobs/$($asaJobName)"

# Check if managed identity has been enabled and granted access to a subscription, resource group, or resource
$AzContext = Get-AzContext -ErrorAction SilentlyContinue
if (-not $AzContext.Subscription.Id)
{
    Throw ("Managed identity is not enabled for this app or it has not been granted access to any Azure resources. Please see https://docs.microsoft.com/en-us/azure/app-service/overview-managed-identity for additional details.")
}

# Check current ASA job status
$currentJobState = Get-AzStreamAnalyticsJob  -ResourceGroupName $resourceGroupName -Name $asaJobName | Foreach-Object {$_.JobState}
Write-Output "asaRobotPause - Job $($asaJobName) is $($currentJobState)."

try
{
    # Switch state
    if ($currentJobState -eq "Running")
    {
        # Get-AzMetric issues warnings about deprecation coming in future releases, here we ignore them via -WarningAction Ignore
        $currentMetricValues = Get-AzMetric -ResourceId $resourceId -TimeGrain 00:01:00 -MetricName $metricName -DetailedOutput -WarningAction Ignore

        # Metric are always lagging 1-3 minutes behind, so grabbing the last N minutes means checking N+3 actually. This may be overly safe and fined tune down per job.
        $lastMetricValues = $currentMetricValues.Data | Sort-Object -Property Timestamp -Descending | Select-Object -First $stopThresholdMinute | Measure-Object -Sum Maximum
        $lastMetricValue = $lastMetricValues.Sum

        Write-Output "asaRobotPause - Job $($asaJobName) is running with a sum of $($lastMetricValue) backlogged events over the last $($stopThresholdMinute) minutes."

        # -eq for equal
        if ($lastMetricValue -eq 0)
        {
            Write-Output "asaRobotPause - Job $($asaJobName) is stopping..."
            Stop-AzStreamAnalyticsJob -ResourceGroupName $resourceGroupName -Name $asaJobName
        }
        else {
            Write-Output "asaRobotPause - Job $($asaJobName) is not stopping yet, it needs to have 0 backlogged events over the last $($stopThresholdMinute) minutes."
        }
    }

    elseif ($currentJobState -eq "Stopped")
    {
        # Get-AzActivityLog issues warnings about deprecation coming in future releases, here we ignore them via -WarningAction Ignore
        # We check in 1000 record of history, to make sure we're not missing what we're looking for. It may need adjustment for a job that has a lot of logging happening.
        # There is a bug in Get-AzActivityLog that triggers an error when Select-Object First is in the same pipeline (on the same line). We move it down.
        $stopTimeStamp = Get-AzActivityLog -ResourceId $resourceId -MaxRecord 1000 -WarningAction Ignore | Where-Object {$_.EventName.Value -like "Stop Job*"}
        $stopTimeStamp = $stopTimeStamp | Select-Object -First 1 | Foreach-Object {$_.EventTimeStamp}

        # Get-Date returns a local time, we project it to the same time zone (universal) as the result of Get-AzActivityLog that we extracted above
        $minutesSinceStopped = ((Get-Date).ToUniversalTime()- $stopTimeStamp).TotalMinutes

        # -ge for greater or equal
        if ($minutesSinceStopped -ge $restartThresholdMinute)
        {
            Write-Output "asaRobotPause - Job $($jobName) was paused $($minutesSinceStopped) minutes ago, set interval is $($restartThresholdMinute), it is now starting..."
            Start-AzStreamAnalyticsJob -ResourceGroupName $resourceGroupName -Name $asaJobName
        }
        else{
            Write-Output "asaRobotPause - Job $($jobName) was paused $($minutesSinceStopped) minutes ago, set interval is $($restartThresholdMinute), it will not be restarted yet."
        }
    }
    else {
        Write-Output "asaRobotPause - Job $($jobName) is not in a state I can manage: $($currentJobState). Let's wait a bit, but consider helping is that doesn't go away!"
    }

    # Final ASA job status check
    $newJobState = Get-AzStreamAnalyticsJob  -ResourceGroupName $resourceGroupName -Name $asaJobName | Foreach-Object {$_.JobState}
    Write-Output "asaRobotPause - Job $($asaJobName) was $($currentJobState), is now $($newJobState). Job completed."
}
catch
{
    throw $_.Exception.Message
}