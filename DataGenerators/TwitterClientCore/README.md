This program listens to twitter stream and sends the tweet responses to eventhub.<br/>
Both twitter credentials and eventhub credentials are in App.config file.<br/>

Twitter stream api is documented [here](https://developer.twitter.com/en/docs/tweets/filter-realtime/api-reference/post-statuses-filter.html) and details about creating api keys is [here](https://developer.twitter.com/en/docs/basics/apps/overview)<br/>
EventHub apis used are documented [here](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-dotnet-standard-getstarted-send).<br/>

Events are sent with GZip compression. Stream Analytics job input should be configured to use compression. Additional details [here](https://docs.microsoft.com/en-us/azure/stream-analytics/stream-analytics-define-inputs).