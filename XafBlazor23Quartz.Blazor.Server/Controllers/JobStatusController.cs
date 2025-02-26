using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor;
using XafBlazor23Quartz.Module.BusinessObjects;

namespace XafBlazor23Quartz.Blazor.Server.Controllers
{
    public class JobStatusController : ViewController
    {
        public JobStatusController() : base()
        {
            // Target required Views (use the TargetXXX properties) and create their Actions.
            this.TargetObjectType = typeof(JobStatus);
            this.TargetViewType = ViewType.ListView;
        }
        public bool Refresh { get; set; }
        protected override void OnActivated()
        {
            Refresh = true;
            base.OnActivated();
            var blazorApplication = Application as BlazorApplication;
            Task.Run(async () => {
                await blazorApplication.InvokeAsync(async () => {
                    while (Refresh)
                    {
                        await Task.Delay(2000);
                        View.ObjectSpace.Refresh();
                        Application.ShowViewStrategy.ShowMessage("Data refreshed");
                    }
                   
                });
            });
            // Perform various tasks depending on the target View.
        }
        protected override void OnDeactivated()
        {
            Refresh = false;
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
        }
    }
}
