using System.Net;
using System.Text;

namespace AutomationAssignment.Utils
{
    public static class HtmlReportManager
    {
        private static readonly object _lock = new();
        private static bool _initialized;
        private static string _folderPath = string.Empty;
        private static string _filePath = string.Empty;

        private static int _passedCount;
        private static int _failedCount;
        private static int _totalCount;

        public static void Init()
        {
            if (_initialized)
                return;

            var projectRoot = FindProjectRoot();

            _folderPath = Path.Combine(projectRoot, "TestResults");
            _filePath = Path.Combine(_folderPath, "latest-report.html");

            if (!Directory.Exists(_folderPath))
                Directory.CreateDirectory(_folderPath);

            _passedCount = 0;
            _failedCount = 0;
            _totalCount = 0;

            File.WriteAllText(_filePath, BuildHtmlStart(), Encoding.UTF8);

            _initialized = true;
        }

        public static void AddResult(string testName, string status, string details, string url = "")
        {
            if (!_initialized)
                Init();

            lock (_lock)
            {
                _totalCount++;

                var isPassed = status.Equals("Passed", StringComparison.OrdinalIgnoreCase);
                if (isPassed)
                    _passedCount++;
                else
                    _failedCount++;

                var statusClass = isPassed ? "passed" : "failed";
                var encodedTestName = WebUtility.HtmlEncode(testName);
                var encodedUrl = WebUtility.HtmlEncode(url);
                var safeDetails = details.Replace("\n", "<br>");

                var block = $@"
<section class='test-card'>
    <div class='test-card-header'>
        <div>
            <h2>{encodedTestName}</h2>
            <div class='badge {statusClass}'>{WebUtility.HtmlEncode(status)}</div>
        </div>
    </div>

    <div class='meta-grid'>
        <div class='meta-item'>
            <span class='meta-label'>URL</span>
            <span class='meta-value'>
                <a href='{encodedUrl}' target='_blank'>{encodedUrl}</a>
            </span>
        </div>
    </div>

    <div class='details-box'>
        {safeDetails}
    </div>
</section>";

                File.AppendAllText(_filePath, block, Encoding.UTF8);
            }
        }

        public static void FinalizeReport()
        {
            if (!_initialized)
                return;

            var summary = $@"
<script>
    document.getElementById('total-tests').textContent = '{_totalCount}';
    document.getElementById('passed-tests').textContent = '{_passedCount}';
    document.getElementById('failed-tests').textContent = '{_failedCount}';
</script>
</body>
</html>";

            File.AppendAllText(_filePath, summary, Encoding.UTF8);
        }

        public static string GetFilePath()
        {
            if (!_initialized)
                Init();

            return _filePath;
        }

        private static string BuildHtmlStart()
        {
            return @"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Automation Assignment Report</title>
    <style>
        * {
            box-sizing: border-box;
        }

        body {
            margin: 0;
            font-family: Arial, sans-serif;
            background: #f4f7fb;
            color: #1f2937;
        }

        .container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 32px 20px 48px;
        }

        .header {
            background: linear-gradient(135deg, #1d4ed8, #2563eb);
            color: white;
            padding: 28px;
            border-radius: 18px;
            box-shadow: 0 10px 30px rgba(37, 99, 235, 0.20);
            margin-bottom: 28px;
        }

        .header h1 {
            margin: 0 0 8px;
            font-size: 34px;
        }

        .header p {
            margin: 0;
            opacity: 0.95;
            font-size: 15px;
        }

        .summary {
            display: grid;
            grid-template-columns: repeat(3, minmax(140px, 1fr));
            gap: 16px;
            margin-bottom: 28px;
        }

        .summary-card {
            background: white;
            border-radius: 16px;
            padding: 20px;
            box-shadow: 0 8px 24px rgba(15, 23, 42, 0.08);
            border: 1px solid #e5e7eb;
        }

        .summary-card .label {
            display: block;
            font-size: 13px;
            color: #6b7280;
            margin-bottom: 8px;
        }

        .summary-card .value {
            font-size: 30px;
            font-weight: 700;
        }

        .tests {
            display: flex;
            flex-direction: column;
            gap: 20px;
        }

        .test-card {
            background: white;
            border-radius: 18px;
            padding: 22px;
            box-shadow: 0 8px 24px rgba(15, 23, 42, 0.08);
            border: 1px solid #e5e7eb;
        }

        .test-card-header {
            display: flex;
            justify-content: space-between;
            align-items: start;
            gap: 16px;
            margin-bottom: 18px;
        }

        .test-card h2 {
            margin: 0 0 10px;
            font-size: 28px;
            word-break: break-word;
        }

        .badge {
            display: inline-block;
            padding: 7px 14px;
            border-radius: 999px;
            font-size: 13px;
            font-weight: 700;
            letter-spacing: 0.2px;
        }

        .badge.passed {
            background: #dcfce7;
            color: #166534;
        }

        .badge.failed {
            background: #fee2e2;
            color: #991b1b;
        }

        .meta-grid {
            display: grid;
            grid-template-columns: 1fr;
            gap: 12px;
            margin-bottom: 18px;
        }

        .meta-item {
            background: #f8fafc;
            border: 1px solid #e5e7eb;
            border-radius: 12px;
            padding: 14px;
        }

        .meta-label {
            display: block;
            font-size: 12px;
            color: #6b7280;
            text-transform: uppercase;
            letter-spacing: 0.6px;
            margin-bottom: 6px;
        }

        .meta-value {
            font-size: 14px;
            word-break: break-all;
        }

        .meta-value a {
            color: #2563eb;
            text-decoration: none;
        }

        .meta-value a:hover {
            text-decoration: underline;
        }

        .details-box {
            background: #0f172a;
            color: #e5e7eb;
            border-radius: 14px;
            padding: 18px;
            line-height: 1.65;
            font-size: 14px;
            white-space: normal;
            overflow-x: auto;
            border: 1px solid #1e293b;
        }

        .details-box a {
            color: #93c5fd;
            text-decoration: none;
        }

        .details-box a:hover {
            text-decoration: underline;
        }

        @media (max-width: 700px) {
            .summary {
                grid-template-columns: 1fr;
            }

            .header h1 {
                font-size: 28px;
            }

            .test-card h2 {
                font-size: 22px;
            }
        }
    </style>
</head>
<body>
    <div class='container'>
        <header class='header'>
            <h1>Automation Assignment Report</h1>
            <p>Generated automatically from the test execution.</p>
        </header>

        <section class='summary'>
            <div class='summary-card'>
                <span class='label'>Total Tests</span>
                <span class='value' id='total-tests'>0</span>
            </div>
            <div class='summary-card'>
                <span class='label'>Passed</span>
                <span class='value' id='passed-tests'>0</span>
            </div>
            <div class='summary-card'>
                <span class='label'>Failed</span>
                <span class='value' id='failed-tests'>0</span>
            </div>
        </section>

        <main class='tests'>
";
        }

        private static string FindProjectRoot()
        {
            var current = new DirectoryInfo(AppContext.BaseDirectory);

            while (current != null)
            {
                if (current.GetFiles("*.csproj").Any())
                    return current.FullName;

                current = current.Parent;
            }

            throw new DirectoryNotFoundException("Could not find project root (.csproj).");
        }
    }
}