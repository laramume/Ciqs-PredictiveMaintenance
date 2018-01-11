using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AeroDataGenerator
{
    public class EventContent
    {
        public String Header { get; set; }
        public IList<String> Content { get; set; }

        public EventContent()
        {
            this.Content = new List<String>();
        }
    }

    public class HubEvents
    {

        public static Stream GetStream(String connectionString, String container, String eventFile)
        {
            return BlobUtils.GetBlobStream(connectionString, container, eventFile);
        }

        public static EventContent LoadHubEvents(String connectionString, String container, String eventFile)
        {
            EventContent returnValue = new EventContent();

            String contents = BlobUtils.GetContents(
                                connectionString,
                                container,
                                eventFile);

            if (!String.IsNullOrEmpty(contents))
            {
                // CSV file, parse it into individual items
                string[] demoContent = contents.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);

                returnValue.Header = demoContent[0];

                for (int i = 1; i < demoContent.Length; i++)
                {
                    returnValue.Content.Add(demoContent[i]);
                }
            }

            return returnValue;
        }

    }
}
