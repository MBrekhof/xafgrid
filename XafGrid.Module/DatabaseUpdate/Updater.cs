using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Updating;
using XafGrid.Module.BusinessObjects;

namespace XafGrid.Module.DatabaseUpdate;

public class Updater : ModuleUpdater {
    public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
        base(objectSpace, currentDBVersion) {
    }
    public override void UpdateDatabaseAfterUpdateSchema() {
        base.UpdateDatabaseAfterUpdateSchema();
        if(ObjectSpace.GetObjectsCount(typeof(Customer), null) > 0) return;
        DemoData.Seed(ObjectSpace);
        ObjectSpace.CommitChanges();
    }
}
