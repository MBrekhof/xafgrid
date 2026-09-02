using DevExpress.Blazor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Editors;
using Microsoft.AspNetCore.Components;
using XafGrid.Module.BusinessObjects;

namespace XafGrid.Blazor.Server.Controllers.Demos;

/// <summary>D8 — drag rows to reorder products; the new order is persisted in Product.SortOrder (AllowDragRows + ItemsDropped).</summary>
public class ReorderController : ObjectViewController<ListView, Product> {
    public ReorderController() {
        TargetViewId = "Product_ListView_Reorder";
    }

    protected override void OnViewControlsCreated() {
        base.OnViewControlsCreated();
        if(View.Editor is not DxGridListEditor editor) return;
        var grid = editor.GridModel;

        grid.AllowDragRows = true; // declared on DxGridBaseModel, not DxGridModel — dxdocs' member list hides it
        grid.AllowedDropTarget = GridAllowedDropTarget.Internal;
        grid.DragHintTextTemplate = ctx => builder => builder.AddContent(0, $"Move {ctx.DataItems.Count} product(s)");
        grid.ItemsDropped = EventCallback.Factory.Create<GridItemsDroppedEventArgs>(this, e => OnItemsDropped(editor, e));
    }

    void OnItemsDropped(DxGridListEditor editor, GridItemsDroppedEventArgs e) {
        var ordered = ObjectSpace.GetObjects<Product>().OrderBy(p => p.SortOrder).ToList();
        var dropped = e.DroppedItems.Select(i => (Product)editor.GetObject(i)).ToList();
        foreach(var p in dropped) ordered.Remove(p);

        var target = e.TargetItem is null ? null : (Product)editor.GetObject(e.TargetItem);
        var index = target is null ? ordered.Count : ordered.IndexOf(target) + (e.DropPosition == GridItemDropPosition.After ? 1 : 0);
        ordered.InsertRange(index, dropped);

        for(int i = 0; i < ordered.Count; i++) ordered[i].SortOrder = i;
        ObjectSpace.CommitChanges();
        View.Refresh(); // reload so the model's SortOrder sorting re-applies
    }
}
