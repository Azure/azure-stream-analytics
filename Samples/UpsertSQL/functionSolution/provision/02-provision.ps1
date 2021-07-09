# Login to Azure
Connect-AzAccount

# If necessary (multiple subscriptions available), select the appropriate one
Get-AzSubscription
Get-AzSubscription -SubscriptionName "mySubscription" | Select-AzSubscription

# Create a Stream Analytics job
$suffix = "dev"

$location="canadacentral"
$suffix="staging"
$datecreated=(Get-Date).tostring(“yyyyMMdd”)

$rg_name="rg-asafunmerge"
$rg_name+=$suffix
$sa_name="saasafunmerge"
$sa_name+=$suffix
$sa_container="container001"
$asa_job_name="asafunmerge"
$asa_job_name+=$suffix

New-AzStreamAnalyticsJob `
  -ResourceGroupName $rg_name `
  -Name $asa_job_name `
  -Location $location `
  -CompatibilityLevel "1.2"`
  -SkuName "Standard"
