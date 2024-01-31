using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.ClientServer;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Xpo;
using XafBlazor23Quartz.Blazor.Server.Services;

namespace XafBlazor23Quartz.Blazor.Server;

public class XafBlazor23QuartzBlazorApplication : BlazorApplication {
    public XafBlazor23QuartzBlazorApplication() {
        ApplicationName = "XafBlazor23Quartz";
        CheckCompatibilityType = DevExpress.ExpressApp.CheckCompatibilityType.DatabaseSchema;
        DatabaseVersionMismatch += XafBlazor23QuartzBlazorApplication_DatabaseVersionMismatch;
    }
    protected override void OnSetupStarted() {
        base.OnSetupStarted();
#if DEBUG
        if(System.Diagnostics.Debugger.IsAttached && CheckCompatibilityType == CheckCompatibilityType.DatabaseSchema) {
            DatabaseUpdateMode = DatabaseUpdateMode.UpdateDatabaseAlways;
        }
#endif
    }
    private void XafBlazor23QuartzBlazorApplication_DatabaseVersionMismatch(object sender, DatabaseVersionMismatchEventArgs e)
    {

        e.Updater.Update();
        e.Handled = true;
    }
}
