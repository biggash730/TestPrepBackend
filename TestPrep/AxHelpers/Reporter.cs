using System.IO;
using System.Web;
using PdfSharp;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace TestPrep.AxHelpers
{
    public class Reporter
    {
        public static byte[] GeneratePdf(string html, PageSize size, bool landscape = false)
        {
            using (var ms = new MemoryStream())
            {
                var config = new PdfGenerateConfig
                {
                    PageOrientation = landscape ? PageOrientation.Landscape : PageOrientation.Portrait,
                    PageSize = size,
                    MarginBottom = 20,
                    MarginLeft = 20,
                    MarginTop = 20,
                    MarginRight = 20
                };
                var css = File.ReadAllText(HttpContext.Current.Server.MapPath(@"~/ReportTemplates/ReportStyle.css"));
                var pdf = PdfGenerator.GeneratePdf(html, config, PdfGenerator.ParseStyleSheet(css));
                pdf.Save(ms);
                return ms.ToArray();
            }
        }
    }
}