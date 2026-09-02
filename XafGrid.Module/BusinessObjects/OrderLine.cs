using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using DevExpress.Persistent.BaseImpl.EF;

namespace XafGrid.Module.BusinessObjects;

public class OrderLine : BaseObject {
    [Browsable(false)]
    public virtual Guid? OrderId { get; set; }
    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; }

    [Browsable(false)]
    public virtual Guid? ProductId { get; set; }
    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; }

    public virtual int Quantity { get; set; }
    public virtual decimal UnitPrice { get; set; }
    /// <summary>Fraction 0..1</summary>
    public virtual decimal Discount { get; set; }
    public virtual decimal LineTotal { get; set; }

    public override void OnSaving() {
        base.OnSaving();
        LineTotal = Quantity * UnitPrice * (1 - Discount);
    }
}
