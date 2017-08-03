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
	$global:SupportedLocations = 'South Central US', 'North Central US', 'Central US', 'West US', 'East US', 'East US2', 'Japan East', 'Japan West', 'East Asia', 'Southeast Asia'
    
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
# MIIkHwYJKoZIhvcNAQcCoIIkEDCCJAwCAQExDzANBglghkgBZQMEAgEFADB5Bgor
# BgEEAYI3AgEEoGswaTA0BgorBgEEAYI3AgEeMCYCAwEAAAQQH8w7YFlLCE63JNLG
# KX7zUQIBAAIBAAIBAAIBAAIBADAxMA0GCWCGSAFlAwQCAQUABCCm4BQo58ki/lzH
# YA268f1h/w0XPIdrfiB171S1FTw6B6CCDZIwggYQMIID+KADAgECAhMzAAAAZEeE
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
# CrE/+6jMpF3BoYibV3FWTkhFwELJm3ZbCoBIa/15n8G9bW1qyVJzEw16UM0xghXj
# MIIV3wIBATCBlTB+MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQ
# MA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9u
# MSgwJgYDVQQDEx9NaWNyb3NvZnQgQ29kZSBTaWduaW5nIFBDQSAyMDExAhMzAAAA
# ZEeElIbbQRk4AAAAAABkMA0GCWCGSAFlAwQCAQUAoIHOMBkGCSqGSIb3DQEJAzEM
# BgorBgEEAYI3AgEEMBwGCisGAQQBgjcCAQsxDjAMBgorBgEEAYI3AgEVMC8GCSqG
# SIb3DQEJBDEiBCB8atKoxgb1G0bQYYg3Ga0X8xmBu5ztUvNaFyvJdrcMRDBiBgor
# BgEEAYI3AgEMMVQwUqAQgA4AVABvAGwAbABBAHAAcKE+gDxodHRwczovL2F6dXJl
# Lm1pY3Jvc29mdC5jb20vZW4tdXMvc2VydmljZXMvc3RyZWFtLWFuYWx5dGljcy8w
# DQYJKoZIhvcNAQEBBQAEggEAiVtMcMHK/oYqcvu5R4N00RbWtMiVv4PD8BdwIRKx
# /0QbSEDW7rln/rk8xExf8jbEpu+ULEk0UA5OopRdLh1WV0338Ggv9uQjvTIjcQLe
# W9wDv78Fr4fLjJJCISxiNSwgKTovXMmeU70s9ee9Qw07u6UX5ARaAN+lrfelu5nh
# KRCC95MXQHkJRFZS6ueGYof2p/NR75W/DvbwGS1IpgnJrMBqrfzaBq9fGQvVKlA9
# RgZvuKEED3mLH0UdgbGQ+8xcKuYQx6e2uELoahSKfFui5bxYBC0GNPcPw9BHUjzE
# Df8dA8kl+agIJ5fxRYN8ZODzg7Ybm89ozC0+PGtSMOIhnKGCE00wghNJBgorBgEE
# AYI3AwMBMYITOTCCEzUGCSqGSIb3DQEHAqCCEyYwghMiAgEDMQ8wDQYJYIZIAWUD
# BAIBBQAwggE9BgsqhkiG9w0BCRABBKCCASwEggEoMIIBJAIBAQYKKwYBBAGEWQoD
# ATAxMA0GCWCGSAFlAwQCAQUABCC88BdBmhmRrVXgQm5UWYgHs0n7uSFLvV0E7E7l
# FN521wIGVqt9vmn/GBMyMDE2MDIxMDAyNDIzMy42OTZaMAcCAQGAAgH0oIG5pIG2
# MIGzMQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMH
# UmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMQ0wCwYDVQQL
# EwRNT1BSMScwJQYDVQQLEx5uQ2lwaGVyIERTRSBFU046QkJFQy0zMENBLTJEQkUx
# JTAjBgNVBAMTHE1pY3Jvc29mdCBUaW1lLVN0YW1wIFNlcnZpY2Wggg7QMIIGcTCC
# BFmgAwIBAgIKYQmBKgAAAAAAAjANBgkqhkiG9w0BAQsFADCBiDELMAkGA1UEBhMC
# VVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNV
# BAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEyMDAGA1UEAxMpTWljcm9zb2Z0IFJv
# b3QgQ2VydGlmaWNhdGUgQXV0aG9yaXR5IDIwMTAwHhcNMTAwNzAxMjEzNjU1WhcN
# MjUwNzAxMjE0NjU1WjB8MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3Rv
# bjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0
# aW9uMSYwJAYDVQQDEx1NaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0EgMjAxMDCCASIw
# DQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAKkdDbx3EYo6IOz8E5f1+n9plGt0
# VBDVpQoAgoX77XxoSyxfxcPlYcJ2tz5mK1vwFVMnBDEfQRsalR3OCROOfGEwWbEw
# RA/xYIiEVEMM1024OAizQt2TrNZzMFcmgqNFDdDq9UeBzb8kYDJYYEbyWEeGMoQe
# dGFnkV+BVLHPk0ySwcSmXdFhE24oxhr5hoC732H8RsEnHSRnEnIaIYqvS2SJUGKx
# Xf13Hz3wV3WsvYpCTUBR0Q+cBj5nf/VmwAOWRH7v0Ev9buWayrGo8noqCjHw2k4G
# kbaICDXoeByw6ZnNPOcvRLqn9NxkvaQBwSAJk3jN/LzAyURdXhacAQVPIk0CAwEA
# AaOCAeYwggHiMBAGCSsGAQQBgjcVAQQDAgEAMB0GA1UdDgQWBBTVYzpcijGQ80N7
# fEYbxTNoWoVtVTAZBgkrBgEEAYI3FAIEDB4KAFMAdQBiAEMAQTALBgNVHQ8EBAMC
# AYYwDwYDVR0TAQH/BAUwAwEB/zAfBgNVHSMEGDAWgBTV9lbLj+iiXGJo0T2UkFvX
# zpoYxDBWBgNVHR8ETzBNMEugSaBHhkVodHRwOi8vY3JsLm1pY3Jvc29mdC5jb20v
# cGtpL2NybC9wcm9kdWN0cy9NaWNSb29DZXJBdXRfMjAxMC0wNi0yMy5jcmwwWgYI
# KwYBBQUHAQEETjBMMEoGCCsGAQUFBzAChj5odHRwOi8vd3d3Lm1pY3Jvc29mdC5j
# b20vcGtpL2NlcnRzL01pY1Jvb0NlckF1dF8yMDEwLTA2LTIzLmNydDCBoAYDVR0g
# AQH/BIGVMIGSMIGPBgkrBgEEAYI3LgMwgYEwPQYIKwYBBQUHAgEWMWh0dHA6Ly93
# d3cubWljcm9zb2Z0LmNvbS9QS0kvZG9jcy9DUFMvZGVmYXVsdC5odG0wQAYIKwYB
# BQUHAgIwNB4yIB0ATABlAGcAYQBsAF8AUABvAGwAaQBjAHkAXwBTAHQAYQB0AGUA
# bQBlAG4AdAAuIB0wDQYJKoZIhvcNAQELBQADggIBAAfmiFEN4sbgmD+BcQM9naOh
# IW+z66bM9TG+zwXiqf76V20ZMLPCxWbJat/15/B4vceoniXj+bzta1RXCCtRgkQS
# +7lTjMz0YBKKdsxAQEGb3FwX/1z5Xhc1mCRWS3TvQhDIr79/xn/yN31aPxzymXlK
# kVIArzgPF/UveYFl2am1a+THzvbKegBvSzBEJCI8z+0DpZaPWSm8tv0E4XCfMkon
# /VWvL/625Y4zu2JfmttXQOnxzplmkIz/amJ/3cVKC5Em4jnsGUpxY517IW3DnKOi
# PPp/fZZqkHimbdLhnPkd/DjYlPTGpQqWhqS9nhquBEKDuLWAmyI4ILUl5WTs9/S/
# fmNZJQ96LjlXdqJxqgaKD4kWumGnEcua2A5HmoDF0M2n0O99g/DhO3EJ3110mCII
# YdqwUB5vvfHhAN/nMQekkzr3ZUd46PioSKv33nJ+YWtvd6mBy6cJrDm77MbL2IK0
# cs0d9LiFAR6A+xuJKlQ5slvayA1VmXqHczsI5pgt6o3gMy4SKfXAL1QnIffIrE7a
# KLixqduWsqdCosnPGUFN4Ib5KpqjEWYw07t0MkvfY3v1mYovG8chr1m1rtxEPJdQ
# cdeh0sVV42neV8HR3jDA/czmTfsNv11P6Z0eGTgvvM9YBS7vDaBQNdrvCScc1bN+
# NR4Iuto229Nfj950iEkSMIIE2jCCA8KgAwIBAgITMwAAAIMoFt5mvLbb2AAAAAAA
# gzANBgkqhkiG9w0BAQsFADB8MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGlu
# Z3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBv
# cmF0aW9uMSYwJAYDVQQDEx1NaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0EgMjAxMDAe
# Fw0xNTEwMjgyMDQwMTZaFw0xNzAxMjgyMDQwMTZaMIGzMQswCQYDVQQGEwJVUzET
# MBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMV
# TWljcm9zb2Z0IENvcnBvcmF0aW9uMQ0wCwYDVQQLEwRNT1BSMScwJQYDVQQLEx5u
# Q2lwaGVyIERTRSBFU046QkJFQy0zMENBLTJEQkUxJTAjBgNVBAMTHE1pY3Jvc29m
# dCBUaW1lLVN0YW1wIFNlcnZpY2UwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEK
# AoIBAQCsgDFTfB/zb3Da4bBAZtUbxhd/hvc6DBfH6M6Ag64NiOnIHkJ2/KnrZQ8J
# wDSiZrmvHeMqD3Y9TmTXz3CoWRbReT4wmelqMLB5+22JwqBSy0zGu62lISgUe6d9
# LrouYdTAqMr2Vu/oPNq3AmcVYvdHfUrweWoWZrwQvI1r809UB20WRjfsy2snhesN
# 0LL7utYoxWdriPMGIdgCZz72/hd5V7c2StmNlGWhl8xXDuqqeG+UeNJKUCECV9Yw
# bpUYI3okrq7jhD9W3WgGaUKLMGaeqZNDlZQ9SbGpLwmFDigVcquwcfBg/HaOtVxN
# ibUtVpKW+f8q0yXwKZNbjigeVgWRAgMBAAGjggEbMIIBFzAdBgNVHQ4EFgQUnkDJ
# nhJQDMfal31fnl832wQSnCswHwYDVR0jBBgwFoAU1WM6XIoxkPNDe3xGG8UzaFqF
# bVUwVgYDVR0fBE8wTTBLoEmgR4ZFaHR0cDovL2NybC5taWNyb3NvZnQuY29tL3Br
# aS9jcmwvcHJvZHVjdHMvTWljVGltU3RhUENBXzIwMTAtMDctMDEuY3JsMFoGCCsG
# AQUFBwEBBE4wTDBKBggrBgEFBQcwAoY+aHR0cDovL3d3dy5taWNyb3NvZnQuY29t
# L3BraS9jZXJ0cy9NaWNUaW1TdGFQQ0FfMjAxMC0wNy0wMS5jcnQwDAYDVR0TAQH/
# BAIwADATBgNVHSUEDDAKBggrBgEFBQcDCDANBgkqhkiG9w0BAQsFAAOCAQEAByq1
# zwZDgtDAHie1pSElusdmegsQVJ3BIZnoKPk8d6h2D1aXJCCvlVxksRxw+5/LT25Z
# u9QPqE529LUtWdKqxKDEIn34cG/+P4078rV/FAFOJXxLAstzynvl6MI3bkWY5yhO
# BJq1AeeKdllCCbkDaAUhrfYElqOlAFvZLfohqzDVmkVYoao9VOsdq2ClJhFqEvVm
# 96DFWGdcuafWKtPfDKo00pydRlGBH5RiWJxIBM0Z4n7D6ZqtOal8OR43CWJ+tU4z
# GDy3yKPO+22FlfHNv0l0NTBvLp/q2VD7mpAQ/CCrwOjNUlrmnZqqIekX2lz8c21j
# I6xkCjyhB58uL95ATaGCA3kwggJhAgEBMIHjoYG5pIG2MIGzMQswCQYDVQQGEwJV
# UzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UE
# ChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMQ0wCwYDVQQLEwRNT1BSMScwJQYDVQQL
# Ex5uQ2lwaGVyIERTRSBFU046QkJFQy0zMENBLTJEQkUxJTAjBgNVBAMTHE1pY3Jv
# c29mdCBUaW1lLVN0YW1wIFNlcnZpY2WiJQoBATAJBgUrDgMCGgUAAxUA3BPwHTLK
# xPofZ9cfcgR2d+CwJ0aggcIwgb+kgbwwgbkxCzAJBgNVBAYTAlVTMRMwEQYDVQQI
# EwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3Nv
# ZnQgQ29ycG9yYXRpb24xDTALBgNVBAsTBE1PUFIxJzAlBgNVBAsTHm5DaXBoZXIg
# TlRTIEVTTjo1N0Y2LUMxRTAtNTU0QzErMCkGA1UEAxMiTWljcm9zb2Z0IFRpbWUg
# U291cmNlIE1hc3RlciBDbG9jazANBgkqhkiG9w0BAQUFAAIFANplAj4wIhgPMjAx
# NjAyMTAwMDI2MzhaGA8yMDE2MDIxMTAwMjYzOFowdzA9BgorBgEEAYRZCgQBMS8w
# LTAKAgUA2mUCPgIBADAKAgEAAgITMAIB/zAHAgEAAgIYADAKAgUA2mZTvgIBADA2
# BgorBgEEAYRZCgQCMSgwJjAMBgorBgEEAYRZCgMBoAowCAIBAAIDFuNgoQowCAIB
# AAIDB6EgMA0GCSqGSIb3DQEBBQUAA4IBAQBZYXFD+87jUSX5RJijS/agEdVLxAUV
# 8fREGA2B4lIJudY35ELxG4OWgPIuoFY9r8/FnQMF2qDGEF+lEXfEF1owP7xXJbx5
# TnA3OlzxJWHVjkKdnsp7FC1pnamnJ+xxr6Q10zm68cequzIK9XaFWW2692/5FgeM
# yTHNNtrt8oH4ZseCXr+orpWE6Dxe1zL9tniwSp3kTt9rBFDtehGQD7HGFNGInQl5
# iDDx0GvWRHf8Ud3cbDFu0iJn3X9aw6WqobOUs0EP/pknTgXTbDxqjdgqhNbpIZ/B
# l5PBUk6BjBv0/9Lx0FKa+qhgQrCGvHwJ3oq8rdnd1eBP0f8RedT/d3PVMYIC9TCC
# AvECAQEwgZMwfDELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAO
# BgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEm
# MCQGA1UEAxMdTWljcm9zb2Z0IFRpbWUtU3RhbXAgUENBIDIwMTACEzMAAACDKBbe
# Zry229gAAAAAAIMwDQYJYIZIAWUDBAIBBQCgggEyMBoGCSqGSIb3DQEJAzENBgsq
# hkiG9w0BCRABBDAvBgkqhkiG9w0BCQQxIgQgat8o5A5DulfhxGYzpePBujwvoy6q
# TycF7Ubzb7b46y0wgeIGCyqGSIb3DQEJEAIMMYHSMIHPMIHMMIGxBBTcE/AdMsrE
# +h9n1x9yBHZ34LAnRjCBmDCBgKR+MHwxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpX
# YXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQg
# Q29ycG9yYXRpb24xJjAkBgNVBAMTHU1pY3Jvc29mdCBUaW1lLVN0YW1wIFBDQSAy
# MDEwAhMzAAAAgygW3ma8ttvYAAAAAACDMBYEFIIeSn04JWGn53YcLsQGjAfDTsOH
# MA0GCSqGSIb3DQEBCwUABIIBAGrybGAIDZP0jwGjvoVk2Y2V8+5opB5egxl/Ic5L
# AH8Bbu2wLPl3PzkqLi/cE74tHroC6OP6nti2JhG+EmyJOrlTjkqgDiTTZG0Zs7pj
# hiU2Gt4WJauMaRCQ9B5JDxNwNV4wcre7iJcPM7CVat1vpD/98HBoqbGZyOAo3026
# KKCQQqFT7oelG8fLYtnVB0DsFIjhmiuC4dwt4SejIP9ocdwSuRJj2njCaJ0xF0Qh
# pSKzcko+1ZUyPrTn4jOrX9JBcbHuNWn9Z1KsV9Iveb99yP9kRT7DCpzG2fLfo0Ea
# Vv9HdWzWTTGifR8gC7y86xsNKNwnwZMBfrtlZ54xBKD+SAo=
# SIG # End signature block
