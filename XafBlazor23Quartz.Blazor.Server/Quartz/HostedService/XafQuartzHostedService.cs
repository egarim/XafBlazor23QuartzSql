using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl.Matchers;
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
        public async Task<bool> IsJobRunning(string jobId, CancellationToken cancellationToken = default)
        {
            if (Scheduler == null)
                return false;

            var executingJobs = await Scheduler.GetCurrentlyExecutingJobs(cancellationToken);
            return executingJobs.Any(job => job.JobDetail.Key.Name == jobId);
        }
        public async Task<List<JobStatus>> GetJobStatus(CancellationToken cancellationToken = default)
        {
            var result = new List<JobStatus>();

            if (Scheduler != null)
            {
                var executingJobs = await Scheduler.GetCurrentlyExecutingJobs(cancellationToken);
                var jobGroups = await Scheduler.GetJobGroupNames(cancellationToken);

                foreach (var group in jobGroups)
                {
                    var groupMatcher = GroupMatcher<JobKey>.GroupEquals(group);
                    var jobKeys = await Scheduler.GetJobKeys(groupMatcher, cancellationToken);

                    foreach (var jobKey in jobKeys)
                    {
                        var detail = await Scheduler.GetJobDetail(jobKey, cancellationToken);
                        var triggers = await Scheduler.GetTriggersOfJob(jobKey, cancellationToken);
                        var trigger = triggers.FirstOrDefault();
                        var state = await Scheduler.GetTriggerState(trigger.Key, cancellationToken);

                        // Check if this job is currently executing
                        var isRunning = executingJobs.Any(j => j.JobDetail.Key.Equals(jobKey));

                        result.Add(new JobStatus
                        {
                            JobName = jobKey.Name,
                            JobGroup = jobKey.Group,
                            LastFireTime = trigger?.GetPreviousFireTimeUtc()?.LocalDateTime,
                            NextFireTime = trigger?.GetNextFireTimeUtc()?.LocalDateTime,
                            TriggerState = isRunning ? "RUNNING" : state.ToString()
                        });
                    }
                }
            }

            return result;
        }
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
