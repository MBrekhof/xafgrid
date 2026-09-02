using Microsoft.Playwright;

namespace XafGrid.Tests.Playwright;

public class DemoTests : XafTest {
    [Test]
    public async Task D1_master_detail_row_expands_to_nested_lines_grid() {
        await OpenViewAsync("Order_ListView_MasterDetail");
        await DataRows.First.Locator("[class*='expand-button']").First.ClickAsync();
        var detail = Page.Locator(".xg-detail");
        await Expect(detail).ToBeVisibleAsync();
        await Expect(detail.Locator(".dxbl-grid tbody tr[data-visible-index]").First).ToBeVisibleAsync();
        await ScreenshotAsync("d1-master-detail");
    }

    [Test]
    public async Task D2_rich_cells_render_badges_avatars_and_bars() {
        await OpenViewAsync("Order_ListView_Cells");
        await Expect(Page.Locator(".xg-badge").First).ToBeVisibleAsync();
        Assert.Multiple(async () => {
            Assert.That(await Page.Locator(".xg-badge").CountAsync(), Is.GreaterThanOrEqualTo(10));
            Assert.That(await Page.Locator(".xg-avatar").CountAsync(), Is.GreaterThanOrEqualTo(10));
            Assert.That(await Page.Locator(".xg-bar-fill").CountAsync(), Is.GreaterThanOrEqualTo(10));
        });
        await ScreenshotAsync("d2-cells");
    }

    [Test]
    public async Task D3_heatmap_classes_are_applied_per_row_and_cell() {
        await OpenViewAsync("Order_ListView_Heatmap");
        await Expect(Page.Locator("tr[class*='xg-heat-']").First).ToBeVisibleAsync();
        Assert.Multiple(async () => {
            Assert.That(await Page.Locator("tr[class*='xg-heat-']").CountAsync(), Is.GreaterThanOrEqualTo(10));
            Assert.That(await Page.Locator("td.xg-hot").CountAsync(), Is.GreaterThanOrEqualTo(1));
        });
        await ScreenshotAsync("d3-heatmap");
    }
}
