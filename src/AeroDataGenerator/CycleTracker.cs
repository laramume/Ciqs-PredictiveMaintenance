using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace AeroDataGenerator.Utils
{
    /// <summary>
    /// PowerBI expects that RUL based on cycle will have a single entry in the database per aircraft/cycle. 
    /// 
    /// Since we are not rolling over and cleaning up the app needs to track the last known cycle for an engine
    /// so it can update the current cycle being uploaded. This will ensure that we do not have duplicate cycles
    /// per engine.
    /// </summary>
    internal class CycleTracker
    {
        #region Constants
        /// <summary>
        /// The file name for the cycle data.
        /// </summary>
        private const String TrackerFile = "AircraftCycleData.json";
        #endregion

        #region Private Members
        /// <summary>
        /// The directory in which the cycle information is stored. This should align with the data file
        /// directory of events. 
        /// </summary>
        private String DataDirectory { get; set; }
        /// <summary>
        /// The tracking file holding the cycle information 
        /// </summary>
        private String ActualTrackerFile { get; set; }

        /// <summary>
        /// The cycles to use for the current execution
        /// </summary>
        private Dictionary<long, long> CurrentCycles { get; set; }

        /// <summary>
        /// A tracking of the last cycles to use.
        /// </summary>
        private Dictionary<long, long> LatestCycles { get; set; }
        #endregion

        public CycleTracker(String directory)
        {
            if (String.IsNullOrEmpty(directory))
            {
                throw new ArgumentNullException("directory", "Tracker directory empty");
            }

            String assemblyLocation = Assembly.GetExecutingAssembly().Location;
            this.DataDirectory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(assemblyLocation), directory);

            if (!Directory.Exists(this.DataDirectory))
            {
                Directory.CreateDirectory(this.DataDirectory);
            }

            this.ActualTrackerFile = System.IO.Path.Combine(this.DataDirectory, TrackerFile);

            this.LoadCycleInformation();
        }

        #region Public Methods
        /// <summary>
        /// Get the current base cycle to use for this cycle through the file
        /// </summary>
        /// <param name="id">Aircraft ID</param>
        /// <returns>Current cycle to use for the aircraft this run through the file</returns>
        public long GetCurrentCycle(long id)
        {
            long returnValue = 0;
            if (this.CurrentCycles.ContainsKey(id))
            {
                returnValue = this.CurrentCycles[id];
            }
            return returnValue;
        }

        /// <summary>
        /// We have to keep track of what actual cycle is being reported. If the app fails or the progress is stopped, we
        /// want to start at the next cycle number so we do not duplicate data in the pipeline which could affect the 
        /// PowerBI dashboard.
        /// </summary>
        /// <param name="id">Aircraft Engine ID</param>
        /// <param name="cycle">Currently reported cycle in file.</param>
        public void ReportCurrentCycle(long id, long cycle)
        {
            // Save out the information only if we are adding or have a new value.
            if (this.LatestCycles.ContainsKey(id) == false)
            {
                this.LatestCycles.Add(id,cycle);
            }
            else 
            {
                this.LatestCycles[id] = cycle;
            }

            this.SaveCycleInformation();
        }

        /// <summary>
        /// When the file recycles we need to ensure that we update the current cycles dictionary 
        /// to avoid duplications.
        /// </summary>
        public void FileRecycle()
        {
            this.CurrentCycles = new Dictionary<long, long>(this.LatestCycles);
        }

        /// <summary>
        /// Save the latest cycles to the file, current cycles are only used for this run.
        /// </summary>
        public void SaveCycleInformation()
        {
            String configurationContent = JsonConvert.SerializeObject(this.LatestCycles, Formatting.Indented);

            using (StreamWriter writer = new StreamWriter(this.ActualTrackerFile, false))
            {
                writer.WriteLine(configurationContent);
            }
        }
        #endregion

        #region Private Helpers
        /// <summary>
        /// Load the existing cycle track information, if present, otherwise use a default empty dictionary.
        /// </summary>
        private void LoadCycleInformation()
        {
            if (System.IO.File.Exists(this.ActualTrackerFile))
            {
                String configurationContent = String.Empty;

                using (StreamReader reader = new StreamReader(this.ActualTrackerFile))
                {
                    configurationContent = reader.ReadToEnd();
                }

                if (!String.IsNullOrEmpty(configurationContent))
                {
                    this.CurrentCycles = JsonConvert.DeserializeObject<Dictionary<long, long>>(configurationContent);
                    this.LatestCycles = new Dictionary<long, long>(this.CurrentCycles);
                }
            }
            else
            {
                this.CurrentCycles = new Dictionary<long, long>();
                this.LatestCycles = new Dictionary<long, long>();
            }
        }
        #endregion
    }
}
