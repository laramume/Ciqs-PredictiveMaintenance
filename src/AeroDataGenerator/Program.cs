//********************************************************* 
// 
//    Copyright (c) Microsoft. All rights reserved. 
//    This code is licensed under the Microsoft Public License. 
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF 
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY 
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR 
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT. 
// 
//*********************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using AeroDataGenerator.Utils;

namespace AeroDataGenerator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string storageConnectionString = string.Concat("DefaultEndpointsProtocol=https;AccountName=",
                ConfigurationManager.AppSettings["StorageAccountName"],
                ";AccountKey=",
                ConfigurationManager.AppSettings["StorageAccountKey"]
                );

            WorkerContext context = new WorkerContext()
            {
                StorageConnectionString = storageConnectionString,
                StorageContainer = ConfigurationManager.AppSettings["DataStorageContainerName"],
                EventHubEventFile = ConfigurationManager.AppSettings["SourceDataFile"],
                EventHubName = ConfigurationManager.AppSettings["EventHubName"],
                EventHubConnectionString = ConfigurationManager.AppSettings["EventHubConnectionString"]
            };

            if (!context.Validate())
            {
                Console.WriteLine("App settings not ready!");
                return;
            }

            Upload(context);
        }

        private static bool Upload(WorkerContext context)
        {
            Console.WriteLine("Upload Task starting");

            Stream contentStream = HubEvents.GetStream(context.StorageConnectionString,
                context.StorageContainer,
                context.EventHubEventFile);

            if (contentStream == null)
            {
                Console.WriteLine("Cannot access the content to stream to the EventHub");
                return false;
                //this.cancellationTokenSource.Cancel();
            }

            try
            {
                EventHubClient client = EventHubClient.CreateFromConnectionString(context.EventHubConnectionString,
                    context.EventHubName);

                // Get a reader stream on the blob
                using (StreamReader contentReader = new StreamReader(contentStream))
                {
                    String streamContentString;
                    var cycTracker = new CycleTracker("cycles");

                    while (true)
                    {
                        // Clean up the database on each iteration
                        streamContentString = contentReader.ReadLine();
                        if (String.IsNullOrEmpty(streamContentString))
                        {
                            Console.WriteLine("Content stream does not contain a header......");
                            return false;
                        }

                        // Figure out where the counter field is.
                        String[] headerParts = streamContentString.Split(new char[] {','},
                            StringSplitOptions.RemoveEmptyEntries);
                        HeaderHelper hdrHelper = new HeaderHelper(headerParts);
                        int counterIdx = hdrHelper.GetIndex("counter");
                        int cycleIdx = hdrHelper.GetIndex("cycle");
                        int idIdx = hdrHelper.GetIndex("id");

                        // Stuff to track which counter we are on
                        String currentCounter = string.Empty;
                        List<String> currentCounterData = new List<string>();

                        //String currentCycle = string.Empty;

                        while (!contentReader.EndOfStream)
                        {
                            String eventData = contentReader.ReadLine();
                            String[] data = eventData.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                            long id = Convert.ToInt64(data[idIdx]);
                            var baseCyc = cycTracker.GetCurrentCycle(id);

                            var currentCycle = Convert.ToInt64(data[cycleIdx]) + baseCyc;

                            data[cycleIdx] = Convert.ToString(currentCycle);

                            string updatedData = string.Join(",", data);

                            if (
                                String.Compare(currentCounter, data[counterIdx],
                                    StringComparison.CurrentCultureIgnoreCase) != 0)
                            {
                                // Send what we have
                                foreach (String uploadData in currentCounterData)
                                {
                                    String payload = String.Format("\"processed\",{0}{1}{3},{2}",
                                        hdrHelper.ToString(),
                                        Environment.NewLine,
                                        uploadData,
                                        DateTime.Now.ToUniversalTime().ToString("O")
                                        );

                                    client.Send(new EventData(Encoding.UTF8.GetBytes(payload)));

                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine("Upload Task event uploaded for counter" + currentCounter);
                                    Console.WriteLine("Sending [ " + currentCounter + " ]: " 
                                        + payload + " at: " +
                                        DateTime.UtcNow);
                                }

                                // Collect currentCounter and start collection process
                                currentCounter = data[counterIdx];
                                currentCounterData.Clear();
                                currentCounterData.Add(updatedData);

                                // Wait 1 second between counters
                                Task.Delay(1000);
                            }
                            else
                            {
                                currentCounterData.Add(updatedData);
                                //currentCounterData.Add(eventData);
                            }

                            cycTracker.ReportCurrentCycle(id, currentCycle);
                        }

                        if (currentCounterData.Count() > 0)
                        {
                            // Send what we have
                            foreach (String uploadData in currentCounterData)
                            {
                                String payload = String.Format("\"processed\",{0}{1}{3},{2}",
                                    hdrHelper.ToString(),
                                    Environment.NewLine,
                                    uploadData,
                                    DateTime.Now.ToUniversalTime().ToString("O")
                                    );

                                client.Send(new EventData(Encoding.UTF8.GetBytes(payload)));

                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Upload Task event uploaded for counter" + currentCounter);
                                Console.WriteLine("Sending [ " + currentCounter + " ]: "
                                                  + payload + " at: " +
                                                  DateTime.UtcNow);
                            }
                        }
                        cycTracker.FileRecycle();
                        // Reset stream position when we run out of data.
                        contentReader.BaseStream.Position = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception working with Event Hub : " + ex.Message);
            }
            finally
            {
                if (contentStream != null)
                {
                    contentStream.Dispose();
                }
            }

            Console.WriteLine("Upload Task completed");
            return true;
        }
    }
}
