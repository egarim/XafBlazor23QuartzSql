using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using XafBlazor23Quartz.Module.BusinessObjects;

namespace XafBlazorQuartzHostedService.Module.BusinessObjects
{
    [DefaultClassOptions()]
    [ModelDefault("IsCloneable", "true")]
    public class ScheduleTask : BaseObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        public ScheduleTask(Session session)
            : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
           
        }



        string name;
        Sql sql;
        Connection connection;
        DateTime executeAt;
        TriggerType triggerType;
        bool enable;

        bool everyMonth;
        int month;
        bool everyDay;
        int day;
        bool everyHour;
        bool everyMinute;
        bool everySecond;
        string expressionDescription;
        private bool manualExpression;


        
        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string Name
        {
            get => name;
            set => SetPropertyValue(nameof(Name), ref name, value);
        }


        public TriggerType TriggerType
        {
            get => triggerType;
            set => SetPropertyValue(nameof(TriggerType), ref triggerType, value);
        }


        public DateTime ExecuteAt
        {
            get => executeAt;
            set => SetPropertyValue(nameof(ExecuteAt), ref executeAt, value);
        }
        private string expression;

        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string Expression
        {
            get
            {
                return expression;
            }
            set
            {
                SetPropertyValue(nameof(Expression), ref expression, value);
            }
        }
        [RuleRequiredField("RuleRequiredField Connection", DefaultContexts.Save, CustomMessageTemplate = "Connection is required to save")]
        public Connection Connection
        {
            get => connection;
            set => SetPropertyValue(nameof(Connection), ref connection, value);
        }
        [RuleRequiredField("RuleRequiredField Sql", DefaultContexts.Save, CustomMessageTemplate = "Sql is required to save")]
        public Sql Sql
        {
            get => sql;
            set => SetPropertyValue(nameof(Sql), ref sql, value);
        }
        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);
            if (propertyName == nameof(Expression) && newValue != null)
            {
                try
                {
                    this.ExpressionDescription = CronExpressionDescriptor.ExpressionDescriptor.GetDescription(newValue.ToString());
                }
                catch (Exception ex)
                {

                    throw new UserFriendlyException(ex);
                }
               
            }

        }




        [Size(SizeAttribute.Unlimited)]
        public string ExpressionDescription
        {
            get => expressionDescription;
            set => SetPropertyValue(nameof(ExpressionDescription), ref expressionDescription, value);
        }


       
        [Association("ScheduleBase-ScheduleExecutionDetails")]
        public XPCollection<Log> Logs
        {
            get
            {
                return GetCollection<Log>(nameof(Logs));
            }
        }
        
        public bool Enable
        {
            get => enable;
            set => SetPropertyValue(nameof(Enable), ref enable, value);
        }


    }
}