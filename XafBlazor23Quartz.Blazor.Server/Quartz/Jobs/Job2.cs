
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using Microsoft.Extensions.Logging;
using Quartz;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using XafBlazorQuartzHostedService.Module.BusinessObjects;

namespace XafBlazor23Quartz.Blazor.Server.Quartz.Jobs
{
    [DisallowConcurrentExecution]
    public class Job2 : XafJob,IJob
    {
        public Job2(ILogger<XafJob> logger, IServiceProvider provider) : base(logger, provider)
        {

        }

        public override Task Execute(IJobExecutionContext context)
        {
            return base.Execute(context);
        }
    }
}




