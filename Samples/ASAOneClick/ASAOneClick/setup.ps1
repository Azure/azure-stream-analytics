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
   [Alias("subscriptionID")]
   [string]$global:subscriptionID,

   [Parameter()]
   [Alias("usecase")]
   [string]$global:useCaseName,

   [Parameter()]
   [Alias("mode")]
   [string]$global:mode,

   [Parameter()]
   [Alias("sqllogin")]
   [string]$global:sqlServerLogin = 'asauser',

   [Parameter()]
   [Alias("sqlpassword")]
   [string]$global:sqlServerPassword = '1Burger!'   
)

# global variables
#[string]$global:subscriptionDefaultAccount
#[string]$global:defaultResourceName
#[string]$global:resourceGroupName 
#[string]$global:location
#[string]$global:inputEventHubNameset
#[string]$global:outputEventHubName
#[string]$global:ServiceBusNamespace
#[string]$global:sqlDBName
#[string]$global:configPath

function Update-JSONFile( $file ){
	#Write-Host  -foreground green (Get-Date)   "Updating [$file]"

	(Get-Content $file ) | Foreach-Object {
		$_  -replace '<azuredbname>', $global:dict["<azuredbname>"] `
 		    -replace '<dbname>', $global:dict["<dbname>"] `
 		    -replace '<userid>', $global:dict["<userid>"] `
 		    -replace '<password>', $global:dict["<password>"] `
			

	} | Set-Content  $file

}

