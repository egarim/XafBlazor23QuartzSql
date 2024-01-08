using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Blazor;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using XafBlazorQuartzHostedService.Module.Blazor.Quartz;
using XafBlazorQuartzHostedService.Module.BusinessObjects;


namespace XafBlazorQuartz2.Module.Blazor.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class QuartzController : ViewController
    {
        SimpleAction RestartScheduler;

        public QuartzController()
        {
            InitializeComponent();


            RestartScheduler = new SimpleAction(this, nameof(RestartScheduler), PredefinedCategory.View) { Caption = "Restart Scheduler" };
            RestartScheduler.Execute += RestartScheduler_Execute;


            SimpleAction StartScheduler = new SimpleAction(this, nameof(StartScheduler), PredefinedCategory.View) { Caption = "Start Scheduler" };
            StartScheduler.Execute += StartScheduler_Execute;


            SimpleAction StopScheduler = new SimpleAction(this, nameof(StopScheduler), PredefinedCategory.View) { Caption = "Stop Scheduler" };
            StopScheduler.Execute += StopScheduler_Execute;



        }
        private async void RestartScheduler_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            IServiceProvider serviceProvider = ((BlazorApplication)Application).ServiceProvider;
            var Service = serviceProvider.GetService<XafQuartzHostedService>();
         
            await Service.StopAsync(new System.Threading.CancellationToken());
            await Service.StartAsync(new System.Threading.CancellationToken());
        }

        private async void StopScheduler_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            IServiceProvider serviceProvider = ((BlazorApplication)Application).ServiceProvider;
            var Service = serviceProvider.GetService<XafQuartzHostedService>();
            if(Service.Started)
                await Service.StopAsync(new System.Threading.CancellationToken());
        }
        private async void StartScheduler_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            IServiceProvider serviceProvider = ((BlazorApplication)Application).ServiceProvider;
            var Service = serviceProvider.GetService<XafQuartzHostedService>();
            if (!Service.Started)
                await Service.StartAsync(new System.Threading.CancellationToken());


        }


        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }
    }
}
