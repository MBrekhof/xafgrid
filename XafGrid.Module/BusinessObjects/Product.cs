using System.ComponentModel;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace XafGrid.Module.BusinessObjects;

[DefaultClassOptions]
[DefaultProperty(nameof(Name))]
public class Product : BaseObject {
    public virtual string Name { get; set; } = string.Empty;
    public virtual string Category { get; set; } = string.Empty;
    public virtual decimal UnitPrice { get; set; }
    public virtual bool Discontinued { get; set; }
    /// <summary>Persisted display order — the drag-to-reorder demo writes this.</summary>
    public virtual int SortOrder { get; set; }
}
