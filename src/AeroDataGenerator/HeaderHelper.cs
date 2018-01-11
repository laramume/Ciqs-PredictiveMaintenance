using System;
using System.Collections.Generic;

namespace AeroDataGenerator.Utils
{
    /// <summary>
    /// Helper class with parsing a CSV header file.
    /// </summary>
    class HeaderHelper
    {
        private List<String> Header { get; set; }

        /// <summary>
        /// Constructor builds up the internal Header list. Headers are stripped of
        /// quotes during the process.
        /// </summary>
        /// <param name="header">Header line of a CSV file</param>
        public HeaderHelper(String[] header)
        {
            Header = new List<string>();
            foreach (String hdr in header)
            {
                this.Header.Add(hdr.Replace('\"',' ').Trim());
            }
        }

        /// <summary>
        /// Given the header text representation, returns the index of the associated column.
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public int GetIndex(String header)
        {
            return Header.IndexOf(header);
        }

        /// <summary>
        /// Recreates the header line sans any additional quotation marks.
        /// </summary>
        /// <returns>Original header without quotations or superfluous spacing</returns>
        public override string ToString()
        {
            return String.Join(",",this.Header);
        }
    }
}
