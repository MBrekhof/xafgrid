namespace XafGrid.Tests.Playwright;

public class SmokeTests : XafTest {
    [Test]
    public async Task Orders_grid_renders_seeded_rows() {
        await OpenViewAsync("Order_ListView");
        await Expect(DataRows.First).ToBeVisibleAsync();
        Assert.That(await DataRows.CountAsync(), Is.GreaterThanOrEqualTo(10));
        await ScreenshotAsync("p0-orders");
    }
}
