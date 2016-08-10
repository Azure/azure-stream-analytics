<#
********************************************************* 
* 
*    Copyright (c) Microsoft. All rights reserved. 
*    This code is licensed under the Microsoft Public License. 
*    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF 
*    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY 
*    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR 
*    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT. 
* 
*********************************************************
#>


[CmdletBinding()]
Param(
   [Parameter()]
   [Alias("mode")]
   [string]$global:mode = 'deploy',

   [Parameter()]
   [Alias("location")]
   [string]$global:customlocation = ''
)


function ValidateParameters{
    $modeList = 'deploy', 'list', 'delete'
	$global:SupportedLocations = 'South Central US', 'North Central US', 'Central US', 'West US', 'East US', 'East US2', 'Japan East', 'Japan West', 'East Asia', 'South East Asia'
    
	if($modeList -notcontains $global:mode){
        Write-Host ''
        Write-Host 'MISSING REQUIRED PARAMETER: -mode parameter must be set to one of: ' $modeList
        $global:mode = Read-Host 'Enter mode'
        while($modeList -notcontains $global:mode){
            Write-Host 'Invalid mode. Please enter a mode from the list above.'
            $global:mode = Read-Host 'Enter mode'                     
        }
    }

	if ($global:customlocation -ne ''){
		$validLocations = $SupportedLocations | foreach  {"$($_.ToLower())"}
		
		if($validLocations  -notcontains $global:customlocation.ToLower()){
			Write-Host ''
			Write-Host 'INVALID OPTIONAL PARAMETER: -location parameter must be set to one of: ' 
			Write-Host $SupportedLocations -Separator ", "
			$global:customlocation = Read-Host 'Enter Location'
			while($validLocations -notcontains $global:customlocation.ToLower()){
				Write-Host 'Invalid location. Please enter a location from the list above.'
				$global:customlocation = Read-Host 'Enter Location'                     
			}
		}
	}
}

function LoadLocation{
    $rowNumber = 1
    $LabLocations = 'South Central US', 'West US', 'East US', 'Central US'
    $validLocations = $global:SupportedLocations | foreach  {"$($_.ToLower())"}
    if($global:location -eq $null) {$global:location = ''}
    
    if($validLocations  -contains $global:location.ToLower()){
        return
    }

    # Select a random location
    Write-Host "Indentifying Location for your lab " -NoNewLine
    $randLocationIndex = Get-Random -Minimum 1 -Maximum ($LabLocations.Count + 1)
    $global:location = $LabLocations[$randLocationIndex - 1]
    Write-Host $global:location 
}

function SetResourceParams {
    $global:useCaseName = "TollData"
    $global:defaultResourceName =  $global:useCaseName + $global:resourceSuffix  
	$global:entryEventHubName = "entry"
    $global:exitEventHubName = "exit"
	$global:ServiceBusNamespace = $global:defaultResourceName
    $global:sqlDBName = $global:useCaseName + "DB"
    $global:sqlServerLogin = 'tolladmin'
    $global:sqlServerPassword = '123toll!'   
    $global:storageAccountName = "tolldata" + $global:resourceSuffix   
}


function InitializeSubscription{
    $global:Validated = $false
    ValidateSubscription

    if($global:AzureAccountIntialized -ne 1){InitSubscription}

    # Check if subscription is already initialized
    $SubscriptionSettingFile = $PSScriptRoot + '\\Settings-' + $global:subscriptionId + '.xml'
    $FileExists = Test-Path $SubscriptionSettingFile
    If ($FileExists -eq $True) {
        $loaded = LoadSubscription $SubscriptionSettingFile
    }
    if($loaded -ne $true){
        $global:resourceSuffix = Get-Random -Maximum 9999999999
		if($global:customLocation -eq ''){
			$global:location = $null
		}
		else
		{
			$global:location = $global:customLocation
		}
        
    }
    LoadLocation
    SetResourceParams
    SaveSubscription
}

function LoadSubscription($fileName){
    try
    {
        [xml] $settings = Get-Content $fileName
        $global:location = $settings.Settings.Location
        $global:resourceSuffix = $settings.Settings.ResourceSuffix
        $global:sqlServerName = $settings.Settings.SqlServerName

        return $true
    }
    catch{
        return $false
    }
}

function SaveSubscription{
    $fileName = $PSScriptRoot + '\\Settings-' + $global:subscriptionId + '.xml'
    Out-File -filePath $fileName -force -InputObject "<Settings><Location>$global:location</Location><ResourceSuffix>$global:resourceSuffix</ResourceSuffix><SqlServerName>$global:sqlserverName</SqlServerName></Settings>"
}

function InitSubscription{
       
    #Remove all Azure Account from Cache if any
    Get-AzureAccount | ForEach-Object { Remove-AzureAccount $_.ID -Force -WarningAction SilentlyContinue }

    #login
    Add-AzureAccount -WarningAction SilentlyContinue | out-null
    $account = Get-AzureAccount
	Write-Host You are signed-in with $account.id

 	$subList = Get-AzureSubscription
	if($subList.Length -lt 1){
		throw 'Your azure account does not have any subscriptions.  A subscription is required to run this tool'
	} 

	$subCount = 0
	foreach($sub in $subList){
		$subCount++
		$sub | Add-Member -type NoteProperty -name RowNumber -value $subCount
	}

	if($subCount -gt 1)
	{
		Write-Host ''
		Write-Host 'Your Azure Subscriptions: '
		
		$subList | Format-Table RowNumber,SubscriptionId,SubscriptionName -AutoSize
		$rowNum = Read-Host 'Enter the row number (1 -'$subCount') of a subscription'

		while( ([int]$rowNum -lt 1) -or ([int]$rowNum -gt [int]$subCount)){
			Write-Host 'Invalid subscription row number. Please enter a row number from the list above'
			$rowNum = Read-Host 'Enter subscription row number'                     
		}
	}
	else{
		$rowNum = 1
	}
	
	$global:subscriptionID = $subList[$rowNum-1].SubscriptionId;
	$global:subscriptionDefaultAccount = $subList[$rowNum-1].DefaultAccount.Split('@')[0]

#switch to appropriate subscription 
    try{ 
        Select-AzureSubscription -SubscriptionId $global:subscriptionID 
    }  
    catch{ 
        throw 'Subscription ID provided is invalid: ' + $global:subscriptionID     
    } 

}

