using Microsoft.Playwright;
using AutomationAssignment.Utils;

namespace AutomationAssignment.Pages
{
    public class WikiPage
    {
        private readonly IPage _page;

        public WikiPage(IPage page)
        {
            _page = page;
        }

        public async Task NavigateAsync()
        {
            const string url = "https://en.wikipedia.org/wiki/Playwright_(software)";

            ReportContext.SetUrl(url);
            ReportContext.AddLine("=== NAVIGATION ===");
            ReportContext.AddLine($"Navigating to: {url}");
            ReportContext.AddLine("");

            await _page.GotoAsync(url);
        }

        public async Task<string> GetDebuggingFeaturesTextAsync()
        {
            var tocLink = _page.Locator("a[href='#Debugging_features']");
            await tocLink.First.ClickAsync();

            var sectionElements = _page.Locator(
                "xpath=//div[contains(@class,'mw-heading')][.//h3[@id='Debugging_features']]/following-sibling::*"
            );

            var texts = new List<string>();
            var count = await sectionElements.CountAsync();

            for (int i = 0; i < count; i++)
            {
                var element = sectionElements.Nth(i);

                var className = await element.GetAttributeAsync("class") ?? "";
                var tagName = await element.EvaluateAsync<string>("el => el.tagName");

                if (className.Contains("mw-heading") ||
                    tagName.Equals("H2", StringComparison.OrdinalIgnoreCase) ||
                    tagName.Equals("H3", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                var text = await element.InnerTextAsync();

                if (!string.IsNullOrWhiteSpace(text))
                    texts.Add(text.Trim());
            }

            var finalText = string.Join(" ", texts);

            ReportContext.AddLine("=== UI DEBUGGING FEATURES TEXT ===");
            ReportContext.AddLine(finalText);
            ReportContext.AddLine("");
            ReportContext.AddLine($"UI Text Length: {finalText.Length}");
            ReportContext.AddLine("");

            return finalText;
        }

        public async Task OpenDebuggingFeaturesAsync()
        {
            var tocLink = _page.Locator("a[href='#Debugging_features']");

            await tocLink.First.WaitForAsync();
            await tocLink.First.ScrollIntoViewIfNeededAsync();
            await tocLink.First.ClickAsync();

            ReportContext.AddLine("=== ACTION ===");
            ReportContext.AddLine("Opened 'Debugging features' section from table of contents.");
            ReportContext.AddLine("");
        }

        public async Task ExpandMicrosoftDevelopmentToolsAsync()
        {
            var showButton = _page.Locator(
                "xpath=//div[contains(@class,'navbox')]//th[contains(.,'Microsoft development tools')]/following::a[normalize-space()='show'][1]"
            );

            await showButton.First.WaitForAsync();
            await showButton.First.ScrollIntoViewIfNeededAsync();
            await showButton.First.ClickAsync();

            ReportContext.AddLine("=== ACTION ===");
            ReportContext.AddLine("Expanded 'Microsoft development tools'.");
            ReportContext.AddLine("");
        }

        public async Task<List<ILocator>> GetDevelopmentToolsAsync()
        {
            var tocLink = _page.Locator("a[href='#Debugging_features']");
            await tocLink.First.ClickAsync();

            var showButton = _page.Locator(
                "xpath=//th[contains(normalize-space(.), 'Microsoft development tools')]//button[.//span[normalize-space()='show']]"
            );

            await showButton.First.WaitForAsync();
            await showButton.First.ScrollIntoViewIfNeededAsync();
            await showButton.First.ClickAsync();

            var links = _page.Locator(
                "xpath=(//th[contains(normalize-space(.), 'Microsoft development tools')]/ancestor::table[1]//table[contains(@class,'navbox-subgroup')])[1]//a"
            );

            var list = new List<ILocator>();
            var count = await links.CountAsync();

            ReportContext.AddLine("=== MICROSOFT DEVELOPMENT TOOLS LINKS ===");
            ReportContext.AddLine($"Total raw links found: {count}");
            ReportContext.AddLine("");

            for (int i = 0; i < count; i++)
            {
                var link = links.Nth(i);
                var text = (await link.InnerTextAsync()).Trim();
                var href = await link.GetAttributeAsync("href");
                var tag = await link.EvaluateAsync<string>("el => el.tagName");

                if (!string.IsNullOrWhiteSpace(text))
                {
                    list.Add(link);

                    ReportContext.AddLine($"TEXT: {text}");
                    ReportContext.AddLine($"HREF: {href}");
                    ReportContext.AddLine($"TAG: {tag}");

                    if (!string.IsNullOrWhiteSpace(href))
                    {
                        ReportContext.AddLink(text, href);
                    }

                    ReportContext.AddLine("----------------------");
                }
            }

            ReportContext.AddLine($"Filtered visible/non-empty links count: {list.Count}");
            ReportContext.AddLine("");

            return list;
        }

        public async Task<string> GetBodyBackgroundColorAsync()
        {
            return await _page.EvaluateAsync<string>(
                "() => getComputedStyle(document.body).backgroundColor");
        }

        public async Task<string> GetHtmlBackgroundColorAsync()
        {
            return await _page.EvaluateAsync<string>(
                "() => getComputedStyle(document.documentElement).backgroundColor");
        }

        public async Task SetColorToDarkAsync()
        {
            var darkOption = _page.Locator("xpath=//label[.//text()[normalize-space()='Dark']]").First;

            await darkOption.WaitForAsync();
            await darkOption.ScrollIntoViewIfNeededAsync();
            await darkOption.ClickAsync();

            await _page.WaitForTimeoutAsync(1000);
        }
    }
}