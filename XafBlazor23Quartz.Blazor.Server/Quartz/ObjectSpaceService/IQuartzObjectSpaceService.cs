using DevExpress.ExpressApp;
using System;

namespace XafBlazor23Quartz.Blazor.Server.Quartz.ObjectSpaceService
{
    public interface IQuartzObjectSpaceService
    {
        IObjectSpace GetObjectSpace();
        string GetConnectionString();
    }
}
