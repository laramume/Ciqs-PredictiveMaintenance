#load "..\CiqsHelpers\All.csx"

using System.Net;
using Microsoft.Azure.Management.StreamAnalytics;
using Microsoft.Azure.Management.StreamAnalytics.Models;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.Rest;

public static async Task<object> Run(HttpRequestMessage req, TraceWriter log)
{
    var parametersReader = await CiqsInputParametersReader.FromHttpRequestMessage(req);
    string subscriptionId = parametersReader.GetParameter<string>("subscriptionId");
    string authorizationToken = parametersReader.GetParameter<string>("accessToken");
    string resourceGroupName = parametersReader.GetParameter<string>("resourceGroupName");
    string saJobNames = parametersReader.GetParameter<string>("saJobNames");

    foreach (var jobName in saJobNames.Split(','))
    {
        try
        {
            StartStreamingJob(subscriptionId, authorizationToken, resourceGroupName, jobName);
        }
        catch (Exception e)
        {
            log.Error("Unable start Stream Analytics Job", e);
            return req.CreateResponse(
                HttpStatusCode.BadRequest, 
                new {
                    Message = $"Unable start Stream Analytics Job {jobName}. \n\n{e.Message}"
                });
        } 
    }
    return null;
}

private static void StartStreamingJob(string subscriptionId, string authorizationToken, string resourceGroupName, string jobName)
{
    var credentials = new TokenCredentials(authorizationToken);
    using (StreamAnalyticsManagementClient streamClient = new StreamAnalyticsManagementClient(credentials))
    {
        streamClient.SubscriptionId = subscriptionId;

        // Start a streaming job
        StartStreamingJobParameters jobStartParameters = new StartStreamingJobParameters()
        {
            OutputStartMode = OutputStartMode.JobStartTime            
        };

        streamClient.StreamingJobs.Start(resourceGroupName, jobName, jobStartParameters);        
    }
}
