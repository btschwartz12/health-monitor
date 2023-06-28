using ISHealthMonitor.UI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;

namespace ISHealthMonitor.UI.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController
    {
        private readonly ILogger<LogController> _logger;

        public LogController(ILogger<LogController> logger)
        {
            _logger = logger;
        }

        [HttpGet("GetAllLogs")]
        public async Task<string> GetAllLogs()
        {
            var start = DateTime.MinValue;
            var end = DateTime.MaxValue;

            LogViewerModel model = await GetViewerModel(start, end);

            return JsonConvert.SerializeObject(model);
        }


        [HttpGet("GetLogsInRange")]
        public async Task<string> GetLogRange(string startInclusive, string endInclusive)
        {
            var start = DateTime.Parse(startInclusive);
            var end = DateTime.Parse(endInclusive);

            LogViewerModel model = await GetViewerModel(start, end);

            return JsonConvert.SerializeObject(model);
        }

        private async Task<LogViewerModel> GetViewerModel(DateTime startInclusive, DateTime endInclusive)
        {
            var model = new LogViewerModel()
            {
                LogFiles = new List<LogFile>()
            };

            var logPath = Path.Combine(Environment.CurrentDirectory, "wwwroot\\lib\\Logs");

            var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            Directory.CreateDirectory(tempPath);


            var files = Directory.EnumerateFiles(logPath, "*.txt");

            foreach (var file in files)
            {

                var tempFilePath = Path.Combine(tempPath, Path.GetFileName(file));

                try
                {
                    System.IO.File.Copy(file, tempFilePath, true);
                }
                catch (IOException)
                {
                    // If the file is being used by another process, skip it.
                    continue;
                }

                var logFile = new LogFile()
                {
                    FileName = Path.GetFileName(file),
                    Date = DateTime.ParseExact(Path.GetFileNameWithoutExtension(file), "yyyyMMdd", CultureInfo.InvariantCulture),
                    LogEntries = new List<LogEntry>()
                };

                // Skip if it does not fall in date range
                if (logFile.Date < startInclusive || logFile.Date > endInclusive)
                {
                    continue;
                }

                var lines = System.IO.File.ReadLines(tempFilePath);
                LogEntry currentLogEntry = null;
                bool isInExceptionBlock = false;

                foreach (var line in lines)
                {

                    if (isInExceptionBlock)
                    {

                        try
                        {
                            // Try to parse the log entry, if successful we are not in the exception block anymore
                            var logEntry = ParseLogEntry(model, line);
                            isInExceptionBlock = false;
                            logFile.LogEntries.Add(currentLogEntry);


                        }
                        catch (Exception ex)
                        {
                            // If it can't parse, we are still in the block
                            currentLogEntry.Content += "\n" + line;
                            continue;
                        }

                    }

                    try
                    {
                        var logEntry = ParseLogEntry(model, line);
                        if (logEntry != null)
                        {
                            if (logEntry.Type == LogEntryType.ERROR.ToString())
                            {
                                isInExceptionBlock = true;
                                currentLogEntry = logEntry;
                            }
                            else
                            {
                                logFile.LogEntries.Add(logEntry);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Directory.Delete(tempPath, true);
                        throw new Exception("Failed to parse log file: " + ex.Message);
                    }
                }
                model.LogFiles.Add(logFile);
            }

            Directory.Delete(tempPath, true);

            return model;

        }


        private LogEntry ParseLogEntry(LogViewerModel model, string line)
        {
            if (string.IsNullOrWhiteSpace(line) || model.IgnorePhrases.Any(phrase => line.Contains(phrase)))
            {
                return null;
            }

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var timestamp = DateTime.Parse(parts[0]);


            bool isSystemLog = false;
            string typeStr = "";
            int contentStartIndex = -1;
            Guid guid = Guid.Empty;

            int guidPartIndex = Array.FindIndex(parts, x => Guid.TryParse(x, out guid));


            // No guid found, is system log
            if (guidPartIndex == -1)
            {
                typeStr = parts[1].TrimStart('[').TrimEnd(']').ToUpper();
                isSystemLog = true;
                contentStartIndex = 2;
            }
            else
            {
                typeStr = parts[2].TrimStart('[').TrimEnd(']').ToUpper();
                isSystemLog = false;
                contentStartIndex = 3;
            }


            if (model.EntryTypeMap.TryGetValue(typeStr, out var type))
            {
                var logEntry = new LogEntry()
                {
                    Timestamp = timestamp,
                    Type = type.ToString(),
                    IsSystemLog = isSystemLog,
                    GUID = guid,
                    Content = string.Join(' ', parts.Skip(contentStartIndex))

                };
                return logEntry;
            }
            else
            {
                throw new Exception("Invalid log type: " + typeStr);
            }

        }


    }
}
