using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Model.Core;
using DevExpress.ExpressApp.Model.DomainLogics;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.Updating;
using DevExpress.Persistent.BaseImpl;
using System.ComponentModel;
using XafBlazor23Quartz.Module.BusinessObjects;
using XafBlazorQuartzHostedService.Module.Blazor.Quartz;

namespace XafBlazor23Quartz.Blazor.Server;

[ToolboxItemFilter("Xaf.Platform.Blazor")]
// For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.ModuleBase.
public sealed class XafBlazor23QuartzBlazorModule : ModuleBase {
    //private void Application_CreateCustomModelDifferenceStore(object sender, CreateCustomModelDifferenceStoreEventArgs e) {
    //    e.Store = new ModelDifferenceDbStore((XafApplication)sender, typeof(ModelDifference), true, "Blazor");
    //    e.Handled = true;
    //}
    private void Application_CreateCustomUserModelDifferenceStore(object sender, CreateCustomModelDifferenceStoreEventArgs e) {
        e.Store = new ModelDifferenceDbStore((XafApplication)sender, typeof(ModelDifference), false, "Blazor");
        e.Handled = true;
    }
    public XafBlazor23QuartzBlazorModule() {
    }
    public override IEnumerable<ModuleUpdater> GetModuleUpdaters(IObjectSpace objectSpace, Version versionFromDB) {
        return ModuleUpdater.EmptyModuleUpdaters;
    }
    public override void Setup(XafApplication application) {
        base.Setup(application);
        // Uncomment this code to store the shared model differences (administrator settings in Model.XAFML) in the database.
        // For more information, refer to the following topic: https://docs.devexpress.com/eXpressAppFramework/113698/
        //application.CreateCustomModelDifferenceStore += Application_CreateCustomModelDifferenceStore;
        application.CreateCustomUserModelDifferenceStore += Application_CreateCustomUserModelDifferenceStore;
        application.SetupComplete += Application_SetupComplete;
    }
    private void Application_SetupComplete(object sender, EventArgs e)
    {
        Application.ObjectSpaceCreated += Application_ObjectSpaceCreated;
    }
    private void Application_ObjectSpaceCreated(object sender, ObjectSpaceCreatedEventArgs e)
    {
        var nonPersistentObjectSpace = e.ObjectSpace as NonPersistentObjectSpace;
        if (nonPersistentObjectSpace != null)
        {
            nonPersistentObjectSpace.ObjectsGetting += ObjectSpace_ObjectsGetting;
        }
    }
    private void ObjectSpace_ObjectsGetting(Object sender, ObjectsGettingEventArgs e)
    {
        if (e.ObjectType == typeof(JobStatus))
        {
            var Jobs=this.Application.ServiceProvider.GetService<XafQuartzHostedService>().GetJobStatus(default).Result;
            BindingList<JobStatus> objects = new BindingList<JobStatus>();
            foreach (var job in Jobs)
            {
                objects.Add(job);
            }
           
            e.Objects = objects;
        }
    }
}
