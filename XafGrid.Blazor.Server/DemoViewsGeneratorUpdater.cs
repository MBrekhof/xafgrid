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
        ("Order_ListView_MasterDetail", typeof(Order), "D1 Master-detail row"),
        ("Order_ListView_Cells", typeof(Order), "D2 Rich cells"),
        ("Order_ListView_Heatmap", typeof(Order), "D3 Heat-map"),
    };

    public override void UpdateNode(ModelNode viewsNode) {
        foreach(var (id, type, caption) in Views) {
            var listView = viewsNode.AddNode<IModelListView>(id);
            listView.ModelClass = viewsNode.Application.BOModel.GetClass(type);
            listView.Caption = caption;
        }
    }
}
