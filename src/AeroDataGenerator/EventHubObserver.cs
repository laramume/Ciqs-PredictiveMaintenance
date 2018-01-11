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
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System.Configuration;


namespace AeroDataGenerator
{
    class EventHubObserver : IObserver<Payload>
    {
        private EventHubConfig _config;
        private EventHubClient _eventHubClient;
       
                
        public EventHubObserver(EventHubConfig config)
        {
            try
            {
                _config = config;
                _eventHubClient = EventHubClient.CreateFromConnectionString(_config.ConnectionString);
                
            }
            catch (Exception ex)
            {
               
            }

        }
        public void OnNext(Payload TwitterPayloadData)
        {
            try
            {
                List<string> headerList = new List<string> { "CreatedAt", "Topic", "Text" };
                List<string> valueList = new List<string> { TwitterPayloadData.CreatedAt.ToString(), TwitterPayloadData.Topic, TwitterPayloadData.Text };
                CsvData csvData = new CsvData(headerList, valueList);
                
                string serialisedString = CsvEncoder.Encode(csvData, null, ",");

                EventData data = new EventData(Encoding.UTF8.GetBytes(serialisedString)) { PartitionKey = TwitterPayloadData.Topic };
                _eventHubClient.Send(data);
               
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Sending" + serialisedString + " at: " + TwitterPayloadData.CreatedAt.ToString() );
                                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured: {0}", ex);
            }

        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            
        }

    }
}
