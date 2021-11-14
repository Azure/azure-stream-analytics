Param(
    [string]$subscriptionId,
    [string]$resourceGroupName,
    [string]$asaJobName,

    [int]$restartThresholdMinute,
    [int]$stopThresholdMinute,
    
    [int]$maxInputBacklog,
    [int]$maxWatermark
)

# Stop on error
$ErrorActionPreference = 'stop'

# Write an information log with the current time.
$currentUTCtime = (Get-Date).ToUniversalTime()
Write-Host "asaRobotPause - PowerShell timer trigger function is starting at time: $currentUTCtime"

# Set variables
$resourceId = "/subscriptions/$($subscriptionId )/resourceGroups/$($resourceGroupName)/providers/Microsoft.StreamAnalytics/streamingjobs/$($asaJobName)"

# Ensures you do not inherit an AzContext in your runbook
Disable-AzContextAutosave -Scope Process | Out-Null

# Connect using a Managed Service Identity
try {
        $AzureContext = (Connect-AzAccount -Identity).context
    }
catch{
        Write-Output "There is no system-assigned user identity. Aborting.";
        exit
    }

try
{
    # throw "This is an error."
    
    # Check current ASA job status
    $currentJobState = Get-AzStreamAnalyticsJob  -ResourceGroupName $resourceGroupName -Name $asaJobName | Foreach-Object {$_.JobState}
    Write-Output "asaRobotPause - Job $($asaJobName) is $($currentJobState)."

    # Switch state
    if ($currentJobState -eq "Running")
    { 
        # Get-AzActivityLog issues warnings about deprecation coming in future releases, here we ignore them via -WarningAction Ignore
        # We check in 1000 record of history, to make sure we're not missing what we're looking for. It may need adjustment for a job that has a lot of logging happening.
        # There is a bug in Get-AzActivityLog that triggers an error when Select-Object First is in the same pipeline (on the same line). We move it down.
        $startTimeStamp = Get-AzActivityLog -ResourceId $resourceId -MaxRecord 1000 -WarningAction Ignore | Where-Object {$_.EventName.Value -like "Start Job*"}
        $startTimeStamp = $startTimeStamp | Select-Object -First 1 | Foreach-Object {$_.EventTimeStamp}

        # Get-AzMetric issues warnings about deprecation coming in future releases, here we ignore them via -WarningAction Ignore
        $currentBacklog = Get-AzMetric -ResourceId $resourceId -TimeGrain 00:01:00 -MetricName "InputEventsSourcesBacklogged" -DetailedOutput -WarningAction Ignore
        $currentWatermark = Get-AzMetric -ResourceId $resourceId -TimeGrain 00:01:00 -MetricName "OutputWatermarkDelaySeconds" -DetailedOutput -WarningAction Ignore

        # Metric are always lagging 1-3 minutes behind, so grabbing the last N minutes means checking N+3 actually. This may be overly safe and fined tune down per job.
        $Backlog =  $currentBacklog.Data | `
                        Where-Object {$_.Maximum -ge 0} | `
                        Sort-Object -Property Timestamp -Descending | `
                        Where-Object {$_.Timestamp -ge $startTimeStamp} | `
                        Select-Object -First $stopThresholdMinute | 
                        Measure-Object -Sum Maximum
        $BacklogSum = $Backlog.Sum

        $Watermark = $currentWatermark.Data | `
                        Where-Object {$_.Maximum -ge 0} | `
                        Sort-Object -Property Timestamp -Descending | `
                        Where-Object {$_.Timestamp -ge $startTimeStamp} | `
                        Select-Object -First $stopThresholdMinute | `
                        Measure-Object -Average Maximum
        $WatermarkAvg = [int]$Watermark.Average

        Write-Output "asaRobotPause - Job $($asaJobName) is running since $($startTimeStamp) with a sum of $($BacklogSum) backlogged events, and an average watermark of $($WatermarkAvg) sec, for $($Watermark.Count) minutes."

        # -le for lesser or equal
        if (
            ($BacklogSum -ge 0) -and ($BacklogSum -le $maxInputBacklog) -and ` # is not null and under the threshold
            ($WatermarkAvg -ge 0) -and ($WatermarkAvg -le $maxWatermark) -and ` # is not null and under the threshold
            ($Watermark.Count -ge $stopThresholdMinute) # at least N values
            )
        {
            Write-Output "asaRobotPause - Job $($asaJobName) is stopping..."
            Stop-AzStreamAnalyticsJob -ResourceGroupName $resourceGroupName -Name $asaJobName
        }
        else {
            Write-Output "asaRobotPause - Job $($asaJobName) is not stopping yet, it needs to have less than $($maxInputBacklog) backlogged events and under $($maxWatermark) sec watermark for at least $($stopThresholdMinute) minutes."
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
            Write-Output "asaRobotPause - Job $($jobName) was paused $([int]$minutesSinceStopped) minutes ago, set interval is $($restartThresholdMinute), it is now starting..."
            Start-AzStreamAnalyticsJob -ResourceGroupName $resourceGroupName -Name $asaJobName -OutputStartMode LastOutputEventTime
        }
        else{
            Write-Output "asaRobotPause - Job $($jobName) was paused $([int]$minutesSinceStopped) minutes ago, set interval is $($restartThresholdMinute), it will not be restarted yet."
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
