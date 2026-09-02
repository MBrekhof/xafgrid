using System.Text.Json;
using DevExpress.Blazor;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Editors;
using Microsoft.AspNetCore.Components;
using XafGrid.Blazor.Server.Components.Grid;
using XafGrid.Module.BusinessObjects;

namespace XafGrid.Blazor.Server.Controllers.Demos;

/// <summary>
/// D9 — named layout presets stored in the database (GridLayoutPreset) via IGrid.SaveLayout/LoadLayout;
/// XAF's own persistence keeps a single layout per view.
/// </summary>
public class PresetsController : ObjectViewController<ListView, Order> {
    public PresetsController() {
        TargetViewId = "Order_ListView_Presets";
    }

    protected override void OnViewControlsCreated() {
        base.OnViewControlsCreated();
        if(View.Editor is not DxGridListEditor editor) return;

        editor.GridModel.ToolbarTemplate = ctx => builder => {
            builder.OpenComponent<PresetToolbar>(0);
            builder.AddAttribute(1, nameof(PresetToolbar.Grid), ctx.Grid);
            builder.AddAttribute(2, nameof(PresetToolbar.Presets), Presets().Select(p => p.Name).OrderBy(n => n).ToList());
            builder.AddAttribute(3, nameof(PresetToolbar.Save), EventCallback.Factory.Create<string>(this, name => Save(ctx.Grid, name)));
            builder.AddAttribute(4, nameof(PresetToolbar.Load), EventCallback.Factory.Create<string>(this, name => Load(ctx.Grid, name)));
            builder.AddAttribute(5, nameof(PresetToolbar.Delete), EventCallback.Factory.Create<string>(this, Delete));
            builder.CloseComponent();
        };
    }

    IList<GridLayoutPreset> Presets() =>
        ObjectSpace.GetObjects<GridLayoutPreset>(CriteriaOperator.Parse("ViewId = ?", View.Id));

    GridLayoutPreset Find(string name) => Presets().FirstOrDefault(p => p.Name == name);

    void Save(IGrid grid, string name) {
        var preset = Find(name) ?? ObjectSpace.CreateObject<GridLayoutPreset>();
        preset.ViewId = View.Id;
        preset.Name = name;
        preset.LayoutJson = JsonSerializer.Serialize(grid.SaveLayout());
        ObjectSpace.CommitChanges();
    }

    void Load(IGrid grid, string name) {
        if(Find(name) is { } preset)
            grid.LoadLayout(JsonSerializer.Deserialize<GridPersistentLayout>(preset.LayoutJson));
    }

    void Delete(string name) {
        if(Find(name) is { } preset) {
            ObjectSpace.Delete(preset);
            ObjectSpace.CommitChanges();
        }
    }
}
