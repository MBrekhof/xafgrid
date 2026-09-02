using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor;

namespace XafGrid.Blazor.Server;

public class XafGridBlazorApplication : BlazorApplication {
    public XafGridBlazorApplication() {
        ApplicationName = "XafGrid";
        CheckCompatibilityType = CheckCompatibilityType.DatabaseSchema;
        // ponytail: demo app on a throw-away SQLite file — always create/update the schema, no debugger gate
        DatabaseUpdateMode = DatabaseUpdateMode.UpdateDatabaseAlways;
        DatabaseVersionMismatch += (_, e) => { e.Updater.Update(); e.Handled = true; };
    }
}
