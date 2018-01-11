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
    public partial class CsvData
    {
        public CsvRecord Header;

        public CsvRecord Record;


        public CsvData(List<string> headerList, List<string> valueList)
        {
            Header = new CsvRecord();
            Record = new CsvRecord();
            Header.Fields = headerList;
            Record.Fields = valueList;
        }

        /// <summary>
        /// Check whether has header record
        /// </summary>
        public bool HasHeader
        {
            get { return Header != null; }
        }
    }

    public partial class CsvRecord
    {
        public List<string> Fields = new List<string>();
    }

    public class CsvEncoder
    {
        public static string Encode(CsvData csvData)
        {
            return Encode(csvData, null);
        }

        /// <summary>
        /// Encode CsvData with Format Options
        /// </summary>
        /// <param name="csvData"></param>
        /// <param name="formatOptions">FieldFormatOption
        /// dict that use 0 based field index as key</param>
        public static string Encode(CsvData csvData,
               Dictionary<int, FieldFormatOption> formatOptions)
        {
            return Encode(csvData, formatOptions, ",");
        }

        public static string Encode(CsvData csvData, Dictionary<int,
               FieldFormatOption> formatOptions, string separator)
        {
            CsvEncoder encoder = new CsvEncoder();
            encoder.FormatOptions = formatOptions;
            encoder.Separator = separator;
            return encoder.EncodeCsvData(csvData);
        }

        Dictionary<int, FieldFormatOption> FormatOptions;
        string Separator = ",";
        static readonly char[] sepcialChars = new char[] { ',', '"', '\r', '\n' };

        private string EncodeCsvData(CsvData csvData)
        {
            StringBuilder text = new StringBuilder();
            
            text.AppendLine(EncodeRecord(csvData.Header));
            text.AppendLine("\n");
            text.AppendLine(EncodeRecord(csvData.Record));
            text.AppendLine("\n");
            return text.ToString();
        }

        private string EncodeRecord(CsvRecord record)
        {
            StringBuilder text = new StringBuilder();
            for (int i = 0; i < record.Fields.Count; i++)
            {
                string field = record.Fields[i];

                FieldFormatOption option = FieldFormatOption.Default;
                if (FormatOptions != null && FormatOptions.ContainsKey(i))
                {
                    option = FormatOptions[i];
                }
                int charsToPad = 0;
                if (field != null)
                {
                    string value = option.AlwaysQuoted ? "\"" +
                           EscapeString(field) + "\"" : EncodeField(field);

                    charsToPad = option.TotalWidth - GetTextWidth(value);
                    if (option.AlignRight && charsToPad > 0)
                    {
                        text.Append(new string(' ', charsToPad));
                    }
                    text.Append(value);
                }
                if (i < record.Fields.Count - 1)
                {
                    text.Append(Separator);
                }

                if (!option.AlignRight && charsToPad > 0)
                {
                    text.Append(new string(' ', charsToPad));
                }
            }
            return text.ToString();
        }

        private static string EncodeField(string field)
        {
            if (field.Trim(' ', '\t').Length < field.Length ||
                field.IndexOfAny(sepcialChars) > -1)
            {
                return "\"" + EscapeString(field) + "\"";
            }
            else if (field == string.Empty)
            {
                return "\"\"";
            }
            else
            {
                return field;
            }
        }

        static int GetTextWidth(string text)
        {
            int width = 0;
            foreach (char ch in text)
            {
                if (ch < 0xff)
                {
                    width += 1;
                }
                else
                {
                    width += 2;
                }
            }
            return width;
        }

        static string EscapeString(string text)
        {
            if (text == null) return null;
            StringBuilder escapedtext = new StringBuilder();
            foreach (char ch in text)
            {
                switch (ch)
                {
                    case '\\':
                        escapedtext.Append("\\\\");
                        break;
                    case '\"':
                        escapedtext.Append("\\\"");
                        break;
                    case '\r':
                        escapedtext.Append("\\r");
                        break;
                    case '\n':
                        escapedtext.Append("\\n");
                        break;
                    case '\t':
                        escapedtext.Append("\\t");
                        break;
                    default:
                        escapedtext.Append(ch);
                        break;
                }
            }
            return escapedtext.ToString();
        }
    }

    public class FieldFormatOption
    {
        public int TotalWidth;
        public bool AlignRight;
        public bool AlwaysQuoted;

        public FieldFormatOption() { }

        public FieldFormatOption(int totalWidth,
               bool alignRight, bool alwaysQuoted)
        {
            TotalWidth = totalWidth;
            AlignRight = alignRight;
            AlwaysQuoted = alwaysQuoted;
        }

        public static readonly FieldFormatOption Default =
                               new FieldFormatOption();
    }
}