function Update-ASAJSONFile( $file ){
	Write-Host  -foreground green (Get-Date)   "Updating [$file]"

	(Get-Content $file ) | Foreach-Object {
		$_  -replace '<inputEventHubName>', $global:dict["<inputEventHubName>"] `
            -replace '<outputEventHubName>', $global:dict["<outputEventHubName>"] `
 		    -replace '<servicebusnamespace>', $global:dict["<servicebusnamespace>"] `
 		    -replace '<sharedaccesspolicykey>', $global:dict["<sharedaccesspolicykey>"] `
 		    -replace '<sharedaccesspolicyname>', $global:dict["<sharedaccesspolicyname>"] `
			-replace '<sqlserver>', $global:dict["<azuredbname>"] `
			-replace '<dbname>', $global:dict["<dbname>"] `
 		    -replace '<userid>', $global:dict["<userid>"] `
 		    -replace '<password>', $global:dict["<password>"] `
		
	} | Set-Content  $file

}

function ValidateParameters{
    $modeList = 'deploy', 'delete'

    if($modeList -notcontains $global:mode){
        Write-Host ''
        Write-Host 'MISSING REQUIRED PARAMETER: -mode parameter must be set to one of: ' $modeList
        $global:mode = Read-Host 'Enter mode'
        while($modeList -notcontains $global:mode){
            Write-Host 'Invalid mode. Please enter a mode from the list above.'
            $global:mode = Read-Host 'Enter mode'                     
        }
    }

    $useCaseNameList = 'sensoralerts'
       
    $global:useCaseName='sensoralerts'

    if($useCaseNameList -notcontains $global:useCaseName.ToLower()){
        Write-Host ''
        Write-Host 'MISSING REQUIRED PARAMETER: -usecase parameter must be set to one of: ' $useCaseNameList
        $global:useCaseName = Read-Host 'Enter use case name'
        while($useCaseNameList -notcontains $global:useCaseName){
            Write-Host 'Invalid use case name. Please enter a name from the list above.'
            $global:useCaseName = Read-Host 'Enter use case name'                     
        }
    }

    Write-Host
    Write-Host '------------------------------------------'
    Write-Host 'Mode: ' $global:mode
    Write-Host 'Use Case: ' $global:useCaseName   
    Write-Host '------------------------------------------'
}

function SetGlobalParams {
    #this function must be called after ValidateParams
    

    [string]$global:location = 'SouthCentralUS'
    [string]$global:locationMultiWord = 'South Central US'

    $output = ($global:useCaseName + $env:COMPUTERNAME).Replace('-','').Replace('_','').ToLower()
    if($output.Length -gt 24) { $output = $output.Remove(23) }

    $global:useCaseName = $global:useCaseName.ToLower()

    $global:defaultResourceName = $output 
	$global:inputEventHubName = $output+'input'
    $global:outputEventHubName = $output+'output'
	$global:ServiceBusNamespace = $output
    
    $global:resourceGroupName = $global:useCaseName

    $global:sqlserverName = ""
    $global:sqlDBName = $global:useCaseName
    

    $global:configPath = ($PSScriptRoot+'\temp\setup\' + $global:useCaseName + '.txt')

    $global:dict = @{}    



}

function SetMappingDictionary {
    #map the global vars to the dictionary  used for string substitution

    $global:dict.Add('<azuredbname>',$global:sqlserverName)
    $global:dict.Add('<userid>', $global:sqlServerLogin)
    $global:dict.Add('<password>', $global:sqlServerPassword)
    $global:dict.Add('<dbname>',$global:sqlDBName)
	$global:dict.Add('<usecase>',$global:useCaseName)
	$global:dict.Add('<inputEventHubName>',$global:inputEventHubName)
    $global:dict.Add('<outputEventHubName>',$global:outputEventHubName)
	$global:dict.Add('<servicebusnamespace>',$global:ServiceBusNamespace)
	$global:dict.Add('<sharedaccesspolicyname>',$global:sharedaccesskeyname)
	$global:dict.Add('<sharedaccesspolicykey>',$global:sharedaccesskey)


}

function InitSubscription{
       
    #Remove all Azure Account from Cache if any
    Get-AzureAccount | ForEach-Object { Remove-AzureAccount $_.ID -Force -WarningAction SilentlyContinue }

    #login
    Add-AzureAccount -WarningAction SilentlyContinue | out-null
    $account = Get-AzureAccount
	Write-Host You are signed-in with $account.id

    if($global:subscriptionID -eq $null -or $global:subscriptionID -eq ''){
        $subList = Get-AzureSubscription

        if($subList.Length -lt 1){
            throw 'Your azure account does not have any subscriptions.  A subscription is required to run this tool'
        } 

        $subCount = 0
        foreach($sub in $subList){
            $subCount++
            $sub | Add-Member -type NoteProperty -name RowNumber -value $subCount
        }

        Write-Host ''
        Write-Host 'Your Azure Subscriptions: '
       
        $subList | Format-Table RowNumber,SubscriptionId,SubscriptionName -AutoSize
        $rowNum = Read-Host 'Enter the row number (1 -'$subCount') of a subscription'

        while( ([int]$rowNum -lt 1) -or ([int]$rowNum -gt [int]$subCount)){
            Write-Host 'Invalid subscription row number. Please enter a row number from the list above'
            $rowNum = Read-Host 'Enter subscription row number'                     
        }
        $supportedMode=$subList[$rowNum-1].SupportedModes

        #Checks if the subscription supports ASM. There are some subscriptions which supports only ARM and they will not work.
        while(-not $supportedMode.Contains('AzureServiceManagement')){
            Write-Host 'The selected subscription does not support AzureServiceManagement. Please select a different subscription'
            $rowNum = Read-Host 'Enter subscription row number' 
            $supportedMode=$subList[$rowNum-1].SupportedModes
        }

       
        $global:subscriptionID = $subList[$rowNum-1].SubscriptionId;
        $global:subscriptionDefaultAccount = $subList[$rowNum-1].DefaultAccount.Split('@')[0]
    }
    
    #switch to appropriate subscription
    try{
        Select-AzureSubscription -SubscriptionId $global:subscriptionID
    } 
    catch{
        throw 'Subscription ID provided is invalid: ' + $global:subscriptionID    
    }

    if($global:mode -eq 'deploy'){        
        #Register subscription for Azure Stream Analytics
        RegisterAzureProvider('Microsoft.StreamAnalytics')
    } 
    

}

function RegisterAzureProvider($providerNamespace){

    try{
        Switch-AzureMode -Name AzureResourceManager
        Register-AzureProvider -Force -ProviderNamespace $providerNamespace 
        Write-Host 'Subscription ' $global:subscriptionID 'successfully registered to ' $providerNamespace
        
    } 
    catch{
        Write-Host 'error.'
        throw 
    }

}

function CreateResourceGroup{
    Switch-AzureMode -Name AzureResourceManager
    #create resource group
    $rg = $null
    try{
        Write-Host 'Creating Resource Group [' $global:resourceGroupName ']......' -NoNewline
        $rg = New-AzureResourceGroup -Name ($global:resourceGroupName) -location $global:locationMultiword -ErrorAction Stop -Force | out-null
        #will update if already exists
    } 
    catch{
        Write-Host 'error.' 
        throw
    }
    Write-Host 'created.'
    return $rg
}

function DeleteResourceGroup{
    Switch-AzureMode AzureResourceManager
    try{
        Write-Host 'Deleting Resource Group [' $global:resourceGroupName ']......Continue (Y/N)?' -NoNewline
        $ans=Read-Host
        if($ans.ToLower() -eq 'y'){
            Remove-AzureResourceGroup -Name $global:resourceGroupName -ErrorAction Stop -Force
            Write-Host 'deleted.'
        }
    }
    catch [ArgumentException]{
        # resource group does not exist        
    } 
    catch {
        Write-Host 'error.'
    }

    
}

function CreateSQLServerAndDB{
    process{
        Write-Host 'Creating SQL Server ...... ' -NoNewline
        Switch-AzureMode AzureServiceManagement
        
        #create sql server & DB
        $sqlsvr = $null
        $createdNew = $FALSE
        try{ 
            $sqlsvrname = Get-Content -Path $global:configPath -ErrorAction SilentlyContinue -WarningAction SilentlyContinue 
            $global:sqlserverName = $sqlsvrname
            
            if($sqlsvrname -eq $null -or $sqlsvrname.Length -le 1){
                $sqlsvr = New-AzureSqlDatabaseServer -location $global:locationMultiWord -AdministratorLogin $global:sqlServerLogin -AdministratorLoginPassword $global:sqlServerPassword
                if($sqlsvr -eq $null){
                    throw $Error
                }

                $sqlsvrname = $sqlsvr.ServerName
                Set-Content -Path $global:configPath -Value $sqlsvrname
                $createdNew = $TRUE;
                
                Write-Host '[svr name: ' $sqlsvrname ']....created.' 
                $global:sqlserverName = $sqlsvrname

                #Setting firewall rule
                $rule = New-AzureSqlDatabaseServerFirewallRule -ServerName $sqlsvr.ServerName -RuleName “demorule” -StartIPAddress “0.0.0.0” -EndIPAddress “255.255.255.255” -ErrorAction SilentlyContinue
                
                


            }
            else{
                Get-AzureSqlDatabaseServer -ServerName $sqlsvrname
                Write-Host '[svr name: ' $sqlsvrname ']......already exists.'
            }

            $check = Get-AzureSqlDatabase -ServerName $global:sqlserverName -DatabaseName $global:sqlDBName -ErrorAction SilentlyContinue -WarningAction SilentlyContinue

            if(!$check){

                #Creating Database
                Write-Host 'Creating SQL DB [' $global:sqlDBName ']......' -NoNewline 
                $servercredential = new-object System.Management.Automation.PSCredential($sqlServerLogin, ($sqlServerPassword  | ConvertTo-SecureString -asPlainText -Force))
                #create a connection context
                $ctx = New-AzureSqlDatabaseServerContext -ServerName $sqlsvrname -Credential $serverCredential
                $sqldb = New-AzureSqlDatabase $ctx –DatabaseName $global:sqlDBName -Edition Basic | out-null
                Write-Host 'created.'
            }
            else{
                Write-Host 'The database [' $global:sqlDBName '] already exists'
            }
        } 
        catch{        
            Write-Host 'error.'
            throw
        }


        return $sqldb
    }
}

function DeleteSQLServerAndDB{
    Switch-AzureMode AzureServiceManagement
    $sqlsvrname = Get-Content $global:configPath
    if($sqlsvrname.Length -gt 0) {
        try{
            Write-Host 'Deleting SQL Svr [' $sqlsvrname '] & SQL DB [' $global:sqlDBName ']......Continue (Y/N)?' -NoNewline
            $ans=Read-Host
            if($ans.ToLower() -eq 'y'){ 
                Remove-AzureSqlDatabaseServer -ServerName $sqlsvrname -Force
                Set-Content -Path $global:configPath -Value ''
                Write-Host 'deleted.' 
            }
        } 
        catch [InvalidOperationException] {
            #thrown when svr doesnt exist
            Write-Host 'doesnt exist.'
        } 
        catch{
            Write-Host 'error.'
        }
    }    
}

function CreateASAJob{
    Write-Host "Preparing ASA job config file......" -NoNewline
	copy -Path "src\\$global:useCaseName\\ASAJob\\ASAJob.json" -Destination temp\json\asa\ -Force
    $files = Get-ChildItem "temp\json\asa\*" -Include ASAJob.json -Recurse -ErrorAction Stop
	
    foreach($file in $files){
        Update-ASAJSONFile  $file.FullName      
    }
	Write-Host 'Prepared.'
	
	Write-Host "Creating the ASA Job......" -NoNewline
	try{
        Switch-AzureMode AzureResourceManager
        $ASAJob = New-AzureStreamAnalyticsJob -File "temp\json\asa\ASAjob.json" -Name $global:defaultResourceName -ResourceGroupName $global:resourceGroupName -ErrorAction Stop
	    if ($ASAJob.JobName -eq $global:defaultResourceName){
            Write-Host 'Created.'
	    }
    }
    catch{
        Write-Host 'Error.'
        throw
    }
}

function DeleteASAJob{
    Process{
        Switch-AzureMode AzureResourceManager
        $retryCount = 2;
        $done = $false
        $errormsg = ''
        while($retryCount -gt 0 -and $done -ne $true){
            try{
                Write-Host 'Deleting ASA Job [' $global:defaultResourceName ']......Continue (Y/N)?' -NoNewline
                $ans=Read-Host
                if($ans.ToLower() -eq 'y'){
                    Remove-AzureStreamAnalyticsJob -Name $global:defaultResourceName -ResourceGroupName $global:resourceGroupName -Force -ErrorAction Stop | out-null
                    $done = $true
                    Write-Host 'Deleted.'     
                }           
            } 
            catch{
                if($error[0].Exception.ToString().contains('ResourceGroupNotFound')){
                    Write-Host 'Doesnt exist.'
                    $retryCount = 0
                    $done = $true
                } 
                else{
                    Write-Host 'Error. retrying delete....'
                    $errormsg = $error[0].Exception.Message
                }                         
            }
            $retryCount--; 
        }
        if($done -eq $false){ 
            Write-Host 'error.' $errormsg
        }
    }
}

function CreateEventHubandSBNamespace{
  
    Process{
        Switch-AzureMode AzureServiceManagement
        $namespace = $null
		$eventhub = $null
		
        try{
            Write-Host "Creating EventHub and ServiceBus Namespace"
			# WARNING: Make sure to reference the latest version of the \Microsoft.ServiceBus.dll
			$namespace = Get-AzureSBNamespace -Name $global:ServiceBusNamespace 
			if($namespace.name -eq $null){
			    Write-Host 'Creating the Service Bus Namespace ['$global:ServiceBusNamespace ']......' -NoNewline
                #create a new Service Bus Namespace
                $namespace = New-AzureSBNamespace -Name $global:ServiceBusNamespace -Location $global:locationMultiWord -CreateACSNamespace $true -NamespaceType Messaging -ErrorAction Stop
			    Write-Host 'created.'
			}
			else{
			    Write-Host 'The ['$global:ServiceBusNamespace '] namespace in the ['$global:locationMultiWord '] region already exists.'
			}
	    }
        catch{
            Write-Host 'error.'
            throw
        }
               
        try{		
			#Create the NamespaceManager object to create the event hub
			$currentnamespace = Get-AzureSBNamespace -Name $global:ServiceBusNamespace
			Write-Host 'Creating a NamespaceManager object for the ['$global:ServiceBusNamespace '] namespace...' -NoNewline
            $filepath = $PSScriptRoot + "\src\\$global:useCaseName\\SensorEventGenerator\*"
            Unblock-File -Path $filepath
			$nsMgrType = [System.Reflection.Assembly]::LoadFrom($PSScriptRoot +"\src\\$global:useCaseName\\SensorEventGenerator\Microsoft.ServiceBus.dll").GetType("Microsoft.ServiceBus.NamespaceManager")
			
            $namespacemanager = $nsMgrType::CreateFromConnectionString($currentnamespace.ConnectionString);
			Write-Host 'created.'
			Write-Host 'Creating the Input EventHub ['$global:inputEventHubName ']......' -NoNewline
			$eventhub = $namespacemanager.CreateEventHubIfNotExists($global:inputEventHubName)
			Write-Host 'created.'
            $global:constring = Get-AzureSBAuthorizationRule -Namespace $global:ServiceBusNamespace
			$constringparse = $global:constring.ConnectionString.Split(';')
			$global:sharedaccesskeyname = $constringparse[1].Substring(20)
			$global:sharedaccesskey = $constringparse[2].Substring(16)
			Write-Host 'Event Hub ConnectionString: ['$global:constring.ConnectionString']'
        }
        catch{
            Write-Host 'error.'
		    throw
        }
        
		
    }
}

function DeleteEventHubandSBNamespace{
    Switch-AzureMode AzureServiceManagement	
	try{
        $currentnamespace = Get-AzureSBNamespace -Name $global:ServiceBusNamespace
	}
	catch{
        Write-Host Azure Service Bus Namespace: $global:ServiceBusNamespace not found! 
	}

	if ($currentnamespace){  
        Write-Host 'Deleting ServiceBus Namespace [' $global:ServiceBusNamespace ']......Continue (Y/N)?' -NoNewline
        $ans=Read-Host
        if($ans.ToLower() -eq 'y'){
            try{
                Remove-AzureSBNamespace -Name $global:ServiceBusNamespace -Force
                Write-Host 'deleted'.
            }
            catch{
                Write-Host 'error'.
                throw
            }
        }
	 }
	 else{
        Write-Host The namespace $global:ServiceBusNamespace does not exists.  
	 }
}

function WriteAccountInformation {
   $accountFile = "$global:useCaseName-accounts.txt"
   copy -Path "src\\account-template.txt" -Destination ".\\$accountFile" -Force
   Update-JSONFile ".\\$accountFile"
}

function PopulateSensorAlerts{
    Write-Host "Populating Sensor Alerts resources"

	Write-Host 'Preparing Data Generator config file......' -NoNewline
	$datgenconfig = ($PSScriptRoot + "\src\\$global:useCaseName\\SensorEventGenerator\\SensorEventGenerator.exe.config")
	[xml] $doc = Get-Content $datgenconfig
	
	$doc.SelectSingleNode('//appSettings/add[@key="EventHubName"]/@value').'#text' = $global:inputEventHubName
	$doc.SelectSingleNode('//appSettings/add[@key="EventHubConnectionString"]/@value').'#text' = $global:constring.ConnectionString 
	$doc.Save($datgenconfig)
	Write-Host 'prepared.'
	
    #Call function to Create ASA Job
    CreateASAJob

    Write-Host "Preparing the Azure SQL Database and creating tables"
    sqlcmd -S "$global:sqlserverName.database.windows.net" -U $global:sqlServerLogin@$global:sqlserverName -P $global:sqlServerPassword -i "src\\$global:useCaseName\\scripts\\sensoralertssqldb.sql" -d $global:sqlDBName
   
    #Starting the ASA Job
    Write-Host Starting the ASA Job [$global:defaultResourceName]. This may take few minutes.....
    try{
        $StartASAJob = Start-AzureStreamAnalyticsJob -Name $global:defaultResourceName -ResourceGroupName $global:resourceGroupName
        if ($StartASAJob -eq "True"){
		    Write-Host 'started.'
        }
    }
    catch{
        Write-Host 'error.'
        throw  
    }
   
  
}

function CreateSensorAlertsResources{
    Write-Host "Creating resources for Sensor Alerts"
    CreateResourceGroup
    CreateEventHubandSBNamespace
    CreateSQLServerAndDB
    
}

function DeleteSensorAlertsResources{
   	DeleteEventHubandSBNamespace
    DeleteSQLServerAndDB
    DeleteASAJob
    DeleteResourceGroup
	
}

#start of main script
$storePreference = $Global:VerbosePreference
$debugPreference= $Global:DebugPreference

ValidateParameters
SetGlobalParams
InitSubscription   

$Global:VerbosePreference = "SilentlyContinue"
$Global:DebugPreference="SilentlyContinue"

switch($global:useCaseName){
    'sensoralerts'{
        switch($global:mode){
            'deploy'{
                    CreateSensorAlertsResources
                    SetMappingDictionary
                    WriteAccountInformation
                    PopulateSensorAlerts
            }
            'delete'{
                    DeleteSensorAlertsResources
            }
        }
    }
}


$Global:VerbosePreference = $storePreference
$Global:DebugPreference= $debugPreference






# SIG # Begin signature block
# MIIkJwYJKoZIhvcNAQcCoIIkGDCCJBQCAQExDzANBglghkgBZQMEAgEFADB5Bgor
# BgEEAYI3AgEEoGswaTA0BgorBgEEAYI3AgEeMCYCAwEAAAQQH8w7YFlLCE63JNLG
# KX7zUQIBAAIBAAIBAAIBAAIBADAxMA0GCWCGSAFlAwQCAQUABCBUoI5KDy9eAtN7
# JWaBJw8ZiZz4upyseJD8aAloJEaWSKCCDZIwggYQMIID+KADAgECAhMzAAAAZEeE
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
# CrE/+6jMpF3BoYibV3FWTkhFwELJm3ZbCoBIa/15n8G9bW1qyVJzEw16UM0xghXr
# MIIV5wIBATCBlTB+MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQ
# MA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9u
# MSgwJgYDVQQDEx9NaWNyb3NvZnQgQ29kZSBTaWduaW5nIFBDQSAyMDExAhMzAAAA
# ZEeElIbbQRk4AAAAAABkMA0GCWCGSAFlAwQCAQUAoIHWMBkGCSqGSIb3DQEJAzEM
# BgorBgEEAYI3AgEEMBwGCisGAQQBgjcCAQsxDjAMBgorBgEEAYI3AgEVMC8GCSqG
# SIb3DQEJBDEiBCCfe7yObBFmiWZe5SzHmqDC8c0jh93CMfbZi3ho7lgBYTBqBgor
# BgEEAYI3AgEMMVwwWqAYgBYAQQBTAEEATwBuAGUAQwBsAGkAYwBroT6APGh0dHBz
# Oi8vYXp1cmUubWljcm9zb2Z0LmNvbS9lbi11cy9zZXJ2aWNlcy9zdHJlYW0tYW5h
# bHl0aWNzLzANBgkqhkiG9w0BAQEFAASCAQBw/xlsdoYsXqOMoUGe4LhMOL+D67j4
# +2fH9T6cE0D19ru8ByvFTmpjSZnxn5bx7BJAIftV8ypJxsFGijseD96cQ0oUtrG4
# aDCjI6f6Ouiz0M18QjyPl1ydpmxiK6NrAnDf9MT7KBr/NUFeE4V8Yr5oxxo3xydv
# ZgxSoAotl/jyZcNSLLnOBrOi6Z2Pb0WGNYsMcb9n1q7VPN8jrguNObEsnMhTb80p
# D9nxJozsHNEcoRMMvZ07G+n4tMYKUySXsQrNWatB3m6qXxej5U3SDVW6jw22vdC3
# tzXDnT3jjLjyYDJDk1Vi+aC3uNE1Y4Qs8yPxYNXqG3S6zglqwoNbinlBoYITTTCC
# E0kGCisGAQQBgjcDAwExghM5MIITNQYJKoZIhvcNAQcCoIITJjCCEyICAQMxDzAN
# BglghkgBZQMEAgEFADCCAT0GCyqGSIb3DQEJEAEEoIIBLASCASgwggEkAgEBBgor
# BgEEAYRZCgMBMDEwDQYJYIZIAWUDBAIBBQAEIOcufprUOuqjsm6LCOqfNiawvTRd
# XbZPNcXm6Bla+Yl/AgZWq3hzvtwYEzIwMTYwMjEyMjEyMjUzLjIzM1owBwIBAYAC
# AfSggbmkgbYwgbMxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAw
# DgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24x
# DTALBgNVBAsTBE1PUFIxJzAlBgNVBAsTHm5DaXBoZXIgRFNFIEVTTjo1ODQ3LUY3
# NjEtNEY3MDElMCMGA1UEAxMcTWljcm9zb2Z0IFRpbWUtU3RhbXAgU2VydmljZaCC
# DtAwggZxMIIEWaADAgECAgphCYEqAAAAAAACMA0GCSqGSIb3DQEBCwUAMIGIMQsw
# CQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9u
# ZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMTIwMAYDVQQDEylNaWNy
# b3NvZnQgUm9vdCBDZXJ0aWZpY2F0ZSBBdXRob3JpdHkgMjAxMDAeFw0xMDA3MDEy
# MTM2NTVaFw0yNTA3MDEyMTQ2NTVaMHwxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpX
# YXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQg
# Q29ycG9yYXRpb24xJjAkBgNVBAMTHU1pY3Jvc29mdCBUaW1lLVN0YW1wIFBDQSAy
# MDEwMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAqR0NvHcRijog7PwT
# l/X6f2mUa3RUENWlCgCChfvtfGhLLF/Fw+Vhwna3PmYrW/AVUycEMR9BGxqVHc4J
# E458YTBZsTBED/FgiIRUQwzXTbg4CLNC3ZOs1nMwVyaCo0UN0Or1R4HNvyRgMlhg
# RvJYR4YyhB50YWeRX4FUsc+TTJLBxKZd0WETbijGGvmGgLvfYfxGwScdJGcSchoh
# iq9LZIlQYrFd/XcfPfBXday9ikJNQFHRD5wGPmd/9WbAA5ZEfu/QS/1u5ZrKsajy
# eioKMfDaTgaRtogINeh4HLDpmc085y9Euqf03GS9pAHBIAmTeM38vMDJRF1eFpwB
# BU8iTQIDAQABo4IB5jCCAeIwEAYJKwYBBAGCNxUBBAMCAQAwHQYDVR0OBBYEFNVj
# OlyKMZDzQ3t8RhvFM2hahW1VMBkGCSsGAQQBgjcUAgQMHgoAUwB1AGIAQwBBMAsG
# A1UdDwQEAwIBhjAPBgNVHRMBAf8EBTADAQH/MB8GA1UdIwQYMBaAFNX2VsuP6KJc
# YmjRPZSQW9fOmhjEMFYGA1UdHwRPME0wS6BJoEeGRWh0dHA6Ly9jcmwubWljcm9z
# b2Z0LmNvbS9wa2kvY3JsL3Byb2R1Y3RzL01pY1Jvb0NlckF1dF8yMDEwLTA2LTIz
# LmNybDBaBggrBgEFBQcBAQROMEwwSgYIKwYBBQUHMAKGPmh0dHA6Ly93d3cubWlj
# cm9zb2Z0LmNvbS9wa2kvY2VydHMvTWljUm9vQ2VyQXV0XzIwMTAtMDYtMjMuY3J0
# MIGgBgNVHSABAf8EgZUwgZIwgY8GCSsGAQQBgjcuAzCBgTA9BggrBgEFBQcCARYx
# aHR0cDovL3d3dy5taWNyb3NvZnQuY29tL1BLSS9kb2NzL0NQUy9kZWZhdWx0Lmh0
# bTBABggrBgEFBQcCAjA0HjIgHQBMAGUAZwBhAGwAXwBQAG8AbABpAGMAeQBfAFMA
# dABhAHQAZQBtAGUAbgB0AC4gHTANBgkqhkiG9w0BAQsFAAOCAgEAB+aIUQ3ixuCY
# P4FxAz2do6Ehb7Prpsz1Mb7PBeKp/vpXbRkws8LFZslq3/Xn8Hi9x6ieJeP5vO1r
# VFcIK1GCRBL7uVOMzPRgEop2zEBAQZvcXBf/XPleFzWYJFZLdO9CEMivv3/Gf/I3
# fVo/HPKZeUqRUgCvOA8X9S95gWXZqbVr5MfO9sp6AG9LMEQkIjzP7QOllo9ZKby2
# /QThcJ8ySif9Va8v/rbljjO7Yl+a21dA6fHOmWaQjP9qYn/dxUoLkSbiOewZSnFj
# nXshbcOco6I8+n99lmqQeKZt0uGc+R38ONiU9MalCpaGpL2eGq4EQoO4tYCbIjgg
# tSXlZOz39L9+Y1klD3ouOVd2onGqBooPiRa6YacRy5rYDkeagMXQzafQ732D8OE7
# cQnfXXSYIghh2rBQHm+98eEA3+cxB6STOvdlR3jo+KhIq/fecn5ha293qYHLpwms
# ObvsxsvYgrRyzR30uIUBHoD7G4kqVDmyW9rIDVWZeodzOwjmmC3qjeAzLhIp9cAv
# VCch98isTtoouLGp25ayp0Kiyc8ZQU3ghvkqmqMRZjDTu3QyS99je/WZii8bxyGv
# WbWu3EQ8l1Bx16HSxVXjad5XwdHeMMD9zOZN+w2/XU/pnR4ZOC+8z1gFLu8NoFA1
# 2u8JJxzVs341Hgi62jbb01+P3nSISRIwggTaMIIDwqADAgECAhMzAAAAb8b+6gY2
# NAEWAAAAAABvMA0GCSqGSIb3DQEBCwUAMHwxCzAJBgNVBAYTAlVTMRMwEQYDVQQI
# EwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3Nv
# ZnQgQ29ycG9yYXRpb24xJjAkBgNVBAMTHU1pY3Jvc29mdCBUaW1lLVN0YW1wIFBD
# QSAyMDEwMB4XDTE1MTAwNzE4MTczNloXDTE3MDEwNzE4MTczNlowgbMxCzAJBgNV
# BAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4w
# HAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xDTALBgNVBAsTBE1PUFIxJzAl
# BgNVBAsTHm5DaXBoZXIgRFNFIEVTTjo1ODQ3LUY3NjEtNEY3MDElMCMGA1UEAxMc
# TWljcm9zb2Z0IFRpbWUtU3RhbXAgU2VydmljZTCCASIwDQYJKoZIhvcNAQEBBQAD
# ggEPADCCAQoCggEBAMySiXpJx9PibJI0OkeysXTe8SOdQ7lFRCC4ntDL9YLfW3E5
# 3F2Ubk6vtSP2cncBFddNRIeZU0Hv++qsX5U82nohO0TAerg8KJzqBjDMLB4Rb8Gq
# OUiqs1HbNrglu+KaPx54YVIbbtpFmSn6hTe6FfkYZkTlPmWZKmM5YFuf0p4sfm+Z
# ES0R1Tb5kc6fmXjQdrWuHUzDbex6XUcOsc8PlWpLYMl6uq4WO+MzzUKByCCF3cBo
# rynMApp4XexV0jJmwKYPm2z3VfYUTLHnfZ+Bmq/sgcv5RUSCFjOuCm5hBBMKotkU
# lqd2u7u4gLvROYmC2i7u2PGAhOLDfBI2SdoAJkUCAwEAAaOCARswggEXMB0GA1Ud
# DgQWBBR5M1ZpcaAlg/9J8BodIjcAk+JCvzAfBgNVHSMEGDAWgBTVYzpcijGQ80N7
# fEYbxTNoWoVtVTBWBgNVHR8ETzBNMEugSaBHhkVodHRwOi8vY3JsLm1pY3Jvc29m
# dC5jb20vcGtpL2NybC9wcm9kdWN0cy9NaWNUaW1TdGFQQ0FfMjAxMC0wNy0wMS5j
# cmwwWgYIKwYBBQUHAQEETjBMMEoGCCsGAQUFBzAChj5odHRwOi8vd3d3Lm1pY3Jv
# c29mdC5jb20vcGtpL2NlcnRzL01pY1RpbVN0YVBDQV8yMDEwLTA3LTAxLmNydDAM
# BgNVHRMBAf8EAjAAMBMGA1UdJQQMMAoGCCsGAQUFBwMIMA0GCSqGSIb3DQEBCwUA
# A4IBAQClvTGMDc7S2JnIN7MhylBdLVbACIUQQu/6kuilQtZZwbcgYMmxjISCwR0u
# Y9xFQlN+CzGF8lMI5ALBXt0Yz4A5cqPTJ7ziHB3BemUBvnV/FHHEJm/TIkYrCekN
# Q02JtY9KfL8cEfBBMT48pfdDmuXpgnvANN8bIxQ0Din+WGfRXZ6XBgoYNYUNnFcY
# kXY9aQnvGRPOXnKkoaiZHH2FxDqy+4zHc+zJ6F4xiw0JucrPXbzEVUBQOIVG0wBV
# j+dUjJdLGRiDftqajAK4GNOmJ4Io+hhGrQt9fShXfRUQdvmRNzzsk1q1cB050gFk
# Fs3bxLHPZYzyxQyON8G9MVSy734foYIDeTCCAmECAQEwgeOhgbmkgbYwgbMxCzAJ
# BgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25k
# MR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xDTALBgNVBAsTBE1PUFIx
# JzAlBgNVBAsTHm5DaXBoZXIgRFNFIEVTTjo1ODQ3LUY3NjEtNEY3MDElMCMGA1UE
# AxMcTWljcm9zb2Z0IFRpbWUtU3RhbXAgU2VydmljZaIlCgEBMAkGBSsOAwIaBQAD
# FQBws4I3WvjEodJ3am5BFXEBsfqHx6CBwjCBv6SBvDCBuTELMAkGA1UEBhMCVVMx
# EzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoT
# FU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjENMAsGA1UECxMETU9QUjEnMCUGA1UECxMe
# bkNpcGhlciBOVFMgRVNOOjRERTktMEM1RS0zRTA5MSswKQYDVQQDEyJNaWNyb3Nv
# ZnQgVGltZSBTb3VyY2UgTWFzdGVyIENsb2NrMA0GCSqGSIb3DQEBBQUAAgUA2miN
# TDAiGA8yMDE2MDIxMjE2NTY0NFoYDzIwMTYwMjEzMTY1NjQ0WjB3MD0GCisGAQQB
# hFkKBAExLzAtMAoCBQDaaI1MAgEAMAoCAQACAh90AgH/MAcCAQACAhuYMAoCBQDa
# ad7MAgEAMDYGCisGAQQBhFkKBAIxKDAmMAwGCisGAQQBhFkKAwGgCjAIAgEAAgMW
# 42ChCjAIAgEAAgMHoSAwDQYJKoZIhvcNAQEFBQADggEBABlDOt7YtHVPTgWBMzq3
# B+8f+vpes19NWqt0HaL3gepDD0g4+TWWiSYEoKA77eEbJ6dTTlA0f76mura/0rD8
# 4JIWE9OpV4GODArSq9dpSWJkuWf+qYZErOQLcFyO/3mjYdFfe7b96tG/1ROioL+Q
# tnnQnwOLL9zVWvfy5CXfoIECE8ULZTG9cwc8Uqy7bMB5d7jf96o2xQeDx6kUAJy9
# PtCwijKLYFADfKFNlxEXkplkWY9kRjvQ5TFUO5Htq7oLgeTvi5eb4N41zgTiW/CE
# 7Z82eqicpDdSBvaL5IvdTUvX4LVNM53lr0ZzwdtIfLgiR28Gda5x1MPuRZZJeWS0
# LHsxggL1MIIC8QIBATCBkzB8MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGlu
# Z3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBv
# cmF0aW9uMSYwJAYDVQQDEx1NaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0EgMjAxMAIT
# MwAAAG/G/uoGNjQBFgAAAAAAbzANBglghkgBZQMEAgEFAKCCATIwGgYJKoZIhvcN
# AQkDMQ0GCyqGSIb3DQEJEAEEMC8GCSqGSIb3DQEJBDEiBCACMhw1nGE8OvZfurVW
# Tda/LN9k8c9UwRxcgWB/n+NPrTCB4gYLKoZIhvcNAQkQAgwxgdIwgc8wgcwwgbEE
# FHCzgjda+MSh0ndqbkEVcQGx+ofHMIGYMIGApH4wfDELMAkGA1UEBhMCVVMxEzAR
# BgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1p
# Y3Jvc29mdCBDb3Jwb3JhdGlvbjEmMCQGA1UEAxMdTWljcm9zb2Z0IFRpbWUtU3Rh
# bXAgUENBIDIwMTACEzMAAABvxv7qBjY0ARYAAAAAAG8wFgQU1Twvk6u1taKLkwG4
# Bkqbp2J4k9kwDQYJKoZIhvcNAQELBQAEggEAR7BTTQo+IVtYyBzLjtAQ4pHB1TcW
# 5FVJ/X8a1rHWE2IOzgVyoXhAery9xu6K3S3+pWJW1JHGs0LvvF17YB4gReHNWj6t
# U2yMnoPgkWxvlfaPJKye0JycDQpEXQj3qZ/6rPUFJEOwFqklgCT9zGJpDosROzfO
# eyOjP9AQHNI4YvyIx0OHmXuMik8WtD03bbzYePhR8VtZeUBBk7d0KsiVr4LCmsze
# 8O6RVzuf+T502OBKVa0CnEA8zkYVEheyt1wLRt/B0m8aO7Bml6attGagQqOdX336
# VLHludMDgQT1asIkLcDJTj+AkVLqap8EFEEd9sGT5onE0wvnzwuhuDrkMg==
# SIG # End signature block
