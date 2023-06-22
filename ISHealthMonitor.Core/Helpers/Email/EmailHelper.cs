using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using ISHealthMonitor.Core.Data.Models;
using RazorEngine;
using RazorEngine.Templating;

namespace ISHealthMonitor.Core.Helpers.Email
{

    public class EmailHelper
    {

		public static void SendEmail(EmailReminderModel model, string rootDir)
        {

			string templatePath;

			if (rootDir == "local")
			{
				templatePath = Path.Combine(Environment.CurrentDirectory, model.TemplateRelativePath);

			}
			else
			{
				templatePath = Path.Combine(rootDir, model.TemplateRelativePath);
			}

			var Template = File.ReadAllText(templatePath);

            string body = Engine.Razor.RunCompile(new LoadedTemplateSource(Template),
                                        "templateKey",
                                        typeof(EmailReminderModel),
                                        model);

			var message = new System.Net.Mail.MailMessage();
			message.IsBodyHtml = true;
			message.From = new System.Net.Mail.MailAddress(model.From);
			message.Subject = String.Format(model.Subject);
			message.Body = body.ToString();

			var smtp = new System.Net.Mail.SmtpClient("PostOffice0");

            foreach (var email in model.Emails)
            {
				message.To.Add(email);
			}
            
			smtp.Send(message);

		}

    }
}
