using DevExpress.ExpressApp.EFCore.Updating;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.ExpressApp.Design;
using DevExpress.ExpressApp.EFCore.DesignTime;

namespace XafGrid.Module.BusinessObjects;


[TypesInfoInitializer(typeof(DbContextTypesInfoInitializer<XafGridEFCoreDbContext>))]
public class XafGridEFCoreDbContext : DbContext {
    public XafGridEFCoreDbContext(DbContextOptions<XafGridEFCoreDbContext> options) : base(options) {
    }
    //public DbSet<ModuleInfo> ModulesInfo { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderLine> OrderLines { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseDeferredDeletion(this);
        modelBuilder.UseOptimisticLock();
        modelBuilder.SetOneToManyAssociationDeleteBehavior(DeleteBehavior.SetNull, DeleteBehavior.Cascade);
        modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
        modelBuilder.UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);

        modelBuilder.Entity<Product>().Property(p => p.UnitPrice).HasPrecision(18, 2);
        modelBuilder.Entity<Order>().Property(p => p.Total).HasPrecision(18, 2);
        modelBuilder.Entity<OrderLine>().Property(p => p.UnitPrice).HasPrecision(18, 2);
        modelBuilder.Entity<OrderLine>().Property(p => p.Discount).HasPrecision(5, 4);
        modelBuilder.Entity<OrderLine>().Property(p => p.LineTotal).HasPrecision(18, 2);
    }
}
