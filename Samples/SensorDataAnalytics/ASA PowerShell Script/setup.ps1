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

    #Remove this line when we add more scenarios    
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
    

    [string]$global:location = 'CentralUS'
    [string]$global:locationMultiWord = 'Central US'

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
    

    $global:configPath = ('.\temp\setup\' + $global:useCaseName + '.txt')

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
    #login
    $account = Get-AzureAccount
	Write-Host You are signed-in with $account.id
	
	If ($account.id -eq $null)
	{
	    Add-AzureAccount -WarningAction SilentlyContinue | out-null
	}
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
        <#
		if ($global:useCaseName -eq 'sensoralerts')
		{
			Write-Host 'Select a subscription that has Azure Stream Analytics (ASA) enabled. If you do not have access to the ASA preview email nrtpmteam@microsoft.com for assistance.'
		}
        #>
        $subList | Format-Table RowNumber,SubscriptionId,SubscriptionName -AutoSize
        $rowNum = Read-Host 'Enter the row number (1 -'$subCount') of a subscription'

        while( ([int]$rowNum -lt 1) -or ([int]$rowNum -gt [int]$subCount)){
            Write-Host 'Invalid subscription row number. Please enter a row number from the list above'
            $rowNum = Read-Host 'Enter subscription row number'                     
        }
        $global:subscriptionID = $subList[$rowNum-1].SubscriptionId;
        $global:subscriptionDefaultAccount = $subList[$rowNum-1].DefaultAccount.Split('@')[0]
    }



    #switch to appropriate subscription
    try{
        Select-AzureSubscription -SubscriptionId $global:subscriptionID

        if($global:mode -eq 'deploy'){        
            #Register subscription for Azure Stream Analytics
            RegisterAzureProvider('Microsoft.StreamAnalytics')
        } 

     } catch {
        throw 'Subscription ID provided is invalid: ' + $global:subscriptionID 
    
        }
    

}

function RegisterAzureProvider($providerNamespace){

    try{
        Switch-AzureMode -Name AzureResourceManager
        Register-AzureProvider -Force -ProviderNamespace $providerNamespace 
        Write-Host 'Subscription ' $global:subscriptionID 'successfully registered to ' $providerNamespace
        
    } catch {
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
    } catch {
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
        if($ans.ToLower() -eq 'y')
        {
            Remove-AzureResourceGroup -Name $global:resourceGroupName -ErrorAction Stop -Force
        }
    }catch [ArgumentException]{
        # resource group does not exist
    } catch {
        Write-Host 'error.'
    }
    Write-Host 'deleted.'
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
                $sqlsvrname = $sqlsvr.ServerName
                Set-Content -Path $global:configPath -Value $sqlsvrname
                $createdNew = $TRUE;
                
                Write-Host '[svr name: ' $sqlsvrname ']....created.' 
                $global:sqlserverName = $sqlsvrname
            }
            else{
                Write-Host '[svr name: ' $sqlsvrname ']......already exists.'
            }
        } catch{        
            Write-Host 'error.'
            throw
        }

        if($createdNew){
            $rule = New-AzureSqlDatabaseServerFirewallRule -ServerName $sqlsvr.ServerName -RuleName “demorule” -StartIPAddress “0.0.0.0” -EndIPAddress “255.255.255.255” -ErrorAction SilentlyContinue

                
            try{
                Write-Host 'Creating SQL DB [' $global:sqlDBName ']......' -NoNewline 
                $servercredential = new-object System.Management.Automation.PSCredential($sqlServerLogin, ($sqlServerPassword  | ConvertTo-SecureString -asPlainText -Force))
                #create a connection context
                $ctx = New-AzureSqlDatabaseServerContext -ServerName $sqlsvrname -Credential $serverCredential
                $sqldb = New-AzureSqlDatabase $ctx –DatabaseName $global:sqlDBName -Edition Basic 
                Write-Host 'created.'
            } catch {
                Write-Host 'error.'
                throw
            }
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
            if($ans.ToLower() -eq 'y')
            { 
                Remove-AzureSqlDatabaseServer -ServerName $sqlsvrname -Force
                Set-Content -Path $global:configPath -Value ''
                Write-Host 'deleted.' 
            }
        } catch [InvalidOperationException] {
            #thrown when svr doesnt exist
            Write-Host 'doesnt exist.'
        } catch {
            Write-Host 'error.'
        }
    }    
}

