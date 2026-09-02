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

        await UseDarkThemeAsync();
        await OpenViewAsync("Order_ListView_Cells");
        await Expect(Page.Locator(".xg-badge").First).ToBeVisibleAsync();
        await ScreenshotAsync("d2-cells-dark");
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

        await UseDarkThemeAsync();
        await OpenViewAsync("Order_ListView_Heatmap");
        await Expect(Page.Locator("tr[class*='xg-heat-']").First).ToBeVisibleAsync();
        await ScreenshotAsync("d3-heatmap-dark");
    }

    [Test]
    public async Task D4_custom_grouping_buckets_dates_with_group_row_template() {
        await OpenViewAsync("Order_ListView_Groups");
        var labels = Page.Locator(".xg-group-label");
        await Expect(labels.First).ToBeVisibleAsync();
        // virtual scrolling renders only the visible groups; the first bucket is always "This week"
        var texts = await labels.AllInnerTextsAsync();
        Assert.That(texts.First(), Is.EqualTo("This week"));
        Assert.That(texts, Is.All.AnyOf("This week", "This month", "This quarter", "This year", "Older"));
        await Expect(Page.Locator(".xg-group-bar").First).ToBeVisibleAsync();
        await ScreenshotAsync("d4-groups");
    }

    [Test]
    public async Task D5_custom_summary_reflects_selection() {
        await OpenViewAsync("Order_ListView_Summary");
        var footer = Page.Locator(".xg-footer-item");
        await Expect(footer.First).ToBeVisibleAsync();
        await Expect(Page.Locator(".xg-footer-item", new() { HasText = "Selected: $0" })).ToBeVisibleAsync();

        await DataRows.First.Locator("input[type='checkbox']").First.CheckAsync();
        await Expect(Page.Locator(".xg-footer-item", new() { HasTextRegex = new("Selected: \\$[1-9]") })).ToBeVisibleAsync();
        await ScreenshotAsync("d5-summary");
    }

    [Test]
    public async Task D6_toolbar_groups_and_exports() {
        await OpenViewAsync("Order_ListView_Toolbar");
        var toolbar = Page.Locator(".xg-toolbar");
        await Expect(toolbar).ToBeVisibleAsync();

        await toolbar.GetByRole(AriaRole.Button, new() { Name = "Group by status" }).ClickAsync();
        await Expect(Page.Locator(".dxbl-grid-group-row").First).ToBeVisibleAsync();
        await ScreenshotAsync("d6-toolbar");

        var download = await Page.RunAndWaitForDownloadAsync(() => toolbar.GetByRole(AriaRole.Button, new() { Name = "XLSX" }).ClickAsync());
        Assert.That(download.SuggestedFilename, Does.EndWith(".xlsx"));
    }

    [Test]
    public async Task D7_unbound_columns_compute_and_act() {
        await OpenViewAsync("Order_ListView_Unbound");
        await Expect(Page.Locator(".dxbl-grid-header", new() { HasText = "Lines" })).ToBeVisibleAsync();
        await Expect(Page.Locator(".dxbl-grid-header", new() { HasText = "Margin (20%)" })).ToBeVisibleAsync();
        await Expect(DataRows.First.Locator("td", new() { HasTextRegex = new("^\\d+$") }).First).ToBeVisibleAsync();

        // pin the row by its order number: "HasText = New" would re-resolve to the next New row after shipping
        var number = await Page.Locator(".dxbl-grid tbody tr[data-visible-index]", new() { HasText = "New" }).First
            .Locator("td").Nth(1).InnerTextAsync();
        var row = Page.Locator(".dxbl-grid tbody tr[data-visible-index]", new() { HasText = number });
        await row.GetByRole(AriaRole.Button, new() { Name = "Ship" }).ClickAsync();
        await Expect(row).ToContainTextAsync("Shipped");
        await Expect(Grid).ToBeVisibleAsync(); // still on the list view — the click must not open the DetailView
        await ScreenshotAsync("d7-unbound");
    }

    // first alphabetic cell of a product row = Name
    static ILocator NameCell(ILocator row) => row.Locator("td", new() { HasTextRegex = new("[A-Za-z]") }).First;

    [Test]
    public async Task D8_drag_row_reorders_and_persists() {
        await OpenViewAsync("Product_ListView_Reorder");
        var first = await NameCell(DataRows.Nth(0)).InnerTextAsync();
        var third = await NameCell(DataRows.Nth(2)).InnerTextAsync();
        Assert.That(third, Is.Not.EqualTo(first));

        // drag row 3 by its handle onto the top edge of row 1 — DevExpress needs intermediate pointer moves
        var handle = DataRows.Nth(2).Locator("[class*='drag']").First;
        var target = await DataRows.Nth(0).BoundingBoxAsync();
        await handle.HoverAsync();
        await Page.Mouse.DownAsync();
        await Page.Mouse.MoveAsync(target!.X + 40, target.Y + target.Height / 2, new() { Steps = 10 });
        await Page.Mouse.MoveAsync(target.X + 40, target.Y + 3, new() { Steps = 10 });
        await Page.Mouse.UpAsync();

        await Expect(NameCell(DataRows.Nth(0))).ToHaveTextAsync(third);
        await ScreenshotAsync("d8-reorder");

        await OpenViewAsync("Product_ListView_Reorder"); // persisted?
        await Expect(NameCell(DataRows.Nth(0))).ToHaveTextAsync(third);
    }

    [Test]
    public async Task D9_layout_preset_round_trips_through_the_database() {
        await OpenViewAsync("Order_ListView_Presets");
        var toolbar = Page.Locator(".xg-presets");
        await Expect(toolbar).ToBeVisibleAsync();
        var firstNumber = DataRows.First.Locator("td").Nth(1);
        await Expect(firstNumber).ToHaveTextAsync("ORD-00001");

        await Page.Locator(".dxbl-grid-header", new() { HasText = "Total" }).ClickAsync(); // sort by Total
        await Expect(firstNumber).Not.ToHaveTextAsync("ORD-00001");
        var sortedFirst = await firstNumber.InnerTextAsync();

        await toolbar.Locator(".xg-preset-name input:visible").FillAsync("ByTotal"); // DxToolbar keeps a hidden measurement clone
        await toolbar.GetByRole(AriaRole.Button, new() { Name = "Save layout" }).ClickAsync();
        await Expect(toolbar.GetByRole(AriaRole.Button, new() { Name = "ByTotal" })).ToBeVisibleAsync();

        await toolbar.GetByRole(AriaRole.Button, new() { Name = "Reset sort" }).ClickAsync(); // ClearSort drops every sort → natural DB order
        await Expect(firstNumber).Not.ToHaveTextAsync(sortedFirst);

        await toolbar.GetByRole(AriaRole.Button, new() { Name = "ByTotal" }).ClickAsync();
        await Expect(firstNumber).ToHaveTextAsync(sortedFirst);
        await ScreenshotAsync("d9-presets");
    }

    [Test]
    public async Task D10_fixed_columns_stay_put_while_the_grid_scrolls() {
        await OpenViewAsync("Order_ListView_Columns");
        var number = Page.Locator(".dxbl-grid-header", new() { HasText = "Number" });
        var orderDate = Page.Locator(".dxbl-grid-header", new() { HasText = "Order Date" });
        var before = await number.BoundingBoxAsync();
        var dateBefore = await orderDate.BoundingBoxAsync();

        var scrolled = await Page.EvaluateAsync<bool>(@"() => {
            const el = [...document.querySelectorAll('.dxbl-grid *')].find(e => e.scrollWidth > e.clientWidth + 20);
            if (!el) return false; el.scrollLeft = 500; el.dispatchEvent(new Event('scroll')); return true; }");
        Assert.That(scrolled, Is.True, "grid should be wider than its viewport");
        await Page.WaitForTimeoutAsync(500); // let the virtualized columns re-render

        var after = await number.BoundingBoxAsync();
        Assert.That(after!.X, Is.EqualTo(before!.X).Within(1), "fixed column moved");
        var dateAfter = await orderDate.BoundingBoxAsync();
        Assert.That(dateAfter is null || dateAfter.X < dateBefore!.X - 100, "scrollable column did not move");
        await ScreenshotAsync("d10-wide");
    }
}
