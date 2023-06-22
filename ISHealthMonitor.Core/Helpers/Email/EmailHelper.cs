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

		public static void SendEmail(EmailReminderModel model, string rootDir, int key)
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



            if (!string.IsNullOrEmpty(model.SSLThumbprint))
            {
                var formatted = string.Join(" ", Enumerable.Range(0, model.SSLThumbprint.Length / 2).Select(i => model.SSLThumbprint.Substring(i * 2, 2)));
                model.SSLThumbprint = formatted;
            }


            string body = Engine.Razor.RunCompile(new LoadedTemplateSource(Template),
                                        "templateKey" + key.ToString(),
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
