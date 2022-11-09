# Build a streaming application with mock data

## Table of Content

* [Overview](#overview)
* [Prerequisites](#prerequisites)
* [Application Examples](#stream-examples)
    * [Clickstream-Filter](#clickstream-filter)
    * [Clickstream-RefJoin](#clickstream-refjoin)

## Overview

This project contains application examples that show you how to implement stream applications in Azure. You learn to deploy Azure resources using PowerShell and explore different stream processing scenarios with mock data. This is only for proof-of-concept (POC) or testing, not suitable for production scenarios.

For more information stream analytics, visit [Azure documentation](https://learn.microsoft.com/en-us/azure/stream-analytics/stream-analytics-introduction).

## Prerequisites

* Azure subscription. If you do not have one, create a [free account](https://azure.microsoft.com/en-us/free/)

## Application Examples

This repository has several application examples you can use to build your streaming application. Each example uses different Azure setups with mock data generated.

| **Application name** | **Goal** | **Concepts used** |
| ------------ | --------------------------------------------- | -------------------------------- |
| Clickstream-Filter | extract GET and POST requests from clickstream | one stream input, filter operators            |
| Clickstream-RefJoin | join userid with username in storage | join two inputs (clickstream and reference DB) |

### Clickstream-Filter

In this example, you learn to extract `GET` and `POST` requests from a website clickstream and store the output results to an Azure Blob Storage. Here's the architecture for this example:
![Clickstream one input](./img/clickstream-one-input.png)

Sample of clickstream data:

```json
{
    "EventTime": "2022-09-09 08:58:59 UTC",
    "UserID": 465,
    "IP": "145.140.61.170",
    "Request": {
    "Method": "GET",
    "URI": "/index.html",
    "Protocol": "HTTP/1.1"
    },
    "Response": {
    "Code": 200,
    "Bytes": 42682
    },
    "Browser": "Chrome"
}
```

Follow these steps to generate mock data and deploy the required resources in Azure:

1. Clone this GitHub repository to your local machine.

2. Open PowerShell from the Start menu, go into the folder with command `cd`.

3. Sign in to Azure with the following command and enter your Azure credentials in the pop-up browser.

    ```powershell
    Connect-AzAccount
    ```

4. Deploy Azure sources. Replace `<subscription-id>` with your Azure subscription id and run the following command. This may take a few minutes to complete.

    ```powershell
    .\CreateJob.ps1 -job ClickStream-Filter -subscriptionid <subscription-id> -eventsPerMinute 11
    ```

    * Subscription id can be found in the Azure dashboard.
    * `eventsPerMinute` is the input rate for generated data. In this case, the input source generates 11 events per minute.

5. Once it's done, open your browser and sign in to Azure portal. You see an Azure Resource Group named **ClickStream-Filter-rg-\*** is created. It has five resources:

    | Resource Type | Name | Description |
    | ------------ | --------------------------------------------- | -------------------------------- |
    | Azure Function | clickstream* | Generate clickstream data |
    | Event Hub | clickstream* | Ingest clickstream data for consuming |
    | Stream Analytics Job | ClickStream-Filter | Define a query to extract `GET` requests from the clickstream input |
    | Blob Storage | clickstream* | Output destination for the ASA job |
    | App Service Plan | clickstream* | A necessity for Azure Function |

6. The ASA job **ClickStream-Filter** uses the following query to join the clickstream with reference input. You can select **Test query** in the query editor to preview the output results.

    ```sql
    SELECT System.Timestamp Systime, UserId, Request.Method, Response.Code, Browser
    INTO BlobOutput
    FROM ClickStream TIMESTAMP BY Timestamp
    WHERE Request.Method = 'GET' or Request.Method = 'POST'
    ```

    ![Test Query](./img/test-query.png)

7. All output results are stored as `JSON` file in the Blog Storage. You can view the result via: Blob Storage > Containers > job-output.
![Blob Storage](./img/blog-storage-containers.png)

8. Congratulation! You have deployed your first streaming application to filter clickstream data.

For other stream analytic scenarios with one stream input, here're some examples for the query:

* Count clicks for every hour

    ```sql
    select System.Timestamp as Systime, count( * )
    FROM clickstream
    TIMESTAMP BY EventTime
    GROUP BY TumblingWindow(hour, 1)
    ```

* Select distinct user

    ```sql
    SELECT *
    FROM clickstream
    TIMESTAMP BY Time
    WHERE ISFIRST(hour, 1) OVER(PARTITION BY userId) = 1
    ```

## Clickstream-RefJoin

If you have a user file and want to find out the username for the clickstream, you can join the clickstream with a reference input as following:
![Clickstream two input](./img/clickstream-two-inputs.png)

Assume you have completed the steps for preview example, follow these to create a new resource group: 

1. Run the following command to deploy all resources. This may take a few minutes to complete.

    ```powershell
    .\CreateJob.ps1 -job ClickStream-RefJoin -subscriptionid <subscription-id> -eventsPerMinute 11
    ```

2. Once it is done, sign in to the Azure portal and you can see a resource group named **ClickStream-RefJoin-rg-\***.

3. The ASA job **ClickStream-RefJoin** uses the following query to join the clickstream with reference sql input.

    ```sql
    CREATE TABLE UserInfo(
      UserId bigint,
      UserName nvarchar(max),
      Gender nvarchar(max)
    );
    SELECT System.Timestamp Systime, ClickStream.UserId, ClickStream.Response.Code, UserInfo.UserName, UserInfo.Gender
    INTO BlobOutput
    FROM ClickStream TIMESTAMP BY EventTime
    LEFT JOIN UserInfo ON ClickStream.UserId = UserInfo.UserId
    ```

4. Congratulation! You have deployed a streaming application to joins clickstream with a reference input.

## Delete Azure resources

If you've tried out this project and no longer need the resource group, run this command on PowerShell to delete the resource group.

```powershell
Remove-AzResourceGroup -Name $resourceGroup
```

If you're planning to use this project in the future, you can skip deleting it, and stop the job for now.

## Need Help?

If you have issues with this project or questions about ASA, report an issue [here](https://github.com/Azure/azure-stream-analytics/issues).

For information about ASA, please visit:
* [Quickstart: Create an Azure Stream Analytics job in VS Code](https://learn.microsoft.com/en-us/azure/stream-analytics/quick-create-visual-studio-code)
* [Test ASA queries locally against live stream input](https://learn.microsoft.com/en-us/azure/stream-analytics/visual-studio-code-local-run-live-input)
* [Optimize query using job diagram simulator](https://learn.microsoft.com/en-us/azure/stream-analytics/optimize-query-using-job-diagram-simulator)
