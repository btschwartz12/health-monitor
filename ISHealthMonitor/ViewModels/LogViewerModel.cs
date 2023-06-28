using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ISHealthMonitor.UI.ViewModels
{
    public class LogViewerModel
    {

        public string? Today { get; set; }
        public string? LastWeek { get; set; }
        public List<LogFile> LogFiles { get; set; }

        public List<string> IgnorePhrases = new List<string>()
        {
            "Hosting environment:",
            "Content root path:"
        };

        public Dictionary<string, LogEntryType> EntryTypeMap = new Dictionary<string, LogEntryType>
        {
            { "INF", LogEntryType.INFO },
            { "WRN", LogEntryType.WARNING },
            { "ERR", LogEntryType.ERROR },
            { "DBG", LogEntryType.DEBUG }
        };
    }



    public class LogFile
    {
        public string FileName { get; set; }
        public DateTime Date { get; set; }
        public List<LogEntry> LogEntries { get; set; }
    }


    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Type { get; set; }
        public bool IsSystemLog { get; set; }
        public Guid GUID { get; set; }
        public string Content { get; set; }
    }

    public enum LogEntryType
    {
        INFO,
        WARNING,
        ERROR,
        DEBUG
    }


    
}
