using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Models;
using ISHealthMonitor.Core.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ISHealthMonitor.Core.Models;
using ISHealthMonitor.UI.ViewModels;
using ISHealthMonitor.Core.Helpers;
using ISHealthMonitor.Core.Common;
using ISHealthMonitor.Core.DataAccess;
using ISHealthMonitor.Core.Implementations;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.IO;

namespace ISHealthMonitor.Controllers
{
    [Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHealthModel _healthModel;
        private readonly IEmployee _employee;
        private readonly IRest _restModel;
        private readonly IConfiguration _config;
        public HomeController(ILogger<HomeController> logger,
            IHealthModel healthModel, IEmployee employee, IRest restModel, IConfiguration config)
        {
            _logger = logger;
            _employee = employee;
            _healthModel = healthModel;
            _restModel = restModel;
            _config = config;
        }

        public async Task<IActionResult> Index()
        {

            var user = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");

			var CurrentEmployee = _employee.GetEmployeeByUserName(user);

            ViewBag.UserName = CurrentEmployee.DisplayName;
            ViewBag.UserIsAdmin = _healthModel.UserIsAdmin(new Guid(CurrentEmployee.GUID));

			bool userHasReminders = await _healthModel.UserHasReminders(new Guid(CurrentEmployee.GUID));
            ViewBag.UserHasReminders = userHasReminders;


            _logger.LogInformation("Visitor: " + CurrentEmployee.DisplayName);




            if (ViewBag.UserIsAdmin)
            {
                string username = _config.GetSection("ApiAuthConfig")["userName"];
                string password = _config.GetSection("ApiAuthConfig")["password"];

                ViewBag.ApiAuthUserName = username;
                ViewBag.ApiAuthPassword = password;
            }
            



            HomeViewModel model = new()
            {
                Username = user,
                DisplayName = CurrentEmployee.DisplayName,
            };


            return View(model);
        }



        public async Task<IActionResult> LogViewer()
        {

            var user = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");

            var CurrentEmployee = _employee.GetEmployeeByUserName(user);

            ViewBag.UserName = CurrentEmployee.DisplayName;
            ViewBag.UserIsAdmin = _healthModel.UserIsAdmin(new Guid(CurrentEmployee.GUID));


            var model = new LogViewerModel()
            {
                Today = DateTime.Now,
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

            return View(model);
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
