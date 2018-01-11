#load "..\CiqsHelpers\All.csx"

using System;
using System.Configuration;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.Azure.Management.DataFactories;
using Microsoft.Azure.Management.DataFactories.Common.Models;

public static async Task<object> Run(HttpRequestMessage req, TraceWriter log)
{
    var parametersReader = await CiqsInputParametersReader.FromHttpRequestMessage(req);
    string subscriptionId = parametersReader.GetParameter<string>("subscriptionId");
    string authorizationToken = parametersReader.GetParameter<string>("accessToken");
    string resourceGroupName = parametersReader.GetParameter<string>("resourceGroupName");
    string dataFactoryName = parametersReader.GetParameter<string>("dataFactoryName");

    const string ResourceManagerEndpoint = "https://management.azure.com/";

    // If the setting not present, wait for a while until it is ready
    int tries = 50;
    while (string.IsNullOrEmpty(subscriptionId) && tries > 0)
    {
        Thread.Sleep(2000);
        tries--;
        subscriptionId = ConfigurationManager.AppSettings["SubscriptionId"];
    }

    if (string.IsNullOrEmpty(subscriptionId))
    {
        Console.WriteLine("App settings not ready!");
        return null;
    }

    int pipelineDurationInHours = Convert.ToInt32(ConfigurationManager.AppSettings["PipelineDurationInHours"]);

    var dfClient = new DataFactoryManagementClient(
        new TokenCloudCredentials(subscriptionId, authorizationToken),
        new Uri(ResourceManagerEndpoint));
    Console.WriteLine("dfClient: " + dfClient.ToString());

    string pipelineStartTime = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture);
    string pipelineEndTime = DateTime.UtcNow.Add(new TimeSpan(pipelineDurationInHours, 0, 0)).ToString("s", CultureInfo.InvariantCulture);

    Console.WriteLine("Getting the first page of pipelines in the data factory");
    var response = dfClient.Pipelines.List(resourceGroupName, dataFactoryName);
    while (response.Pipelines != null && response.Pipelines.Count > 0)
    {
        foreach (var pipeline in response.Pipelines)
        {
            if (!string.IsNullOrEmpty(pipeline.Name))
            {
                Console.WriteLine("Pipeline name: " + pipeline.Name);
                Console.WriteLine("Setting start and end for the pipeline");
                dfClient.Pipelines.SetActivePeriod(
                    resourceGroupName,
                    dataFactoryName,
                    pipeline.Name,
                    new PipelineSetActivePeriodParameters(pipelineStartTime, pipelineEndTime));

                Console.WriteLine("Resuming the pipeline");
                dfClient.Pipelines.Resume(
                    resourceGroupName,
                    dataFactoryName,
                    pipeline.Name);
            }
            else
            {
                Console.WriteLine("Found pipeline with no name. Ignoring..");
            }
        }

        // get next page of pipelines
        if (!string.IsNullOrEmpty(response.NextLink))
        {
            Console.WriteLine("Getting the next page of pipelines");
            response = dfClient.Pipelines.ListNext(response.NextLink);
        }
        else
        {
            break;
        }
    }

    Console.WriteLine("Done");

    return null;
}
