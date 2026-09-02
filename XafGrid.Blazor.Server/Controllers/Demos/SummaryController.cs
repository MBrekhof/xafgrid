using DevExpress.Blazor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Editors;
using DevExpress.ExpressApp.Blazor.Editors.Models;
using XafGrid.Blazor.Server.Components.Grid;
using XafGrid.Module.BusinessObjects;

namespace XafGrid.Blazor.Server.Controllers.Demos;

/// <summary>
/// D5 — total summaries added in code (Sum / Avg / Count), a Custom summary over the *selected* rows
/// (CustomSummary + CustomizeSummaryDisplayText) and a stacked footer (ColumnFooterTemplate).
/// </summary>
public class SummaryController : ObjectViewController<ListView, Order> {
    const string SelectedSum = "SelectedSum";

    public SummaryController() {
        TargetViewId = "Order_ListView_Summary";
    }

    protected override void OnActivated() {
        base.OnActivated();
        View.SelectionChanged += View_SelectionChanged;
    }

    protected override void OnDeactivated() {
        View.SelectionChanged -= View_SelectionChanged;
        base.OnDeactivated();
    }

    void View_SelectionChanged(object sender, EventArgs e) {
        // GridInstance is only valid after render; never cache it
        if(View.Editor is DxGridListEditor editor) editor.GridInstance?.RefreshSummary();
    }

    protected override void OnViewControlsCreated() {
        base.OnViewControlsCreated();
        if(View.Editor is not DxGridListEditor editor) return;
        var grid = editor.GridModel;

        var total = editor.GridSummary.TotalSummary;
        total.Add(Item(nameof(Order.Number), GridSummaryItemType.Count));
        total.Add(Item(nameof(Order.Total), GridSummaryItemType.Sum, "c0"));
        total.Add(Item(nameof(Order.Total), GridSummaryItemType.Avg, "c0"));
        total.Add(Item(nameof(Order.Total), GridSummaryItemType.Custom, "c0", SelectedSum));
        grid.FooterDisplayMode = GridFooterDisplayMode.Always;

        grid.CustomSummary += e => {
            if(e.Item.Name != SelectedSum) return;
            switch(e.SummaryStage) {
                case GridCustomSummaryStage.Start:
                    e.TotalValue = 0m;
                    break;
                case GridCustomSummaryStage.Calculate:
                    if(e.Grid.IsDataItemSelected(e.DataItem))
                        e.TotalValue = (decimal)e.TotalValue + Convert.ToDecimal(e.GetRowValue(nameof(Order.Total)));
                    break;
            }
        };
        grid.CustomizeSummaryDisplayText += e => {
            if(e.Item.Name == SelectedSum) e.DisplayText = $"Selected: {e.Value:c0}";
        };

        grid.ColumnFooterTemplate = ctx => builder => {
            builder.OpenComponent<FooterCell>(0);
            builder.AddAttribute(1, nameof(FooterCell.Context), ctx);
            builder.CloseComponent();
        };
    }

    static DxGridSummaryItemWrapper Item(string field, GridSummaryItemType type, string format = null, string name = null) =>
        new(new DxGridSummaryItemModel { FieldName = field, SummaryType = type, ValueDisplayFormat = format, Name = name });
}
