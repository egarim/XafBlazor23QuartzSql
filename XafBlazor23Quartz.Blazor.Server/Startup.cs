﻿using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.Services;
using DevExpress.Persistent.Base;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.Circuits;
using DevExpress.ExpressApp.Xpo;
using XafBlazor23Quartz.Blazor.Server.Services;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using Quartz.Impl;
using Quartz.Spi;
using Quartz;
using XafBlazor23Quartz.Blazor.Server.Quartz.Jobs;
using XafBlazor23Quartz.Blazor.Server.Quartz.ObjectSpaceService;
using XafBlazor23Quartz.Blazor.Server.Quartz.Quartz;
using XafBlazorQuartzHostedService.Module.Blazor.Quartz;
using XafBlazorQuartzHostedService.Module.BusinessObjects;

namespace XafBlazor23Quartz.Blazor.Server;

public class Startup {
    public Startup(IConfiguration configuration) {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services) {
        services.AddSingleton(typeof(Microsoft.AspNetCore.SignalR.HubConnectionHandler<>), typeof(ProxyHubConnectionHandler<>));

        services.AddRazorPages();
        services.AddServerSideBlazor();
        services.AddHttpContextAccessor();
        services.AddScoped<CircuitHandler, CircuitHandlerProxy>();
        services.AddXaf(Configuration, builder => {
            builder.UseApplication<XafBlazor23QuartzBlazorApplication>();
            builder.Modules
                .AddCloningXpo()
                .AddConditionalAppearance()
                .AddValidation(options => {
                    options.AllowValidationDetailsAccess = false;
                })
                .Add<XafBlazor23Quartz.Module.XafBlazor23QuartzModule>()
            	.Add<XafBlazor23QuartzBlazorModule>();
            builder.ObjectSpaceProviders
                .AddSecuredXpo((serviceProvider, options) => {
                    string connectionString = null;
                    if(Configuration.GetConnectionString("ConnectionString") != null) {
                        connectionString = Configuration.GetConnectionString("ConnectionString");
                    }
#if EASYTEST
                    if(Configuration.GetConnectionString("EasyTestConnectionString") != null) {
                        connectionString = Configuration.GetConnectionString("EasyTestConnectionString");
                    }
#endif
                    ArgumentNullException.ThrowIfNull(connectionString);
                    options.ConnectionString = connectionString;
                    options.ThreadSafe = true;
                    options.UseSharedDataStoreProvider = true;
                })
                .AddNonPersistent();
            builder.Security
                .UseIntegratedMode(options => {
                    options.RoleType = typeof(PermissionPolicyRole);
                    // ApplicationUser descends from PermissionPolicyUser and supports the OAuth authentication. For more information, refer to the following topic: https://docs.devexpress.com/eXpressAppFramework/402197
                    // If your application uses PermissionPolicyUser or a custom user type, set the UserType property as follows:
                    options.UserType = typeof(XafBlazor23Quartz.Module.BusinessObjects.ApplicationUser);
                    // ApplicationUserLoginInfo is only necessary for applications that use the ApplicationUser user type.
                    // If you use PermissionPolicyUser or a custom user type, comment out the following line:
                    options.UserLoginInfoType = typeof(XafBlazor23Quartz.Module.BusinessObjects.ApplicationUserLoginInfo);
                    options.UseXpoPermissionsCaching();
                    options.Events.OnSecurityStrategyCreated += securityStrategy => {
                        // Use the 'PermissionsReloadMode.NoCache' option to load the most recent permissions from the database once
                        // for every Session instance when secured data is accessed through this instance for the first time.
                        // Use the 'PermissionsReloadMode.CacheOnFirstAccess' option to reduce the number of database queries.
                        // In this case, permission requests are loaded and cached when secured data is accessed for the first time
                        // and used until the current user logs out. 
                        // See the following article for more details: https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Security.SecurityStrategy.PermissionsReloadMode.
                        ((SecurityStrategy)securityStrategy).PermissionsReloadMode = PermissionsReloadMode.NoCache;
                    };
                })
                .AddPasswordAuthentication(options => {
                    options.IsSupportChangePassword = true;
                });
        });
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => {
            options.LoginPath = "/LoginPage";
        });


        //Add the hosted service, the service that will keep the process running
        services.AddSingleton<XafQuartzHostedService>();
        services.AddHostedService<XafQuartzHostedService>(provider => provider.GetService<XafQuartzHostedService>());

        // Add Quartz services (required by quartz)
        services.AddSingleton<IJobFactory, SingletonJobFactory>();
        services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

        //TODO egister types for the objectspace, this is needed for the job to be able to create/read objects
        List<Type> types = new List<Type>();
        types.Add(typeof(DomainObject1));
        types.Add(typeof(ScheduleBase));
        services.AddSingleton(typeof(IQuartzObjectSpaceService), new QuartzObjectSpaceService(types, Configuration.GetConnectionString("ConnectionString")));

        ////TODO register Jobs
        services.AddSingleton<XafJob>();
        services.AddSingleton<Job1>();
        services.AddSingleton<Job2>();
        //services.AddSingleton(new JobSchedule(
        //    jobType: typeof(XafJob),
        //    cronExpression: "0/5 * * * * ?")); // run every 5 seconds

    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
        if(env.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
        }
        else {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. To change this for production scenarios, see: https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        app.UseRequestLocalization();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseXaf();
        app.UseEndpoints(endpoints => {
            endpoints.MapXafEndpoints();
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
            endpoints.MapControllers();
        });
    }
}
