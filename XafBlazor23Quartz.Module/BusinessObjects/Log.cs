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

        ScheduleTask scheduleBase;
        string log;
        DateTime date;

        public DateTime Date
        {
            get => date;
            set => SetPropertyValue(nameof(Date), ref date, value);
        }

        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string LogText
        {
            get => log;
            set => SetPropertyValue(nameof(LogText), ref log, value);
        }
        
        [Association("ScheduleBase-ScheduleExecutionDetails")]
        public ScheduleTask ScheduleBase
        {
            get => scheduleBase;
            set => SetPropertyValue(nameof(ScheduleBase), ref scheduleBase, value);
        }
    }
}