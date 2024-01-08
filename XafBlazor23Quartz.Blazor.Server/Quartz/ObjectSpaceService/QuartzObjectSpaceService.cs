using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Text;

namespace XafBlazor23Quartz.Blazor.Server.Quartz.ObjectSpaceService
{
    public class QuartzObjectSpaceService : IQuartzObjectSpaceService
    {
        XPObjectSpaceProvider osProvider;
        public QuartzObjectSpaceService(IEnumerable<Type> Types, string Cnx)
        {
            XpoTypesInfoHelper.GetXpoTypeInfoSource();
            foreach (Type type in Types)
            {
                XafTypesInfo.Instance.RegisterEntity(type);
            }

            osProvider = new XPObjectSpaceProvider(Cnx, null);

        }

        public string GetConnectionString()
        {
            return osProvider.ConnectionString;
        }

        public IObjectSpace GetObjectSpace()
        {
            return osProvider.CreateObjectSpace();
        }
    }
}
