using System.Collections.ObjectModel;
using System.ComponentModel;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace XafGrid.Module.BusinessObjects;

[DefaultClassOptions]
[DefaultProperty(nameof(Name))]
public class Customer : BaseObject {
    public virtual string Name { get; set; } = string.Empty;
    public virtual string Country { get; set; } = string.Empty;
    public virtual string City { get; set; } = string.Empty;
    /// <summary>1..5 — rendered as stars in the cell-template demo.</summary>
    public virtual int Rating { get; set; }
    public virtual DateTime Since { get; set; }
    public virtual IList<Order> Orders { get; set; } = new ObservableCollection<Order>();
}