function ValidateSubscription{
    #Check if the current subscription is available
    $global:AzureAccountIntialized = 0
    if($global:subscriptionID -ne $null -and $global:subscriptionID -ne ''){
        $subscription = Get-AzureSubscription -SubscriptionId $global:subscriptionID -ErrorAction SilentlyContinue
        if($subscription -ne $null){
            $locations = Get-AzureLocation -ErrorAction SilentlyContinue
            if($locations -ne $null){
                $global:AzureAccountIntialized = 1
            }
        }
    }
}

function CreateAndValidateStorageAccount{
    $containerName = "tolldata"
    $storageAccount = Get-AzureStorageAccount -StorageAccountName $global:storageAccountName -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
    if($storageAccount -eq $null){
		Write-Host "Creating AzureStorageAccount [ $global:storageAccountName ]...." -NoNewline
        New-AzureStorageAccount -StorageAccountName $global:storageAccountName -Location $global:location -Label "ASAHandsOnLab"
        Write-Host Write-Host "AzureStorageAccount [ $global:storageAccountName ] created." 
    }
	else{
		Write-Host "AzureStorageAccount [ $global:storageAccountName ] already exists." 
	}
	
    $storageKeys = Get-AzureStorageKey -StorageAccountName $storageAccountName
    $global:storageAccountKey = $storageKeys.Primary
    $storageContext = New-AzureStorageContext -StorageAccountName $storageAccountName -StorageAccountKey $storageKeys.Primary
    $container = $null
    
    try{
        $container = Get-AzureStorageContainer -Name $containerName -Context $storageContext -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
    }
    catch{
        $container = $null
    }
    
    if($container -eq $null){
		Write-Host "Creating Container [ $containerName ]...." -NoNewline
        $container = New-AzureStorageContainer -Name $containerName -Context $storageContext 
		Write-Host "created"
    }
	else{
		Write-Host "Container [ $containerName ] exists" 
	}
	
	Write-Host "Uploading reference data to container...." -NoNewline
    $copyResult = Set-AzureStorageBlobContent -file ($PSScriptRoot + "\\Data\\Registration.json") -Container tolldata -Blob "registration.json" -Context $storageContext -Force
    Write-Host "Completed"
}

function CreateSqlServer{
	Write-Host 'Creating SQL Server ...... ' -NoNewline
    $sqlsvr = New-AzureSqlDatabaseServer -location $global:location -AdministratorLogin $global:sqlServerLogin -AdministratorLoginPassword $global:sqlServerPassword
    if($sqlsvr -eq $null){
        throw $Error
    }

    $sqlsvrname = $sqlsvr.ServerName
    $createdNew = $TRUE;
                
    Write-Host '[svr name: ' $sqlsvrname ']....created.' 
    $global:sqlserverName = $sqlsvrname

    #Setting firewall rule
    $rule = New-AzureSqlDatabaseServerFirewallRule -ServerName $sqlsvr.ServerName -RuleName "demorule" -StartIPAddress "0.0.0.0" -EndIPAddress "255.255.255.255" -ErrorAction Stop
}

function CreateAndValidateSQLServerAndDB{
    process{
        
        #create sql server & DB
        $sqlsvr = $null
        $createdNew = $FALSE
        try{ 
     
            if($global:sqlserverName -eq $null -or $global:sqlserverName.Length -le 1){
                CreateSqlServer
            }
            else{
                $server = Get-AzureSqlDatabaseServer -ServerName $global:sqlserverName -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
                if($server -eq $null)
                {
                    CreateSqlServer
                }
				else{
					Write-Host "SQL Server [ $global:sqlserverName ] exists." 
				}
            }

            $check = Get-AzureSqlDatabase -ServerName $global:sqlserverName -DatabaseName $global:sqlDBName -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
            if(!$check){
                #Creating Database
                Write-Host "Creating SQL DB [ $global:sqlDBName ]......" -NoNewline 
                $servercredential = new-object System.Management.Automation.PSCredential($global:sqlServerLogin, ($global:sqlServerPassword  | ConvertTo-SecureString -asPlainText -Force))
                #create a connection context
                $ctx = New-AzureSqlDatabaseServerContext -ServerName $global:sqlserverName -Credential $serverCredential
                $sqldb = New-AzureSqlDatabase $ctx -DatabaseName $global:sqlDBName -Edition Basic | out-null
                Write-Host 'created.'
            }
			else{
				Write-Host "SQL Database [ $global:sqlDBName ] exists in server [ $global:sqlserverName ]" 
			}
        } 
        catch{        
            Write-Host 'error.'
            throw
        }
        return $sqldb
    }
}

function CreateSqlTable{
	# Create Sql tables
	Write-Host "Creating required sqlTables......" -NoNewline
	$sqlConnString = GetSqlConnectionString
	$sqlConn= New-Object System.Data.SqlClient.SqlConnection($sqlConnString)
	$sqlConn.Open()

	$cmdText = [System.IO.File]::ReadAllText("$PSScriptRoot\\SqlScripts\\CreateTables.sql")
	$sqlCmd= New-Object System.Data.SqlClient.SqlCommand($cmdText,$sqlConn)
	$cmdResult = $sqlCmd.ExecuteNonQuery()
	$sqlConn.Close()
	Write-Host "created."
}

function GetSqlConnectionString{
    return "Server=tcp:$global:sqlserverName.database.windows.net,1433;Database=$global:sqlDBName;Uid=$global:sqlServerLogin@$global:sqlserverName;Pwd=

$global:sqlServerPassword;Encrypt=yes;Connection Timeout=30;"
}


