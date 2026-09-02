using DevExpress.Blazor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Editors;
using DevExpress.ExpressApp.Blazor.Editors.Models;
using XafGrid.Blazor.Server.Components.Grid;
using XafGrid.Module.BusinessObjects;

namespace XafGrid.Blazor.Server.Controllers.Demos;

/// <summary>
/// D4 — group OrderDate into age buckets (CustomGroup + CustomSort), label them
/// (CustomizeGroupValueDisplayText) and render count / sum / bar in the group row (DataColumnGroupRowTemplate).
/// </summary>
public class GroupsController : ObjectViewController<ListView, Order> {
    public GroupsController() {
        TargetViewId = "Order_ListView_Groups";
    }

    protected override void OnViewControlsCreated() {
        base.OnViewControlsCreated();
        if(View.Editor is not DxGridListEditor editor) return;
        var grid = editor.GridModel;

        var dateColumn = editor.Columns.OfType<DxGridColumnWrapper>().First(c => c.PropertyName == nameof(Order.OrderDate)).DxGridDataColumnModel;
        dateColumn.GroupIndex = 0;
        dateColumn.GroupInterval = GridColumnGroupInterval.Custom;
        dateColumn.SortMode = GridColumnSortMode.Custom;

        grid.CustomGroup += e => {
            if(e.FieldName != nameof(Order.OrderDate)) return;
            e.SameGroup = Bucket(e.Value1) == Bucket(e.Value2);
            e.Handled = true;
        };
        grid.CustomSort += e => {
            if(e.FieldName != nameof(Order.OrderDate)) return;
            e.Result = Bucket(e.Value1).CompareTo(Bucket(e.Value2));
            e.Handled = true;
        };
        grid.CustomizeGroupValueDisplayText += e => {
            if(e.FieldName == nameof(Order.OrderDate)) e.DisplayText = Label(Bucket(e.Value));
        };

        // group summaries through XAF's wrapper collection (DxGridSummary), not the RenderFragment
        editor.GridSummary.GroupSummary.Add(new DxGridSummaryItemWrapper(new DxGridSummaryItemModel {
            FieldName = nameof(Order.Number), SummaryType = GridSummaryItemType.Count,
        }));
        editor.GridSummary.GroupSummary.Add(new DxGridSummaryItemWrapper(new DxGridSummaryItemModel {
            FieldName = nameof(Order.Total), SummaryType = GridSummaryItemType.Sum, ValueDisplayFormat = "c0",
        }));

        grid.DataColumnGroupRowTemplate = ctx => builder => {
            builder.OpenComponent<GroupRowContent>(0);
            builder.AddAttribute(1, nameof(GroupRowContent.Context), ctx);
            builder.CloseComponent();
        };
        grid.AutoExpandAllGroupRows = true;
    }

    static int Bucket(object value) => value is DateTime d
        ? (DateTime.Today - d.Date).TotalDays switch { <= 7 => 0, <= 30 => 1, <= 90 => 2, <= 365 => 3, _ => 4 }
        : 5;

    static string Label(int bucket) => bucket switch {
        0 => "This week", 1 => "This month", 2 => "This quarter", 3 => "This year", 4 => "Older", _ => "No date",
    };
}
