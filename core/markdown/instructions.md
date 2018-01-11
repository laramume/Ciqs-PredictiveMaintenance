## Manual Operation Instructions
1. Go [here]({Outputs.saJobOutputsUrl}) and click on the Outputs box under Job topology section and then for all the tables listed in the redirected page click on __Renew Authorization__ to set up authorization for Stream Analytics Power BI Output. To start/stop your Stream Analytics Job, you may use the START / STOP commands [here]({Outputs.saJobOutputsUrl}) 
3. You can go to [Power BI Dashboard](https://powerbi.microsoft.com/) and use a Real-time dataset to build reports and dashboards using your data!
4. You can download the PowerBI template for creating cold path dashboard [here]({Outputs.pbiTemplate}). Instructions on creating cold and hot path Power BI dashboards are available [here](https://github.com/Azure/cortana-intelligence-predictive-maintenance-aerospace).
> **Note**: If you are downloading using an Edge Browser the .pbix file extension might change to .zip and you will need to modify the extension to .pbix to open the file in Power BI Desktop

## Inspect and Monitor the Resources
1. [Web Jobs](https://portal.azure.com/#resource/subscriptions/{SubscriptionId}/resourceGroups/{ResourceGroup.Name}/providers/Microsoft.Web/sites/{Outputs.functionAppName}/webJobs).
		
2. [Azure Data Factory]({Outputs.dataFactoryUrl}).

3. [Azure SQL Database]({Outputs.sqlServerUrl})

4. ML WebService
	* [You can view your machine learning web service API manual]({Outputs.webServiceHelpUrl}).
	* [You can view the experiment by navigating to your]({Outputs.experimentUrl}).
	
## Resources
1. Read [Technical Guide](https://github.com/Azure/cortana-intelligence-predictive-maintenance-aerospace) on more information for the solution.