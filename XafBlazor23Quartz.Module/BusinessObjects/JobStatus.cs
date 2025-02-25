using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XafBlazor23Quartz.Module.BusinessObjects
{
    [NavigationItem("Quartz")]
    [DomainComponent, DefaultClassOptions]
    public class JobStatus: NonPersistentBaseObject
    {
        public string JobName { get; set; }
        public string JobGroup { get; set; }
        [ModelDefault("DisplayFormat", "G")]
        public DateTime? LastFireTime { get; set; }
        [ModelDefault("DisplayFormat", "G")]
        public DateTime? NextFireTime { get; set; }
        public string TriggerState { get; set; }
        public bool IsRunning => TriggerState == "RUNNING";
    }
}
