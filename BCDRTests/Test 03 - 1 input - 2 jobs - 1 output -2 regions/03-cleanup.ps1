
######################################################################
#Gather necessary variables from previous terminal if necessary, and paste them into a new window

Write-Host "`
`$rgName = `"$rgName`""

######################################################################
#Delete the resource group

Remove-AzResourceGroup -Name $rgName -Force
