using AutomationAssignment.Api;
using AutomationAssignment.Pages;
using AutomationAssignment.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationAssignment.Tests
{
    public class DebuggingFeaturesUiTests : BaseTest
    {
        [Test,Order(1)]
        public async Task Extract_DebuggingFeatures_Text_From_UI()
        {
            var wikiPage = new WikiPage(page);
            var wikiApi = new WikiApi();

            await wikiPage.NavigateAsync();
            ReportContext.AddLine("Starting Extract_DebuggingFeatures_Text_From_UI");

            await wikiPage.NavigateAsync();
            await wikiPage.OpenDebuggingFeaturesAsync();

            var uiText = await wikiPage.GetDebuggingFeaturesTextAsync();
            var apiText = await wikiApi.GetDebuggingFeaturesTextAsync();

            var uiCount = TextUtils.CountUnique(uiText);
            var apiCount = TextUtils.CountUnique(apiText);

            ReportContext.AddLine($"UI Unique Words Count: {uiCount}");
            ReportContext.AddLine($"API Unique Words Count: {apiCount}");
            ReportContext.AddLine($"Counts Match: {uiCount == apiCount}");

            Assert.That(uiText, Is.Not.Empty, "UI text is empty.");
            Assert.That(apiText, Is.Not.Empty, "API text is empty.");
            Assert.That(uiCount, Is.EqualTo(apiCount),
                $"Unique word count mismatch. UI={uiCount}, API={apiCount}");
        }
        [Test, Order(2)]
        public async Task TechnologyNames_AreLinks()
        {
            var wikiPage = new WikiPage(page);
            var pageObj = new WikiPage(page);
            await wikiPage.NavigateAsync();
            ReportContext.AddLine("Started test.");

            await pageObj.NavigateAsync();
            await pageObj.OpenDebuggingFeaturesAsync();

            var links = await pageObj.GetDevelopmentToolsAsync();

            Assert.That(links.Count, Is.GreaterThan(0), "No technology links found");

            ReportContext.AddLine($"Total Links Found: {links.Count}");
            ReportContext.AddLine("");

            foreach (var link in links)
            {
                var text = (await link.InnerTextAsync()).Trim();
                var href = await link.GetAttributeAsync("href");
                var tag = await link.EvaluateAsync<string>("el => el.tagName");

                var fullHref = href.StartsWith("http")
                    ? href
                    : $"https://en.wikipedia.org{href}";

                ReportContext.AddLine($"TEXT: {text}");
                ReportContext.AddLine($"HREF: {href}");
                ReportContext.AddLine($"TAG: {tag}");
                ReportContext.AddLine($"LINK: <a href='{fullHref}' target='_blank'>{text}</a>");
                ReportContext.AddLine("----------------------");
            }
        }
        [Test , Order(3)]
        public async Task ColorTheme_CanBeChanged_To_Dark()
        {
            var wikiPage = new WikiPage(page);

            await wikiPage.NavigateAsync();

            var beforeBody = await wikiPage.GetBodyBackgroundColorAsync();

            ReportContext.AddLine("=== COLOR TEST ===");
            ReportContext.AddLine($"Before BODY color: {beforeBody}");
            ReportContext.AddLine("");

            await wikiPage.SetColorToDarkAsync();

            var afterBody = await wikiPage.GetBodyBackgroundColorAsync();

            ReportContext.AddLine($"After BODY color: {afterBody}");
            ReportContext.AddLine("");

            var bodyChanged = beforeBody != afterBody;

            ReportContext.AddLine($"Body changed: {bodyChanged}");
            ReportContext.AddLine("");

            Assert.That(bodyChanged, Is.True,
                "Body background color did not change after switching to Dark mode.");
        }
    }
}
