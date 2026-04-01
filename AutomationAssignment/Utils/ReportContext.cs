using System.Text;

namespace AutomationAssignment.Utils
{
    public static class ReportContext
    {
        private static StringBuilder _details = new();
        private static string _url = string.Empty;

        public static void Start()
        {
            _details = new StringBuilder();
            _url = string.Empty;
        }

        public static void SetUrl(string url)
        {
            _url = url ?? string.Empty;
        }

        public static string GetUrl()
        {
            return _url;
        }

        public static void AddLine(string line)
        {
            _details.AppendLine(line);
        }

        public static void AddLink(string text, string href)
        {
            if (string.IsNullOrWhiteSpace(href))
                return;

            var fullHref = href.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? href
                : $"https://en.wikipedia.org{href}";

            _details.AppendLine(
                $"LINK: <a href='{fullHref}' target='_blank'>{text}</a>");
        }

        public static string GetDetails()
        {
            return _details.ToString();
        }

        public static void Clear()
        {
            _details.Clear();
            _url = string.Empty;
        }
    }
}