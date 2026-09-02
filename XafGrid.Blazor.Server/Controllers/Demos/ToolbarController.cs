using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Editors;
using XafGrid.Blazor.Server.Components.Grid;
using XafGrid.Module.BusinessObjects;

namespace XafGrid.Blazor.Server.Controllers.Demos;

/// <summary>
/// D6 — a DxToolbar inside the grid (ToolbarTemplate) driving the IGrid API: group/expand/collapse,
/// auto-fit, filter builder, clear filter, XLSX/CSV export (XAF only ships PDF); EmptyDataAreaTemplate.
/// </summary>
public class ToolbarController : ObjectViewController<ListView, Order> {
    public ToolbarController() {
        TargetViewId = "Order_ListView_Toolbar";
    }

    protected override void OnViewControlsCreated() {
        base.OnViewControlsCreated();
        if(View.Editor is not DxGridListEditor editor) return;
        var grid = editor.GridModel;

        grid.ToolbarTemplate = ctx => builder => {
            builder.OpenComponent<DemoToolbar>(0);
            builder.AddAttribute(1, nameof(DemoToolbar.Grid), ctx.Grid);
            builder.CloseComponent();
        };
        grid.EmptyDataAreaTemplate = ctx => builder => {
            builder.OpenComponent<EmptyArea>(0);
            builder.AddAttribute(1, nameof(EmptyArea.Grid), ctx.Grid);
            builder.CloseComponent();
        };
    }
}
