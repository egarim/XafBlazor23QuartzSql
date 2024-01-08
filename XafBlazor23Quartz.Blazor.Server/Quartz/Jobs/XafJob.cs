
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using Microsoft.Extensions.Logging;
using Quartz;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using XafBlazor23Quartz.Module.BusinessObjects;
using XafBlazorQuartzHostedService.Module.BusinessObjects;

namespace XafBlazor23Quartz.Blazor.Server.Quartz.Jobs
{
    //HACK we need concurrent executions
    //[DisallowConcurrentExecution]
    public class XafJob : IJob
    {

        protected readonly ILogger<XafJob> _logger;
        protected readonly IServiceProvider _provider;
        protected string cnx;
        public XafJob(ILogger<XafJob> logger, IServiceProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }

        public virtual Task Execute(IJobExecutionContext context)
        {
            


            var dataMap = context.JobDetail.JobDataMap;
            var Oid = dataMap.GetGuid("Oid");
            cnx = dataMap.GetString("cnx");
           



            XpoTypesInfoHelper.GetXpoTypeInfoSource();
            XafTypesInfo.Instance.RegisterEntity(typeof(ScheduleTask));
            XafTypesInfo.Instance.RegisterEntity(typeof(Sql));
            XafTypesInfo.Instance.RegisterEntity(typeof(Log));
            XafTypesInfo.Instance.RegisterEntity(typeof(Connection));

            cnx = XpoDefault.GetConnectionPoolString(cnx);
            XPObjectSpaceProvider osProvider = new XPObjectSpaceProvider(cnx, null);
           
            IObjectSpace objectSpace = osProvider.CreateObjectSpace();


            var Schedule = objectSpace.GetObjectsQuery<ScheduleTask>().FirstOrDefault(sc => sc.Oid == Oid);
            var ExecutionDetail = objectSpace.CreateObject<Log>();
            ExecutionDetail.Date = DateTime.UtcNow;
            try
            {
             

                var TargetDatabase = XpoDefault.GetConnectionPoolString(Schedule.Connection.ConnectionString);
                var TargetDal= XpoDefault.GetDataLayer(TargetDatabase, DevExpress.Xpo.DB.AutoCreateOption.SchemaAlreadyExists);
                UnitOfWork unitOfWork=new UnitOfWork(TargetDal);
                unitOfWork.ExecuteNonQuery(Schedule.Sql.QueryString);
                ExecutionDetail.Result = Result.Success;
                ExecutionDetail.LogText = "Success";


            }
            catch (Exception ex)
            {
                ExecutionDetail.Result = Result.Error;
                ExecutionDetail.LogText = ex.Message;
            }
            Schedule.Logs.Add(ExecutionDetail);
            if (objectSpace.IsModified)
                objectSpace.CommitChanges();




            return Task.CompletedTask;
        }

    
    }
}




