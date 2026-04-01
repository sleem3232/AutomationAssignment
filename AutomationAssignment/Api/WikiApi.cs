using System.Net.Http;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using AutomationAssignment.Utils;

namespace AutomationAssignment.Api
{
    public class WikiApi
    {
        private readonly HttpClient _client;

        public WikiApi()
        {
            _client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            _client.DefaultRequestHeaders.Add("User-Agent", "AutomationAssignment/1.0");
        }

        public async Task<string> GetDebuggingFeaturesTextAsync()
        {
            const string url =
                "https://en.wikipedia.org/w/api.php?action=parse&page=Playwright_(software)&format=json&prop=text";

            ReportContext.AddLine("=== API REQUEST ===");
            ReportContext.AddLine(url);
            ReportContext.AddLine("");

            try
            {
                HttpResponseMessage response = null;
                string content = null;

                for (int attempt = 1; attempt <= 3; attempt++)
                {
                    try
                    {
                        ReportContext.AddLine($"API Attempt #{attempt}");

                        response = await _client.GetAsync(url);
                        content = await response.Content.ReadAsStringAsync();

                        response.EnsureSuccessStatusCode();
                        break;
                    }
                    catch (Exception ex)
                    {
                        ReportContext.AddLine($"Attempt {attempt} failed: {ex.Message}");

                        if (attempt == 3)
                            throw;

                        await Task.Delay(2000);
                    }
                }

                if (response == null)
                    throw new Exception("API response is null after retries.");

                if (string.IsNullOrWhiteSpace(content))
                    throw new Exception("API content is empty after retries.");

                ReportContext.AddLine("=== API STATUS CODE ===");
                ReportContext.AddLine(response.StatusCode.ToString());
                ReportContext.AddLine("");

                var json = JObject.Parse(content);
                var html = json["parse"]?["text"]?["*"]?.ToString();

                if (string.IsNullOrWhiteSpace(html))
                    throw new Exception("No HTML returned from MediaWiki Parse API.");

                var extractedText = ExtractDebuggingFeatures(html);

                ReportContext.AddLine("=== API EXTRACTED TEXT ===");
                ReportContext.AddLine(extractedText);
                ReportContext.AddLine("");
                ReportContext.AddLine($"API Extracted Text Length: {extractedText.Length}");
                ReportContext.AddLine("");

                return extractedText;
            }
            catch (Exception ex)
            {
                ReportContext.AddLine("=== API ERROR ===");
                ReportContext.AddLine(ex.Message);
                ReportContext.AddLine("");
                throw;
            }
        }

        private string ExtractDebuggingFeatures(string html)
        {
            var marker = "id=\"Debugging_features\"";
            var startIndex = html.IndexOf(marker, StringComparison.OrdinalIgnoreCase);

            if (startIndex < 0)
                throw new Exception("Debugging features section not found in API response.");

            var afterHeading = html.IndexOf("</div>", startIndex, StringComparison.OrdinalIgnoreCase);

            if (afterHeading < 0)
                throw new Exception("Could not find end of Debugging features heading block.");

            afterHeading += "</div>".Length;

            var nextSection = html.IndexOf(
                "<div class=\"mw-heading mw-heading3\">",
                afterHeading,
                StringComparison.OrdinalIgnoreCase);

            var nextH2 = html.IndexOf("<h2", afterHeading, StringComparison.OrdinalIgnoreCase);

            int endIndex;

            if (nextSection == -1 && nextH2 == -1)
                endIndex = html.Length;
            else if (nextSection == -1)
                endIndex = nextH2;
            else if (nextH2 == -1)
                endIndex = nextSection;
            else
                endIndex = Math.Min(nextSection, nextH2);

            var sectionHtml = html.Substring(afterHeading, endIndex - afterHeading);

            sectionHtml = Regex.Replace(sectionHtml, "<.*?>", " ");
            sectionHtml = System.Net.WebUtility.HtmlDecode(sectionHtml);
            sectionHtml = Regex.Replace(sectionHtml, @"\s+", " ").Trim();

            return sectionHtml;
        }
    }
}