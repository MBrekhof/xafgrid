using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework.Interfaces;

namespace XafGrid.Tests.Playwright;

public abstract class XafTest : PageTest {
    public override BrowserNewContextOptions ContextOptions() => new() {
        ViewportSize = new ViewportSize { Width = 1600, Height = 1000 },
        IgnoreHTTPSErrors = true,
    };

    [SetUp]
    public void Timeouts() => SetDefaultExpectTimeout(60_000); // XAF cold start on first circuit

    protected ILocator Grid => Page.Locator(".dxbl-grid").First;
    /// <summary>DX 26.1 data rows carry no dedicated class — only data-visible-index; filler rows are .dxbl-grid-empty-row.</summary>
    protected ILocator DataRows => Page.Locator(".dxbl-grid tbody tr[data-visible-index]");

    /// <summary>XAF Blazor routes every view at /{ViewId}.</summary>
    protected async Task OpenViewAsync(string viewId) {
        await Page.GotoAsync($"{AppHost.BaseUrl}/{viewId}", new() { WaitUntil = WaitUntilState.NetworkIdle });
        // the static splash (#applicationLoadingPanel) overlays the app until XAF's JS reveals <app>; rows exist in the DOM before it fades
        await Expect(Page.Locator("#applicationLoadingPanel")).ToBeHiddenAsync();
        await Expect(Grid).ToBeVisibleAsync();
    }

    /// <summary>XAF restores the theme from the XAF_CurrentTheme cookie (classic theme = its caption from appsettings ThemeSwitcher).</summary>
    protected Task UseDarkThemeAsync() => Context.AddCookiesAsync(new[] {
        new Cookie { Name = "XAF_CurrentTheme", Value = "Blazing Dark", Url = AppHost.BaseUrl },
    });

    /// <summary>Evidence screenshots are committed under docs/screens.</summary>
    protected async Task ScreenshotAsync(string name) {
        var dir = Path.Combine(AppHost.RepoRoot, "docs", "screens");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, name + ".png");
        await Page.ScreenshotAsync(new() { Path = path, Animations = ScreenshotAnimations.Disabled });
        TestContext.AddTestAttachment(path);
    }

    [TearDown]
    public async Task ScreenshotOnFailure() {
        if(TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
            await ScreenshotAsync("FAIL_" + TestContext.CurrentContext.Test.Name);
    }
}
