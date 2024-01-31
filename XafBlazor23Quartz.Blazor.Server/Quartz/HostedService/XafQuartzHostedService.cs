using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XafBlazor23Quartz.Blazor.Server.Quartz.Jobs;
using XafBlazor23Quartz.Blazor.Server.Quartz.ObjectSpaceService;
using XafBlazor23Quartz.Module.BusinessObjects;
using XafBlazorQuartzHostedService.Module.Blazor.Quartz;
using XafBlazorQuartzHostedService.Module.BusinessObjects;

namespace XafBlazorQuartzHostedService.Module.Blazor.Quartz
{
    public class XafQuartzHostedService : IHostedService
    {

        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IJobFactory _jobFactory;
        private readonly IQuartzObjectSpaceService _objectSpaceService;


        public XafQuartzHostedService(
            ISchedulerFactory schedulerFactory,
            IJobFactory jobFactory,
           IQuartzObjectSpaceService objectSpaceService)
        {
            _schedulerFactory = schedulerFactory;
            _objectSpaceService = objectSpaceService;
            _jobFactory = jobFactory;

        }
        public IScheduler Scheduler { get; set; }

        public bool Started { get; set; }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
             StringBuilder Log=new StringBuilder();
            Scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            Scheduler.JobFactory = _jobFactory;
            Log.AppendLine("Scheduler Started");
            IObjectSpace Os;
            IEnumerable<ScheduleTask> Schedules = null;
            try
            {
                Os = _objectSpaceService.GetObjectSpace();
                Log.AppendLine("ObjectSpace Created");
                Schedules = Os.CreateCollection(typeof(ScheduleTask)).Cast<ScheduleTask>();
                //HACK get the count to evaluate the property
                var Count = Schedules.Count();
            }
            catch (Exception ex) when (ex is DevExpress.Xpo.DB.Exceptions.SchemaCorrectionNeededException || ex is DevExpress.Xpo.DB.Exceptions.UnableToOpenDatabaseException)
            {

                return;
            }


            foreach (var item in Schedules)
            {



                if (!item.Enable)
                    continue;


                JobSchedule jobSchedule = null;
                jobSchedule = new JobSchedule(
                 jobType: typeof(XafJob),
                 cronExpression: item.Expression,
                 triggerType: item.TriggerType);

             

                IDictionary<string, object> map = new Dictionary<string, object>()
                {
                    //hack to avoid threading problems its better to pass the connection string and then create an object space provider inside of the job
                    {"Oid",item.Oid },
                    {"cnx",_objectSpaceService.GetConnectionString() },

                };
                var DataMap = new JobDataMap(map);

                var job = CreateJob(jobSchedule, DataMap, item.Oid);
                var trigger = CreateTrigger(jobSchedule, item.Oid);



                await Scheduler.ScheduleJob(job, trigger, cancellationToken);
                Log.AppendLine($"Job {item.Name} schedule ({item.ExpressionDescription}) ");

            }
            var log=Os.CreateObject<SchedulerLog>();
            log.Date = DateTime.UtcNow;
            log.Log = Log.ToString();   
            Os.CommitChanges();
            Started = true;

            await Scheduler.Start(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Scheduler?.Shutdown(cancellationToken);
            Started = false;
        }

        private static IJobDetail CreateJob(JobSchedule schedule, JobDataMap DataMap,Guid Identity)
        {
            var jobType = schedule.JobType;
            return JobBuilder
                .Create(jobType)
                .WithIdentity(Identity.ToString())
                .SetJobData(DataMap)
                .WithDescription(jobType.Name)
                .Build();
        }


        private static ITrigger CreateTrigger(JobSchedule schedule,Guid Identity)
        {
            switch (schedule.TriggerType)
            {
                case TriggerType.CronExpression:
                    return TriggerBuilder
                   .Create()
                   .WithIdentity(Identity.ToString())
                   .WithCronSchedule(schedule.CronExpression)
                   .WithDescription(schedule.CronExpression)
                   .Build();

                case TriggerType.DateTrigger:
                    return TriggerBuilder
                   .Create()
                   .WithIdentity(Identity.ToString())
                   .StartAt(schedule.StartTime)
                   .WithDescription(schedule.StartTime.ToString())
                   .Build();


            }
            return null;

        }
    }
}
