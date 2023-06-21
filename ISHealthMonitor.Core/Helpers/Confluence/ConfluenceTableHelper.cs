using ISHealthMonitor.Core.Helpers.Email;
using RazorEngine.Templating;
using RazorEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Helpers.Confluence
{
    public class ConfluenceTableHelper
    {

        public static string GetSiteTableHTML(ConfluenceTableModel model, string rootDir)
        {

            string templatePath = "";

            if (rootDir == "local")
            {
                templatePath = Path.Combine(Path.GetDirectoryName(Environment.CurrentDirectory), model.TemplateRelativePath);

			}
            else
            {
				templatePath = Path.Combine(rootDir, model.TemplateRelativePath);
			}

			var Template = File.ReadAllText(templatePath);

            foreach (var site in model.sites)
            {
                if (!string.IsNullOrEmpty(site.SSLThumbprint))
                {
                    var formatted = string.Join(" ", Enumerable.Range(0, site.SSLThumbprint.Length / 2).Select(i => site.SSLThumbprint.Substring(i * 2, 2)));
                    site.SSLThumbprint = formatted;
                }
            }



            string body = Engine.Razor.RunCompile(new LoadedTemplateSource(Template),
                                        "templateKey",
                                        typeof(ConfluenceTableModel),
                                        model);

            //File.WriteAllText("C:\\Users\\bschwartz\\source\\repos\\ishealthmonitor\\ISHealthMonitor.Core\\Helpers\\Email\\table.html", body);
            return body;
        }
    }
}