function CreateAndValidateSBNamespace{
  
    Process{
        $namespace = $null
        try{
			# WARNING: Make sure to reference the latest version of the \Microsoft.ServiceBus.dll
			$namespace = Get-AzureSBNamespace -Name $global:ServiceBusNamespace 
			if($namespace.name -eq $null){
			    Write-Host "Creating the Service Bus Namespace [ $global:ServiceBusNamespace ]......" -NoNewline
                #create a new Service Bus Namespace
                $namespace = New-AzureSBNamespace -Name $global:ServiceBusNamespace -Location $global:location -CreateACSNamespace $true -NamespaceType Messaging -ErrorAction Stop
			    Write-Host 'created.'
			}
			else{
			    Write-Host "ServiceBusNamespace $global:ServiceBusNamespace exists"
			}
	    }
        catch{
            Write-Host 'error.'
            throw
        }
               
        try{		
            $global:constring = Get-AzureSBAuthorizationRule -Namespace $global:ServiceBusNamespace
			$constringparse = $global:constring.ConnectionString.Split(';')
			$global:sharedaccesskeyname = $constringparse[1].Substring(20)
			$global:sharedaccesskey = $constringparse[2].Substring(16)
        }
        catch{
            Write-Host 'error.'
		    throw
        }
    }
}


function CreateAndValidateEventHub($eventHubName)
{
 
    Process{
        $namespace = $null
		$eventhub = $null
        
        try{		
			#Create the NamespaceManager object to create the event hub
			$currentnamespace = Get-AzureSBNamespace -Name $global:ServiceBusNamespace
            $nsMgrType = [System.Reflection.Assembly]::LoadFrom($PSScriptRoot +"\Microsoft.ServiceBus.dll").GetType("Microsoft.ServiceBus.NamespaceManager")
			
            $namespacemanager = $nsMgrType::CreateFromConnectionString($currentnamespace.ConnectionString);
			Write-Host 'Creating EventHub [' $eventHubName ']......' -NoNewline
			$eventhub = $namespacemanager.CreateEventHubIfNotExists($eventHubName)
			Write-Host 'created.'
        }
        catch{
            Write-Host 'error.'
		    throw
        }
    }
}

function DeleteSBNameSpace {
#Cleanup Service Bus namespaces

Write-Host "WARNING: This script is going to delete resources that match resource names used in the lab. Please carefully review names of the resources before confirming 

delete operation" -ForegroundColor Yellow
Write-Host "Remove Service Bus namespaces starting with '$global:useCaseName'"
Get-AzureSBNamespace | Where-Object {$_.Name -like "*" + $global:useCaseName + "*"} | Remove-AzureSBNamespace -Confirm
}

function DeleteSqlServer{
Write-Host "Remove Azure SQL servers with Administrator Login 'tolladmin'"
Get-AzureSqlDatabaseServer | Where-Object {$_.AdministratorLogin -eq 'tolladmin'} | Remove-AzureSqlDatabaseServer -Confirm
}

function DeleteStorageAccount{
foreach ($storageaccount in Get-AzureStorageAccount -WarningAction SilentlyContinue | Where-Object {$_.StorageAccountName -like "*$global:useCaseName*"})
{
	$caption = "Choose Action";
	$message = "Are you sure you want to delete stroage account: " + $storageaccount.StorageAccountName + " ?";
	$yesanswer = new-Object System.Management.Automation.Host.ChoiceDescription "&Yes","Yes";
	$noanswer = new-Object System.Management.Automation.Host.ChoiceDescription "&No","No";
	$choices = [System.Management.Automation.Host.ChoiceDescription[]]($yesanswer,$noanswer);
	$answer = $host.ui.PromptForChoice($caption,$message,$choices,1)

	switch ($answer){
		0 {$storageaccount | Remove-AzureStorageAccount -WarningAction SilentlyContinue; break}
		1 {break}
	}
}
}

function ShowMenu{

    Write-Host ""
    Write-Host " Command Options "
    Write-Host ""
    Write-Host '1 - Create or Validate Resources'
    Write-Host '2 - List Resources'
    Write-Host '3 - Start Event Generator'
    Write-Host '4 - Delete Resources'
    Write-Host '5 - Exit'
    Write-Host ""

    $SelectedAction = Read-Host 'Enter menu option (1 - 5)'
	while( ([int]$SelectedAction -lt 1) -or ([int]$SelectedAction -gt 5)){
		Write-Host 'Invalid Menu option. Please enter a number from the list above'
		$SelectedAction = Read-Host 'Enter menu option (1 - 5)'                     
	}

    return $SelectedAction
}

function IntitializeAccount{
	InitSubscription
	ValidateSubscription
}


function CreateAndValidateResources{
	
    Write-Host "Creating/Validating resources for Toll App"
    CreateAndValidateSBNamespace
	CreateAndValidateEventHub($global:entryEventHubName)
	CreateAndValidateEventHub($global:exitEventHubName)
    CreateAndValidateSQLServerAndDB
    CreateSqlTable
    CreateAndValidateStorageAccount
    SaveSubscription
    $global:Validated = $true
}

function LaunchGenerator{
    if($global:Validated -eq $false){
        Write-Host "Create/Validate resource before List Resources"
        return
    }
    $configFile = "$PSScriptRoot\\TollApp.exe.config"
    $exeFile = "$PSScriptRoot\\TollApp.exe"
    [xml] $configXml = Get-Content $configFile
    $node=Select-Xml "//configuration/appSettings/add[@key='Microsoft.ServiceBus.ConnectionString']" $configXml
    $node.Node.value=$global:constring.ConnectionString
    $configXml.Save($configFile)
    start-process $exeFile
}

