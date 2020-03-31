This example project shows a use case for C# user-defined-functions in Azure Stream Analytics. With this capability, you can implement your own C# function using any Nuget and use it in your query. You can then invoke this UDF from your query. This UDF will be executed on every input event your job receives. You can [learn more](https://docs.microsoft.com/azure/stream-analytics/stream-analytics-edge-csharp-udf-methods) about C# UDFs in Stream Analytics.

This sample shows how you can use a pre-built ONNX model as a UDF in your job to do real-time scoring/prediction. The model was trained on NYC taxi data to predict fares for each ride. The model accepts 6 inputs:
1. Vendor id
2. Passenger count
3. Trip time
4. Trip distance
5. Pick up time (month, day of week and hour)

The sample input folder has both streaming input (taxi ride data) and reference data input (region definition of some neighborhoods in Manhattan). This will allow you to easily test your query using C# ONNX Model.

The C# UDF project has the UDF implementation which loads the ONNX model and predicts the fare based on the input parameters sent by the query. This project has the following NuGet references:
1. Microsoft.Azure.StreamAnalytics
2. Microsoft.ML
3. Microsoft.ML.ImageAnalytics
4. Microsoft.ML.OnnxRuntime
5. Microsoft.ML.OnnxTransformer

You can clone this repo from github and open the project using Visual Studio 2017. After this:
1. Build the solution (both Stream Analyics project as well as ASA C# UDF project)
2. Add reference of the C# UDF project to your Stream Analytics project. [Screenshots](https://docs.microsoft.com/azure/stream-analytics/stream-analytics-edge-csharp-udf-methods#example).
3. Ensure the predict.json under 'Functions' in Stream Analytics project is configured to the UDF implementation.
4. Ensure local data points to the right json file paths
5. Hit run locally.