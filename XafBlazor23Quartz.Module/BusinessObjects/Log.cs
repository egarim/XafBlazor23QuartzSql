using System;
using System.Linq;
using System.Text;
using DevExpress.Xpo;
using DevExpress.ExpressApp;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using System.Collections.Generic;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;

namespace XafBlazorQuartzHostedService.Module.BusinessObjects
{
    public enum Result
    {
        Success=0,Error=1
    }
    [DefaultClassOptions()]
    public class Log : BaseObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        public Log(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
        }

        Result result;
        ScheduleTask scheduleTask;
        string logText;
        DateTime date;
        [Custom("DisplayFormat", "dd/MM/yyyy - hh:mm:ss")]
        [ModelDefault("AllowEdit", "false")]
        public DateTime Date
        {
            get => date;
            set => SetPropertyValue(nameof(Date), ref date, value);
        }
        [ModelDefault("AllowEdit", "false")]
        [Size(SizeAttribute.Unlimited)]
        public string LogText
        {
            get => logText;
            set => SetPropertyValue(nameof(LogText), ref logText, value);
        }
        [ModelDefault("AllowEdit","false")]
        public Result Result
        {
            get => result;
            set => SetPropertyValue(nameof(Result), ref result, value);
        }
        [Association("ScheduleBase-ScheduleExecutionDetails")]
        public ScheduleTask ScheduleTask
        {
            get => scheduleTask;
            set => SetPropertyValue(nameof(ScheduleTask), ref scheduleTask, value);
        }
    }
}