function ListResources{
    if($global:Validated -eq $false){
        Write-Host "Create/Validate resource before List Resources"
        return
    }
	$account = Get-AzureAccount
	$subscription = Get-AzureSubscription -SubscriptionId $global:subscriptionID
    Write-Host ""
    Write-Host "All Resource Names"
    Write-Host ""
	Write-Host "You are signed-in with " $account.id
	Write-Host "Subscription Id: " $subscription.SubscriptionID
	Write-Host "Subscription Name: " $subscription.SubscriptionName
	Write-Host ""
    Write-Host "Service Bus:"
    Write-Host "`tNamespace: $global:ServiceBusNamespace"
    Write-Host "`tSharedAccessKeyName: $global:sharedaccesskeyname"
    Write-Host "`tSharedAccessKey: $global:sharedaccesskey"
    Write-Host ""
    Write-Host "Sql Server:"
    Write-Host "`tServer: $global:sqlserverName.database.windows.net"
    Write-Host "`tSqlLogin: $global:sqlServerLogin"
    Write-Host "`tPassword: $global:sqlServerPassword"
    Write-Host "`tDatabaseName: $global:sqlDBName"
    Write-Host ""
    Write-Host "Storage Account:"
    Write-Host "`tAccountName: $global:storageAccountName"
    Write-Host "`tAccountKey: $global:storageAccountKey"
    Write-Host ""
    Write-Host "Location: " $global:location 
    Write-Host ""
    Write-Host ""
}

function DeleteResources{
    DeleteSBNameSpace 
    DeleteSqlServer
    DeleteStorageAccount
    Remove-Item "$PSScriptRoot\\Settings-$global:subscriptionId.xml" -ErrorAction SilentlyContinue
    $global:subscriptionID = ""
    exit
}

#start of main script
$storePreference = $Global:VerbosePreference
$debugPreference= $Global:DebugPreference


$Global:VerbosePreference = "SilentlyContinue"
$Global:DebugPreference="SilentlyContinue"

dir | unblock-file
ValidateParameters
InitializeSubscription

switch($global:mode){
    'deploy'{
        CreateAndValidateResources
        ListResources
        LaunchGenerator
    }
    'list'{
        CreateAndValidateResources
        ListResources
    }
    'delete'{ DeleteResources }
}

$Global:VerbosePreference = $storePreference
$Global:DebugPreference= $debugPreference

