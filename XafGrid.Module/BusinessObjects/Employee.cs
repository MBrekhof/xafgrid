using System.ComponentModel;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace XafGrid.Module.BusinessObjects;

[DefaultClassOptions]
[DefaultProperty(nameof(Name))]
public class Employee : BaseObject {
    public virtual string Name { get; set; } = string.Empty;
    public virtual string Title { get; set; } = string.Empty;
}
