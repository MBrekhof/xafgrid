using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Editors;
using XafGrid.Blazor.Server.Components.Grid;
using XafGrid.Module.BusinessObjects;

namespace XafGrid.Blazor.Server.Controllers.Demos;

/// <summary>D1 — expand an order row into a nested lines grid + customer card (DxGridModel.DetailRowTemplate).</summary>
public class MasterDetailController : ObjectViewController<ListView, Order> {
    public MasterDetailController() {
        TargetViewId = "Order_ListView_MasterDetail;Order_ListView_Showcase";
    }

    protected override void OnViewControlsCreated() {
        base.OnViewControlsCreated();
        if(View.Editor is not DxGridListEditor editor) return;

        var grid = editor.GridModel;
        grid.AutoCollapseDetailRow = true; // one open detail at a time
        grid.DetailRowTemplate = ctx => builder => {
            // ctx.DataItem is the entity in Client mode and a BlazorObjectRecord in server modes — GetObject handles both
            builder.OpenComponent<OrderDetailRow>(0);
            builder.AddAttribute(1, nameof(OrderDetailRow.Order), (Order)editor.GetObject(ctx.DataItem));
            builder.CloseComponent();
        };
    }
}
