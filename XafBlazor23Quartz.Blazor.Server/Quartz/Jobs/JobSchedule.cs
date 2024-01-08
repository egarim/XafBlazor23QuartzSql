
using System;
using XafBlazor23Quartz.Module.BusinessObjects;

namespace XafBlazor23Quartz.Blazor.Server.Quartz.Jobs
{
    public class JobSchedule
    {
        public JobSchedule(Type jobType, string cronExpression, TriggerType triggerType)
        {
            JobType = jobType;
            CronExpression = cronExpression;
            triggerType = triggerType;
        }
       
        public Type JobType { get; }
        public string CronExpression { get; }
        public DateTime StartTime { get; set; }
        public TriggerType TriggerType { get; set; }
    }
}