function CreateASAJob{
    Write-Host "Preparing ASA job config file......" -NoNewline
	copy -Path "src\\$global:useCaseName\\ASAJob\\ASAJob.json" -Destination temp\json\asa\ -Force
    $files = Get-ChildItem "temp\json\asa\*" -Include ASAJob.json -Recurse -ErrorAction Stop
	
    foreach($file in $files)
    {
        Update-ASAJSONFile  $file.FullName      
    }
	Write-Host 'prepared.'
	
	Write-Host "Creating the ASA Job......" -NoNewline
	try{
            Switch-AzureMode AzureResourceManager
            $ASAJob = New-AzureStreamAnalyticsJob -File "temp\json\asa\ASAjob.json" -Name $global:defaultResourceName -ResourceGroupName $global:resourceGroupName -ErrorAction Stop
			if ($ASAJob.JobName -eq $global:defaultResourceName)
			{
            Write-Host 'created.'
			}
        }catch {
            Write-Host 'error.'
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
                if($ans.ToLower() -eq 'y')
                {
                    Remove-AzureStreamAnalyticsJob -Name $global:defaultResourceName -ResourceGroupName $global:resourceGroupName -Force -ErrorAction Stop
                    $done = $true
                    Write-Host 'deleted.'     
                }           
            } catch {
                if($error[0].Exception.ToString().contains('ResourceGroupNotFound')){
                    Write-Host 'doesnt exist.'
                    $retryCount = 0
                    $done = $true
                } else{
                    Write-Host 'error. retrying delete....'
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
			if($namespace.name -eq $null)
			{
			Write-Host 'Creating the Service Bus Namespace ['$global:ServiceBusNamespace ']......' -NoNewline
            #create a new Service Bus Namespace
            $namespace = New-AzureSBNamespace -Name $global:ServiceBusNamespace -Location $global:locationMultiWord -CreateACSNamespace $true -NamespaceType Messaging -ErrorAction Stop
			Write-Host 'created.'
			}
			else {
			Write-Host 'The ['$global:ServiceBusNamespace '] namespace in the ['$global:locationMultiWord '] region already exists. Delete and try again'
			}
		 }catch{
                Write-Host 'error.'
                throw
			}
               
        try{		
			#Create the NamespaceManager object to create the event hub
			$currentnamespace = Get-AzureSBNamespace -Name $global:ServiceBusNamespace
			Write-Host 'Creating a NamespaceManager object for the ['$global:ServiceBusNamespace '] namespace...' -NoNewline
            $filepath = $PSScriptRoot + "\src\\$global:useCaseName\\bin\Debug\*"
            Unblock-File -Path $filepath
			$nsMgrType = [System.Reflection.Assembly]::LoadFrom($PSScriptRoot +"\src\\$global:useCaseName\\bin\Debug\Microsoft.ServiceBus.dll").GetType("Microsoft.ServiceBus.NamespaceManager")
			

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
        }catch{
                  Write-Host 'error.'
				  throw
            }
        
		
    }
}

function DeleteventHubandSBNamespace{
    Switch-AzureMode AzureServiceManagement
	$serviceBusDll = $PSScriptRoot + "\src\\$global:useCaseName\\bin\Debug\Microsoft.ServiceBus.dll"
	Add-Type -Path $serviceBusDll
	try
	{
    $currentnamespace = Get-AzureSBNamespace -Name $global:ServiceBusNamespace
	}
	catch
	{
    Write-Host Azure Service Bus Namespace: $global:ServiceBusNamespace not found! 
	}

	if ($currentnamespace)
	{
  
        Write-Host 'Deleting ServiceBus Namespace [' $global:ServiceBusNamespace ']......Continue (Y/N)?' -NoNewline
        $ans=Read-Host
        if($ans.ToLower() -eq 'y')
        {
            try
            {
                Remove-AzureSBNamespace -Name $global:ServiceBusNamespace -Force
                Write-Host 'deleted'.
            }
            catch{
                Write-Host 'error'.
                throw
            }
        }
	 }
	 else
	 {
        Write-Host The namespace $global:ServiceBusNamespace does not exists.  
	 }
}

function writeAccountInformation {
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
   if ($StartASAJob -eq "True")
   {
		Write-Host 'started.'
   }
   }catch {
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
   	DeleteventHubandSBNamespace
    DeleteSQLServerAndDB
    DeleteResourceGroup
	DeleteASAJob
}

#start of main script
$storePreference = $Global:VerbosePreference
ValidateParameters
SetGlobalParams
InitSubscription   

$Global:VerbosePreference = "SilentlyContinue"

switch($global:useCaseName){
    'sensoralerts'{
        switch($global:mode){
        'deploy'{
                CreateSensorAlertsResources
                SetMappingDictionary
                writeAccountInformation
                PopulateSensorAlerts
            }
            'delete'{
                DeleteSensorAlertsResources
            }
        }
    }
}


$Global:VerbosePreference = $storePreference






