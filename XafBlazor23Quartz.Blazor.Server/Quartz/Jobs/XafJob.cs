
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
            //XpoDirect();


            var dataMap = context.JobDetail.JobDataMap;
            var Oid = dataMap.GetGuid("Oid");
            cnx = dataMap.GetString("cnx");
            var Osp = dataMap.Get("Osp") as IObjectSpaceProvider;



            XpoTypesInfoHelper.GetXpoTypeInfoSource();
            XafTypesInfo.Instance.RegisterEntity(typeof(DomainObject1));



            cnx = XpoDefault.GetConnectionPoolString(cnx);
            XPObjectSpaceProvider osProvider = new XPObjectSpaceProvider(cnx, null);
            IObjectSpace objectSpace = osProvider.CreateObjectSpace();


            var Schedule = objectSpace.GetObjectsQuery<ScheduleBase>().FirstOrDefault(sc => sc.Oid == Oid);
            var ExecutionDetail = objectSpace.CreateObject<ScheduleExecutionDetail>();
            try
            {





                var Instance = objectSpace.CreateObject<DomainObject1>();
                Instance.Name = Oid.ToString() + DateTime.Now.ToString();
                ExecutionDetail.Date = DateTime.UtcNow;
                ExecutionDetail.Log = "Success";


            }
            catch (Exception ex)
            {

                ExecutionDetail.Log = ex.Message;
            }
            Schedule.ScheduleExecutionDetails.Add(ExecutionDetail);
            if (objectSpace.IsModified)
                objectSpace.CommitChanges();



            _logger.LogInformation("Hello world!");
            return Task.CompletedTask;
        }

        private void XpoDirect()
        {


            IDataLayer dl = XpoDefault.GetDataLayer(cnx, DevExpress.Xpo.DB.AutoCreateOption.SchemaAlreadyExists);
            using (Session session = new Session(dl))
            {
                System.Reflection.Assembly[] assemblies = new System.Reflection.Assembly[] {
                       typeof(DomainObject1).Assembly,

                   };
                session.UpdateSchema(assemblies);
                session.CreateObjectTypeRecords(assemblies);
            }
            UnitOfWork UoW = new UnitOfWork(dl);

            DomainObject1 domainObject1 = new DomainObject1(UoW);
            domainObject1.Name = DateTime.Now.ToString();

            UoW.CommitChanges();
        }
    }
}