# SIG # Begin signature block
# MIIkLwYJKoZIhvcNAQcCoIIkIDCCJBwCAQExDzANBglghkgBZQMEAgEFADB5Bgor
# BgEEAYI3AgEEoGswaTA0BgorBgEEAYI3AgEeMCYCAwEAAAQQH8w7YFlLCE63JNLG
# KX7zUQIBAAIBAAIBAAIBAAIBADAxMA0GCWCGSAFlAwQCAQUABCCuxjb6oZDzm947
# YId+Bv+49rzMMlJ2zPVQxagiX0/NbaCCDZIwggYQMIID+KADAgECAhMzAAAAZEeE
# lIbbQRk4AAAAAABkMA0GCSqGSIb3DQEBCwUAMH4xCzAJBgNVBAYTAlVTMRMwEQYD
# VQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNy
# b3NvZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNpZ25p
# bmcgUENBIDIwMTEwHhcNMTUxMDI4MjAzMTQ2WhcNMTcwMTI4MjAzMTQ2WjCBgzEL
# MAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1v
# bmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjENMAsGA1UECxMETU9Q
# UjEeMBwGA1UEAxMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMIIBIjANBgkqhkiG9w0B
# AQEFAAOCAQ8AMIIBCgKCAQEAky7a2OY+mNkbD2RfTahYTRQ793qE/DwRMTrvicJK
# LUGlSF3dEp7vq2YoNNV9KlV7TE2K8sDxstNSFYu2swi4i1AL3X/7agmg3GcExPHf
# vHUYIEC+eCyZVt3u9S7dPkL5Wh8wrgEUirCCtVGg4m1l/vcYCo0wbU06p8XzNi3u
# XyygkgCxHEziy/f/JCV/14/A3ZduzrIXtsccRKckyn6B5uYxuRbZXT7RaO6+zUjQ
# hiyu3A4hwcCKw+4bk1kT9sY7gHIYiFP7q78wPqB3vVKIv3rY6LCTraEbjNR+phBQ
# EL7hyBxk+ocu+8RHZhbAhHs2r1+6hURsAg8t4LAOG6I+JQIDAQABo4IBfzCCAXsw
# HwYDVR0lBBgwFgYIKwYBBQUHAwMGCisGAQQBgjdMCAEwHQYDVR0OBBYEFFhWcQTw
# vbsz9YNozOeARvdXr9IiMFEGA1UdEQRKMEikRjBEMQ0wCwYDVQQLEwRNT1BSMTMw
# MQYDVQQFEyozMTY0Mis0OWU4YzNmMy0yMzU5LTQ3ZjYtYTNiZS02YzhjNDc1MWM0
# YjYwHwYDVR0jBBgwFoAUSG5k5VAF04KqFzc3IrVtqMp1ApUwVAYDVR0fBE0wSzBJ
# oEegRYZDaHR0cDovL3d3dy5taWNyb3NvZnQuY29tL3BraW9wcy9jcmwvTWljQ29k
# U2lnUENBMjAxMV8yMDExLTA3LTA4LmNybDBhBggrBgEFBQcBAQRVMFMwUQYIKwYB
# BQUHMAKGRWh0dHA6Ly93d3cubWljcm9zb2Z0LmNvbS9wa2lvcHMvY2VydHMvTWlj
# Q29kU2lnUENBMjAxMV8yMDExLTA3LTA4LmNydDAMBgNVHRMBAf8EAjAAMA0GCSqG
# SIb3DQEBCwUAA4ICAQCI4gxkQx3dXK6MO4UktZ1A1r1mrFtXNdn06DrARZkQTdu0
# kOTLdlGBCfCzk0309RLkvUgnFKpvLddrg9TGp3n80yUbRsp2AogyrlBU+gP5ggHF
# i7NjGEpj5bH+FDsMw9PygLg8JelgsvBVudw1SgUt625nY7w1vrwk+cDd58TvAyJQ
# FAW1zJ+0ySgB9lu2vwg0NKetOyL7dxe3KoRLaztUcqXoYW5CkI+Mv3m8HOeqlhyf
# FTYxPB5YXyQJPKQJYh8zC9b90JXLT7raM7mQ94ygDuFmlaiZ+QSUR3XVupdEngrm
# ZgUB5jX13M+Pl2Vv7PPFU3xlo3Uhj1wtupNC81epoxGhJ0tRuLdEajD/dCZ0xIni
# esRXCKSC4HCL3BMnSwVXtIoj/QFymFYwD5+sAZuvRSgkKyD1rDA7MPcEI2i/Bh5O
# MAo9App4sR0Gp049oSkXNhvRi/au7QG6NJBTSBbNBGJG8Qp+5QThKoQUk8mj0ugr
# 4yWRsA9JTbmqVw7u9suB5OKYBMUN4hL/yI+aFVsE/KJInvnxSzXJ1YHka45ADYMK
# AMl+fLdIqm3nx6rIN0RkoDAbvTAAXGehUCsIod049A1T3IJyUJXt3OsTd3WabhIB
# XICYfxMg10naaWcyUePgW3+VwP0XLKu4O1+8ZeGyaDSi33GnzmmyYacX3BTqMDCC
# B3owggVioAMCAQICCmEOkNIAAAAAAAMwDQYJKoZIhvcNAQELBQAwgYgxCzAJBgNV
# BAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4w
# HAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xMjAwBgNVBAMTKU1pY3Jvc29m
# dCBSb290IENlcnRpZmljYXRlIEF1dGhvcml0eSAyMDExMB4XDTExMDcwODIwNTkw
# OVoXDTI2MDcwODIxMDkwOVowfjELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hp
# bmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jw
# b3JhdGlvbjEoMCYGA1UEAxMfTWljcm9zb2Z0IENvZGUgU2lnbmluZyBQQ0EgMjAx
# MTCCAiIwDQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIBAKvw+nIQHC6t2G6qghBN
# NLrytlghn0IbKmvpWlCquAY4GgRJun/DDB7dN2vGEtgL8DjCmQawyDnVARQxQtOJ
# DXlkh36UYCRsr55JnOloXtLfm1OyCizDr9mpK656Ca/XllnKYBoF6WZ26DJSJhIv
# 56sIUM+zRLdd2MQuA3WraPPLbfM6XKEW9Ea64DhkrG5kNXimoGMPLdNAk/jj3gcN
# 1Vx5pUkp5w2+oBN3vpQ97/vjK1oQH01WKKJ6cuASOrdJXtjt7UORg9l7snuGG9k+
# sYxd6IlPhBryoS9Z5JA7La4zWMW3Pv4y07MDPbGyr5I4ftKdgCz1TlaRITUlwzlu
# ZH9TupwPrRkjhMv0ugOGjfdf8NBSv4yUh7zAIXQlXxgotswnKDglmDlKNs98sZKu
# HCOnqWbsYR9q4ShJnV+I4iVd0yFLPlLEtVc/JAPw0XpbL9Uj43BdD1FGd7P4AOG8
# rAKCX9vAFbO9G9RVS+c5oQ/pI0m8GLhEfEXkwcNyeuBy5yTfv0aZxe/CHFfbg43s
# TUkwp6uO3+xbn6/83bBm4sGXgXvt1u1L50kppxMopqd9Z4DmimJ4X7IvhNdXnFy/
# dygo8e1twyiPLI9AN0/B4YVEicQJTMXUpUMvdJX3bvh4IFgsE11glZo+TzOE2rCI
# F96eTvSWsLxGoGyY0uDWiIwLAgMBAAGjggHtMIIB6TAQBgkrBgEEAYI3FQEEAwIB
# ADAdBgNVHQ4EFgQUSG5k5VAF04KqFzc3IrVtqMp1ApUwGQYJKwYBBAGCNxQCBAwe
# CgBTAHUAYgBDAEEwCwYDVR0PBAQDAgGGMA8GA1UdEwEB/wQFMAMBAf8wHwYDVR0j
# BBgwFoAUci06AjGQQ7kUBU7h6qfHMdEjiTQwWgYDVR0fBFMwUTBPoE2gS4ZJaHR0
# cDovL2NybC5taWNyb3NvZnQuY29tL3BraS9jcmwvcHJvZHVjdHMvTWljUm9vQ2Vy
# QXV0MjAxMV8yMDExXzAzXzIyLmNybDBeBggrBgEFBQcBAQRSMFAwTgYIKwYBBQUH
# MAKGQmh0dHA6Ly93d3cubWljcm9zb2Z0LmNvbS9wa2kvY2VydHMvTWljUm9vQ2Vy
# QXV0MjAxMV8yMDExXzAzXzIyLmNydDCBnwYDVR0gBIGXMIGUMIGRBgkrBgEEAYI3
# LgMwgYMwPwYIKwYBBQUHAgEWM2h0dHA6Ly93d3cubWljcm9zb2Z0LmNvbS9wa2lv
# cHMvZG9jcy9wcmltYXJ5Y3BzLmh0bTBABggrBgEFBQcCAjA0HjIgHQBMAGUAZwBh
# AGwAXwBwAG8AbABpAGMAeQBfAHMAdABhAHQAZQBtAGUAbgB0AC4gHTANBgkqhkiG
# 9w0BAQsFAAOCAgEAZ/KGpZjgVHkaLtPYdGcimwuWEeFjkplCln3SeQyQwWVfLiw+
# +MNy0W2D/r4/6ArKO79HqaPzadtjvyI1pZddZYSQfYtGUFXYDJJ80hpLHPM8QotS
# 0LD9a+M+By4pm+Y9G6XUtR13lDni6WTJRD14eiPzE32mkHSDjfTLJgJGKsKKELuk
# qQUMm+1o+mgulaAqPyprWEljHwlpblqYluSD9MCP80Yr3vw70L01724lruWvJ+3Q
# 3fMOr5kol5hNDj0L8giJ1h/DMhji8MUtzluetEk5CsYKwsatruWy2dsViFFFWDgy
# cScaf7H0J/jeLDogaZiyWYlobm+nt3TDQAUGpgEqKD6CPxNNZgvAs0314Y9/HG8V
# fUWnduVAKmWjw11SYobDHWM2l4bf2vP48hahmifhzaWX0O5dY0HjWwechz4GdwbR
# BrF1HxS+YWG18NzGGwS+30HHDiju3mUv7Jf2oVyW2ADWoUa9WfOXpQlLSBCZgB/Q
# ACnFsZulP0V3HjXG0qKin3p6IvpIlR+r+0cjgPWe+L9rt0uX4ut1eBrs6jeZeRhL
# /9azI2h15q/6/IvrC4DqaTuv/DDtBEyO3991bWORPdGdVk5Pv4BXIqF4ETIheu9B
# CrE/+6jMpF3BoYibV3FWTkhFwELJm3ZbCoBIa/15n8G9bW1qyVJzEw16UM0xghXz
# MIIV7wIBATCBlTB+MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQ
# MA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9u
# MSgwJgYDVQQDEx9NaWNyb3NvZnQgQ29kZSBTaWduaW5nIFBDQSAyMDExAhMzAAAA
# ZEeElIbbQRk4AAAAAABkMA0GCWCGSAFlAwQCAQUAoIHeMBkGCSqGSIb3DQEJAzEM
# BgorBgEEAYI3AgEEMBwGCisGAQQBgjcCAQsxDjAMBgorBgEEAYI3AgEVMC8GCSqG
# SIb3DQEJBDEiBCAquA/jsZPApxSIYOL2FvKDS4OUTeogeSUDR+Pm1zgBmzByBgor
# BgEEAYI3AgEMMWQwYqAggB4AUwB0AHIAZQBhAG0AQQBuAGEAbAB5AHQAaQBjAHOh
# PoA8aHR0cHM6Ly9henVyZS5taWNyb3NvZnQuY29tL2VuLXVzL3NlcnZpY2VzL3N0
# cmVhbS1hbmFseXRpY3MvMA0GCSqGSIb3DQEBAQUABIIBAH2ky7K9bh7H1btT8EK/
# QjJhuubdOQuZfcEUPKwW5+dxqVfVJkARYlCgFxms+LOy0fMktd3cfGD4PI69nn/5
# jtlNYyORfjehEYxsJfXTkzfLS0GTQ//FB/vGrge6aLJt9nlkTXsjIcXBnl6vkH61
# jvLcnoy5tiPdfYKFuAOe60bGMos0wcbZfi92I9GAmAQZ/ADsz4t9MX17FRfddfqN
# yEsFxNhXFm5GYApzRogSWH1LbdD2ieb31ebJTkC5uhtUjI1gzv+w87jNSmnfxBvK
# iTHL7FzvC51l5QEm0szC7TUvXOlkZ0N0nn9YkrpU9HLUqoCgCxtY2REH95Bfvxm2
# 7gKhghNNMIITSQYKKwYBBAGCNwMDATGCEzkwghM1BgkqhkiG9w0BBwKgghMmMIIT
# IgIBAzEPMA0GCWCGSAFlAwQCAQUAMIIBPQYLKoZIhvcNAQkQAQSgggEsBIIBKDCC
# ASQCAQEGCisGAQQBhFkKAwEwMTANBglghkgBZQMEAgEFAAQgM7DN18f3MgVu1O6e
# 6yCavsCCJ9I4juKchcGcpB6PxcQCBlaregcHpxgTMjAxNjAyMDYwMDU4MDQuMDQz
# WjAHAgEBgAIB9KCBuaSBtjCBszELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hp
# bmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jw
# b3JhdGlvbjENMAsGA1UECxMETU9QUjEnMCUGA1UECxMebkNpcGhlciBEU0UgRVNO
# OkY1MjgtMzc3Ny04QTc2MSUwIwYDVQQDExxNaWNyb3NvZnQgVGltZS1TdGFtcCBT
# ZXJ2aWNloIIO0DCCBnEwggRZoAMCAQICCmEJgSoAAAAAAAIwDQYJKoZIhvcNAQEL
# BQAwgYgxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQH
# EwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xMjAwBgNV
# BAMTKU1pY3Jvc29mdCBSb290IENlcnRpZmljYXRlIEF1dGhvcml0eSAyMDEwMB4X
# DTEwMDcwMTIxMzY1NVoXDTI1MDcwMTIxNDY1NVowfDELMAkGA1UEBhMCVVMxEzAR
# BgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1p
# Y3Jvc29mdCBDb3Jwb3JhdGlvbjEmMCQGA1UEAxMdTWljcm9zb2Z0IFRpbWUtU3Rh
# bXAgUENBIDIwMTAwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCpHQ28
# dxGKOiDs/BOX9fp/aZRrdFQQ1aUKAIKF++18aEssX8XD5WHCdrc+Zitb8BVTJwQx
# H0EbGpUdzgkTjnxhMFmxMEQP8WCIhFRDDNdNuDgIs0Ldk6zWczBXJoKjRQ3Q6vVH
# gc2/JGAyWGBG8lhHhjKEHnRhZ5FfgVSxz5NMksHEpl3RYRNuKMYa+YaAu99h/EbB
# Jx0kZxJyGiGKr0tkiVBisV39dx898Fd1rL2KQk1AUdEPnAY+Z3/1ZsADlkR+79BL
# /W7lmsqxqPJ6Kgox8NpOBpG2iAg16HgcsOmZzTznL0S6p/TcZL2kAcEgCZN4zfy8
# wMlEXV4WnAEFTyJNAgMBAAGjggHmMIIB4jAQBgkrBgEEAYI3FQEEAwIBADAdBgNV
# HQ4EFgQU1WM6XIoxkPNDe3xGG8UzaFqFbVUwGQYJKwYBBAGCNxQCBAweCgBTAHUA
# YgBDAEEwCwYDVR0PBAQDAgGGMA8GA1UdEwEB/wQFMAMBAf8wHwYDVR0jBBgwFoAU
# 1fZWy4/oolxiaNE9lJBb186aGMQwVgYDVR0fBE8wTTBLoEmgR4ZFaHR0cDovL2Ny
# bC5taWNyb3NvZnQuY29tL3BraS9jcmwvcHJvZHVjdHMvTWljUm9vQ2VyQXV0XzIw
# MTAtMDYtMjMuY3JsMFoGCCsGAQUFBwEBBE4wTDBKBggrBgEFBQcwAoY+aHR0cDov
# L3d3dy5taWNyb3NvZnQuY29tL3BraS9jZXJ0cy9NaWNSb29DZXJBdXRfMjAxMC0w
# Ni0yMy5jcnQwgaAGA1UdIAEB/wSBlTCBkjCBjwYJKwYBBAGCNy4DMIGBMD0GCCsG
# AQUFBwIBFjFodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vUEtJL2RvY3MvQ1BTL2Rl
# ZmF1bHQuaHRtMEAGCCsGAQUFBwICMDQeMiAdAEwAZQBnAGEAbABfAFAAbwBsAGkA
# YwB5AF8AUwB0AGEAdABlAG0AZQBuAHQALiAdMA0GCSqGSIb3DQEBCwUAA4ICAQAH
# 5ohRDeLG4Jg/gXEDPZ2joSFvs+umzPUxvs8F4qn++ldtGTCzwsVmyWrf9efweL3H
# qJ4l4/m87WtUVwgrUYJEEvu5U4zM9GASinbMQEBBm9xcF/9c+V4XNZgkVkt070IQ
# yK+/f8Z/8jd9Wj8c8pl5SpFSAK84Dxf1L3mBZdmptWvkx872ynoAb0swRCQiPM/t
# A6WWj1kpvLb9BOFwnzJKJ/1Vry/+tuWOM7tiX5rbV0Dp8c6ZZpCM/2pif93FSguR
# JuI57BlKcWOdeyFtw5yjojz6f32WapB4pm3S4Zz5Hfw42JT0xqUKloakvZ4argRC
# g7i1gJsiOCC1JeVk7Pf0v35jWSUPei45V3aicaoGig+JFrphpxHLmtgOR5qAxdDN
# p9DvfYPw4TtxCd9ddJgiCGHasFAeb73x4QDf5zEHpJM692VHeOj4qEir995yfmFr
# b3epgcunCaw5u+zGy9iCtHLNHfS4hQEegPsbiSpUObJb2sgNVZl6h3M7COaYLeqN
# 4DMuEin1wC9UJyH3yKxO2ii4sanblrKnQqLJzxlBTeCG+SqaoxFmMNO7dDJL32N7
# 9ZmKLxvHIa9Zta7cRDyXUHHXodLFVeNp3lfB0d4wwP3M5k37Db9dT+mdHhk4L7zP
# WAUu7w2gUDXa7wknHNWzfjUeCLraNtvTX4/edIhJEjCCBNowggPCoAMCAQICEzMA
# AABytngu0A9Uj4gAAAAAAHIwDQYJKoZIhvcNAQELBQAwfDELMAkGA1UEBhMCVVMx
# EzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoT
# FU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEmMCQGA1UEAxMdTWljcm9zb2Z0IFRpbWUt
# U3RhbXAgUENBIDIwMTAwHhcNMTUxMDA3MTgxNzM4WhcNMTcwMTA3MTgxNzM4WjCB
# szELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1Jl
# ZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjENMAsGA1UECxME
# TU9QUjEnMCUGA1UECxMebkNpcGhlciBEU0UgRVNOOkY1MjgtMzc3Ny04QTc2MSUw
# IwYDVQQDExxNaWNyb3NvZnQgVGltZS1TdGFtcCBTZXJ2aWNlMIIBIjANBgkqhkiG
# 9w0BAQEFAAOCAQ8AMIIBCgKCAQEArwySfFp/b/s9/KTMw0U0VHqcOaMh7UnTdnA8
# YWZ/v1uGGFeFpZGALzpHTr3th9AOqbqDrDklNI83BSXOrt4+dbuYa2dVdqrZj1GW
# NGrHozzYZUUbPgeCDHsslEgDh4hVNKQGdu1BGUHqALy07r0g54CGCpxosbrsdGVy
# VqFypP2o/fVEbpeWiqZSC0P4YzpsMPJHu03MzKW278/YfI+rVHxWw0RArDjl9hbF
# yK1DVb+/19Wu2NPZsfss4DDEhQwphPYV4gLpYbZBG7Wn5Y65uqhv8BHB3R0qbuAG
# ALSJqbuVwGdI7Km3StUz46noERx1GQZyjtOAdkAXzCn39RkhDwIDAQABo4IBGzCC
# ARcwHQYDVR0OBBYEFPZwiokNdc0lLz6rnRGCBIF9ZThEMB8GA1UdIwQYMBaAFNVj
# OlyKMZDzQ3t8RhvFM2hahW1VMFYGA1UdHwRPME0wS6BJoEeGRWh0dHA6Ly9jcmwu
# bWljcm9zb2Z0LmNvbS9wa2kvY3JsL3Byb2R1Y3RzL01pY1RpbVN0YVBDQV8yMDEw
# LTA3LTAxLmNybDBaBggrBgEFBQcBAQROMEwwSgYIKwYBBQUHMAKGPmh0dHA6Ly93
# d3cubWljcm9zb2Z0LmNvbS9wa2kvY2VydHMvTWljVGltU3RhUENBXzIwMTAtMDct
# MDEuY3J0MAwGA1UdEwEB/wQCMAAwEwYDVR0lBAwwCgYIKwYBBQUHAwgwDQYJKoZI
# hvcNAQELBQADggEBAIZWcjha2TG3r5aFMOMCNNMEN98O+sIbJsUsd2mJcpfbJL96
# QkC+7PPJD4lQPeqwxyF6sGdTnFrmXlJgI4EVm92bRXraZafb4a6ij71czSuVepTf
# H4oq4Xk68ijHP9JsV8NRBJqjRa/cNftkMphaTjDGe2fkv4iIydzyCoWokb1uyBG0
# JgOE0gneqevEtl27MPc1nVcSrQ6P1lRPYfHIKpxNlzxJ+gCV0d4A2Iuap2cZdZl5
# TEdFHMev9CNL2SlOQpvZE6o1Na2Yfe1cwCXPLoH0lCzsAYOR/pP96Ppk+Y5qejPN
# 4m0jiVvY7P2XtsAx2wiYkGPAyOfrAf5KORFHcDehggN5MIICYQIBATCB46GBuaSB
# tjCBszELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcT
# B1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjENMAsGA1UE
# CxMETU9QUjEnMCUGA1UECxMebkNpcGhlciBEU0UgRVNOOkY1MjgtMzc3Ny04QTc2
# MSUwIwYDVQQDExxNaWNyb3NvZnQgVGltZS1TdGFtcCBTZXJ2aWNloiUKAQEwCQYF
# Kw4DAhoFAAMVADoxxGInbGLLojAX1xE9CMdkuZw3oIHCMIG/pIG8MIG5MQswCQYD
# VQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEe
# MBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMQ0wCwYDVQQLEwRNT1BSMScw
# JQYDVQQLEx5uQ2lwaGVyIE5UUyBFU046NTdGNi1DMUUwLTU1NEMxKzApBgNVBAMT
# Ik1pY3Jvc29mdCBUaW1lIFNvdXJjZSBNYXN0ZXIgQ2xvY2swDQYJKoZIhvcNAQEF
# BQACBQDaX7sIMCIYDzIwMTYwMjA2MDAyMTI4WhgPMjAxNjAyMDcwMDIxMjhaMHcw
# PQYKKwYBBAGEWQoEATEvMC0wCgIFANpfuwgCAQAwCgIBAAICBi8CAf8wBwIBAAIC
# GJowCgIFANphDIgCAQAwNgYKKwYBBAGEWQoEAjEoMCYwDAYKKwYBBAGEWQoDAaAK
# MAgCAQACAxbjYKEKMAgCAQACAwehIDANBgkqhkiG9w0BAQUFAAOCAQEAh5FK+2ic
# D/B4c+FFOEQTi7ve980KvuPKRYiJkA8zu95c+tl0arcM/qpeaxC6lUszsWGBaL3j
# jDWPlcRM3pWgEMW4Kb+atcu8xzTGgqfZwypKXHfew0epkPviRvwRp1Yx76wEWtG/
# 6w8J6LcbqOTB7MCMV461qgv1KxY1jNrskrXmfW7qyfNk+XCPaj46RPTyktAk2mkV
# muD7IEJpsNr/oVZR38AH3LLtC2DKC9dfP4jCbF66BonS2n8n46geEyXPeB/BSGkI
# aGFiyZVUcsOFJnLdLbU68nWGgEubsHWixyP95go9IrqJQIyWCEFj+RbDke1aJFc8
# ksrVgZvWqB81DDGCAvUwggLxAgEBMIGTMHwxCzAJBgNVBAYTAlVTMRMwEQYDVQQI
# EwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3Nv
# ZnQgQ29ycG9yYXRpb24xJjAkBgNVBAMTHU1pY3Jvc29mdCBUaW1lLVN0YW1wIFBD
# QSAyMDEwAhMzAAAAcrZ4LtAPVI+IAAAAAAByMA0GCWCGSAFlAwQCAQUAoIIBMjAa
# BgkqhkiG9w0BCQMxDQYLKoZIhvcNAQkQAQQwLwYJKoZIhvcNAQkEMSIEIBcK56uC
# bbHnQuT9Xxslz9igWXgg44Jjq7rxBiSd3gZHMIHiBgsqhkiG9w0BCRACDDGB0jCB
# zzCBzDCBsQQUOjHEYidsYsuiMBfXET0Ix2S5nDcwgZgwgYCkfjB8MQswCQYDVQQG
# EwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwG
# A1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSYwJAYDVQQDEx1NaWNyb3NvZnQg
# VGltZS1TdGFtcCBQQ0EgMjAxMAITMwAAAHK2eC7QD1SPiAAAAAAAcjAWBBTkoMQI
# NwDz+7nBAggFaxFjkbkeMTANBgkqhkiG9w0BAQsFAASCAQARjnTZsN7+2viOW9SB
# EomLclbVJbCd7j0QxJBgB9CnmsShdM+joS8U1XB9ZoCJReAQKqTxh6pRdxegNYit
# KDvEusKVSY3ggooEiRNVk5jsvDgyMP/c7PvlwjDe08HW0xYbKj7V9T0Li+YiaxRU
# W9O6g0DZb+MPa89XP8yOZhocPJg+cb69cQK6AC7r5e/3jCAp1UdF6gdeUjS4WZnB
# cnu+0kCe4ctB2SD+5sEt2QUboJ7MljMXK5qpqhosKVVq6u9JJirFEOglSkeD2cqV
# ym6Eyv2K4S2m157P8OZen6yPbR6F9a001PpqmychkVxix0jQuUrtn9a9oL+IYHgT
# 6wcP
# SIG # End signature block
