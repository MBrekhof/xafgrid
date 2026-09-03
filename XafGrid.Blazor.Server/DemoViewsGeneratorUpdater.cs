using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Model.Core;
using DevExpress.ExpressApp.Model.NodeGenerators;
using XafGrid.Module.BusinessObjects;

namespace XafGrid.Blazor.Server;

/// <summary>
/// One extra ListView per demo, created in code (dxdocs 113315). A ListView declared only in
/// Model.xafml gets no generated Columns node — the grid renders without data columns.
/// Navigation items for these views live in Model.xafml.
/// </summary>
public class DemoViewsGeneratorUpdater : ModelNodesGeneratorUpdater<ModelViewsNodesGenerator> {
    static readonly (string Id, Type Type, string Caption)[] Views = {
        ("Order_ListView_Showcase", typeof(Order), "Showcase"), // D1 + D2 on one grid — README hero image
        ("Order_ListView_MasterDetail", typeof(Order), "D1 Master-detail row"),
        ("Order_ListView_Cells", typeof(Order), "D2 Rich cells"),
        ("Order_ListView_Heatmap", typeof(Order), "D3 Heat-map"),
        ("Order_ListView_Groups", typeof(Order), "D4 Custom grouping"),
        ("Order_ListView_Summary", typeof(Order), "D5 Custom summaries"),
        ("Order_ListView_Toolbar", typeof(Order), "D6 In-grid toolbar"),
        ("Order_ListView_Unbound", typeof(Order), "D7 Unbound columns"),
        ("Product_ListView_Reorder", typeof(Product), "D8 Drag to reorder"),
        ("Order_ListView_Presets", typeof(Order), "D9 Layout presets"),
        ("Order_ListView_Columns", typeof(Order), "D10 Wide grid"),
    };

    public override void UpdateNode(ModelNode viewsNode) {
        foreach(var (id, type, caption) in Views) {
            var listView = viewsNode.AddNode<IModelListView>(id);
            listView.ModelClass = viewsNode.Application.BOModel.GetClass(type);
            listView.Caption = caption;
        }

        // D8: DxGrid refuses between-row drops while a column sort is active, so the grid itself must stay
        // unsorted: drop the generator's default sort on the DefaultProperty column (Name) and order the
        // data through the collection source (model Sorting node) instead.
        var reorder = (IModelListView)viewsNode.GetNode("Product_ListView_Reorder");
        reorder.Columns[nameof(Product.Name)].SortIndex = -1;
        var sort = reorder.Sorting.AddNode<IModelSortProperty>(nameof(Product.SortOrder));
        sort.PropertyName = nameof(Product.SortOrder);
        sort.Direction = DevExpress.Xpo.DB.SortingDirection.Ascending;

        // D10: nested-property columns to make the grid wider than the viewport
        var wide = (IModelListView)viewsNode.GetNode("Order_ListView_Columns");
        foreach(var path in new[] { "Customer.Country", "Customer.City", "Customer.Rating", "Customer.Since", "Employee.Title" }) {
            var column = wide.Columns.AddNode<IModelColumn>(path);
            column.PropertyName = path;
        }
    }
}
