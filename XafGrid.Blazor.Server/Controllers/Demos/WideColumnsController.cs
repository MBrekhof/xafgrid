using DevExpress.Blazor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Editors;
using XafGrid.Module.BusinessObjects;

namespace XafGrid.Blazor.Server.Controllers.Demos;

/// <summary>
/// D10 — a grid wider than the viewport: fixed first/last columns, header caption templates with
/// tooltips, per-column filter-button mode, row + column virtualization.
/// </summary>
public class WideColumnsController : ObjectViewController<ListView, Order> {
    public WideColumnsController() {
        TargetViewId = "Order_ListView_Columns";
    }

    protected override void OnViewControlsCreated() {
        base.OnViewControlsCreated();
        if(View.Editor is not DxGridListEditor editor) return;

        editor.GridModel.VirtualScrollingMode = GridVirtualScrollingMode.RowsAndColumns; // VirtualScrollingEnabled comes from Model.xafml Options

        foreach(var column in editor.Columns.OfType<DxGridColumnWrapper>()) {
            var model = column.DxGridDataColumnModel;
            model.Width = "170px";
            model.MinWidth = 120;
            model.FixedPosition = column.PropertyName switch {
                nameof(Order.Number) => GridColumnFixedPosition.Left,
                nameof(Order.Total) => GridColumnFixedPosition.Right,
                _ => GridColumnFixedPosition.None,
            };
            if(column.PropertyName.Contains('.'))
                model.FilterMenuButtonDisplayMode = GridFilterMenuButtonDisplayMode.Never; // nested columns: no filter menu

            var caption = column.Caption;
            var tooltip = $"{caption} — {column.PropertyName}";
            model.HeaderCaptionTemplate = ctx => builder => {
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "class", "xg-header");
                builder.AddAttribute(2, "title", tooltip);
                builder.AddContent(3, (column.PropertyName.Contains('.') ? "↗ " : "") + caption);
                builder.CloseElement();
            };
        }
    }
}
