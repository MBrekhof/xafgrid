using DevExpress.Blazor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Editors;
using DevExpress.ExpressApp.Blazor.Editors.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using XafGrid.Module.BusinessObjects;

namespace XafGrid.Blazor.Server.Controllers.Demos;

/// <summary>
/// D7 — columns that are not entity properties: computed in code (UnboundColumnData), computed by
/// criteria expression (UnboundExpression) and an action button column (CellDisplayTemplate).
/// </summary>
public class UnboundController : ObjectViewController<ListView, Order> {
    public UnboundController() {
        TargetViewId = "Order_ListView_Unbound";
    }

    protected override void OnViewControlsCreated() {
        base.OnViewControlsCreated();
        if(View.Editor is not DxGridListEditor editor) return;

        editor.AddColumnModel(new DxGridDataColumnModel {
            FieldName = "LineCount", Caption = "Lines", UnboundType = GridUnboundColumnType.Integer, Width = "80px",
        });
        editor.AddColumnModel(new DxGridDataColumnModel {
            FieldName = "DaysOpen", Caption = "Days open", UnboundType = GridUnboundColumnType.Integer, Width = "100px",
        });
        editor.AddColumnModel(new DxGridDataColumnModel {
            FieldName = "Margin", Caption = "Margin (20%)", UnboundType = GridUnboundColumnType.Decimal,
            UnboundExpression = "[Total] * 0.2", DisplayFormat = "c", Width = "120px",
        });

        var actions = new DxGridDataColumnModel {
            FieldName = "Actions", Caption = " ", UnboundType = GridUnboundColumnType.String, Width = "90px",
        };
        actions.CellDisplayTemplate = ctx => builder => {
            var order = (Order)editor.GetObject(ctx.DataItem);
            // the wrapper swallows the click so XAF's row click does not open the DetailView
            builder.OpenElement(0, "span");
            builder.AddEventStopPropagationAttribute(1, "onclick", true);
            builder.OpenComponent<DxButton>(2);
            builder.AddAttribute(3, nameof(DxButton.Text), "Ship");
            builder.AddAttribute(4, nameof(DxButton.RenderStyle), ButtonRenderStyle.Secondary);
            builder.AddAttribute(5, nameof(DxButton.SizeMode), SizeMode.Small);
            builder.AddAttribute(6, nameof(DxButton.Enabled), order.Status is OrderStatus.New or OrderStatus.Confirmed);
            builder.AddAttribute(7, nameof(DxButton.Click), EventCallback.Factory.Create<MouseEventArgs>(this, () => Ship(order)));
            builder.CloseComponent();
            builder.CloseElement();
        };
        editor.AddColumnModel(actions);

        editor.GridModel.UnboundColumnData += e => {
            if(editor.GetObject(e.DataItem) is not Order o) return;
            e.Value = e.FieldName switch {
                "LineCount" => o.Lines.Count,
                "DaysOpen" => ((o.ShippedDate ?? DateTime.Today) - o.OrderDate.Date).Days,
                _ => e.Value,
            };
        };
    }

    void Ship(Order order) {
        order.Status = OrderStatus.Shipped;
        order.ShippedDate = DateTime.Today;
        ObjectSpace.CommitChanges();
        Application.ShowViewStrategy.ShowMessage($"{order.Number} shipped");
    }
}
