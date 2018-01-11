using System;
using System.Collections.Generic;

namespace AeroDataGenerator.Utils
{
    class WorkerContext
    {
        /// <summary>
        /// Storage account to be used for the demo process
        /// </summary>
        public String StorageConnectionString { get; set; }
        /// <summary>
        /// Container that will hold trigger files and the events to upload to Event Hub
        /// </summary>
        public String StorageContainer { get; set; }
        /// <summary>
        /// Name of the file that contains the events to upload to Event Hub
        /// </summary>
        public String EventHubEventFile { get; set; }
        /// <summary>
        /// Name of the event hub to upload events to.
        /// </summary>
        public String EventHubName { get; set; }
        /// <summary>
        /// The connection string for the event hub/service bus
        /// </summary>
        public String EventHubConnectionString { get; set; }

        /// <summary>
        /// Quick validation check to see that we have all of the settings
        /// </summary>
        /// <returns>True if settings are valid, false otherwise.</returns>
        public bool Validate()
        {
            return !String.IsNullOrEmpty(this.StorageConnectionString) &&
                   !String.IsNullOrEmpty(this.EventHubName) &&
                   !String.IsNullOrEmpty(this.EventHubConnectionString) &&                   
                   !String.IsNullOrEmpty(this.StorageContainer) &&
                   !String.IsNullOrEmpty(this.EventHubEventFile);
        }

    }
}
