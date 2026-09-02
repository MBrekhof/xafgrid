using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace XafGrid.Module.BusinessObjects;

public enum OrderStatus { New, Confirmed, Shipped, Delivered, Cancelled }

[DefaultClassOptions]
[DefaultProperty(nameof(Number))]
public class Order : BaseObject {
    public virtual string Number { get; set; } = string.Empty;

    [Browsable(false)]
    public virtual Guid? CustomerId { get; set; }
    [ForeignKey(nameof(CustomerId))]
    public virtual Customer Customer { get; set; }

    [Browsable(false)]
    public virtual Guid? EmployeeId { get; set; }
    [ForeignKey(nameof(EmployeeId))]
    public virtual Employee Employee { get; set; }

    public virtual DateTime OrderDate { get; set; }
    public virtual DateTime? ShippedDate { get; set; }
    public virtual OrderStatus Status { get; set; }
    /// <summary>Persisted (not [NotMapped]) so server-mode sorting/grouping/summaries work on it.</summary>
    public virtual decimal Total { get; set; }

    [Aggregated]
    public virtual IList<OrderLine> Lines { get; set; } = new ObservableCollection<OrderLine>();

    public override void OnSaving() {
        base.OnSaving();
        // ponytail: recompute from the raw fields, not LineTotal — OnSaving order between parent and lines is not guaranteed
        Total = Lines.Sum(l => l.Quantity * l.UnitPrice * (1 - l.Discount));
    }
}
