using System.ComponentModel;
using DevExpress.Persistent.BaseImpl.EF;

namespace XafGrid.Module.BusinessObjects;

/// <summary>A named DxGrid layout (GridPersistentLayout as JSON) for one ListView — XAF itself keeps only one layout per view.</summary>
[DefaultProperty(nameof(Name))]
public class GridLayoutPreset : BaseObject {
    public virtual string ViewId { get; set; } = string.Empty;
    public virtual string Name { get; set; } = string.Empty;
    public virtual string LayoutJson { get; set; } = string.Empty;
}
