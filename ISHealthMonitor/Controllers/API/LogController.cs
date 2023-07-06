using ISHealthMonitor.UI.ViewModels;
using ISHealthMonitor.Core.Helpers.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace ISHealthMonitor.UI.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController
    {
        private readonly ILogger<LogController> _logger;
        private readonly LogCache _cache;

        public LogController(ILogger<LogController> logger, LogCache cache)
        {
            _logger = logger;
            _cache = cache;
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

            var logPath = Path.Combine(Environment.CurrentDirectory, "Logs");

            var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            Directory.CreateDirectory(tempPath);


            var files = Directory.EnumerateFiles(logPath, "*.txt");

            foreach (var file in files)
            {
                var tempFilePath = Path.Combine(tempPath, Path.GetFileName(file));

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

                string key = $"LogFile_{logFile.FileName}";

                bool isToday = logFile.Date == DateTime.Now.Date;

                // Try to get the data from cache
                if (!isToday && _cache.Cache.TryGetValue(key, out LogFile cachedLogFile))
                {
                    model.LogFiles.Add(cachedLogFile);
                    continue;
                }

                // If not in cache, parse the file
                try
                {
                    System.IO.File.Copy(file, tempFilePath, true);
                }
                catch (IOException ex)
                {
                    throw new Exception("Failed to copy log file: " + ex.Message);
                }

                logFile = ParseLogFile(logFile, model, tempPath, tempFilePath);

                // Set the data in the cache, with some expiration policy
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Set cache entry size by extension method.
                    .SetSize(1);

                _cache.Cache.Set(key, logFile, cacheEntryOptions);

                model.LogFiles.Add(logFile);
            }


            Directory.Delete(tempPath, true);

            return model;

        }


        private LogFile ParseLogFile(LogFile logFile, LogViewerModel model, string tempDir, string tempFilePath)
        {
            var lines = System.IO.File.ReadLines(tempFilePath);
            LogEntry currentLogEntry = null;
            bool isInBlock = false;

            foreach (var line in lines)
            {

                if (isInBlock)
                {

                    try
                    {
                        // Try to parse the log entry, if successful we are not in the exception block anymore
                        var logEntry = ParseLogEntry(model, line);
                        isInBlock = false;
                        logFile.LogEntries.Add(currentLogEntry);
                        currentLogEntry = null;


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
                    LogEntry logEntry = ParseLogEntry(model, line);
                    if (logEntry != null)
                    {
                        // Instead of finding every case of where a log entry should have multiple 
                        // lines and set isInBlock=true, we can just treat every entry as a block
                        // and it will still get handled fine if it is one line

                        // This means the code should be refactored, but.....it's fine

						isInBlock = true;
						currentLogEntry = logEntry;

                    }

                }
                catch (Exception ex)
                {
                    Directory.Delete(tempDir, true);
                    throw new Exception("Failed to parse log file: " + ex.Message);
                }
            }

            if (currentLogEntry != null)
            {
				logFile.LogEntries.Add(currentLogEntry);
			}

            return logFile;
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
