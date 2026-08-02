Server Error in '/' Application.
Object reference not set to an instance of an object.
Description: An unhandled exception occurred during the execution of the current web request. Please review the stack trace for more information about the error and where it originated in the code.

Exception Details: System.NullReferenceException: Object reference not set to an instance of an object.

Source Error:


Line 11:         public static void RegisterGlobalFilters(GlobalFilterCollection filters)
Line 12:         {
Line 13:             var HideConcurrentLoginFunc = Convert.ToBoolean(ConfigurationManager.AppSettings["HideConcurrentLoginFunc"].ToString());
Line 14:             if (!HideConcurrentLoginFunc)
Line 15:             {

Source File: C:\Users\satyavineel.k\Downloads\fhpl-spectra-v1\fhplrepos-fhpl-spectra-e6bce17a7eb8\Enrollment\App_Start\FilterConfig.cs    Line: 13

Stack Trace:


[NullReferenceException: Object reference not set to an instance of an object.]
   Enrollment.FilterConfig.RegisterGlobalFilters(GlobalFilterCollection filters) in C:\Users\satyavineel.k\Downloads\fhpl-spectra-v1\fhplrepos-fhpl-spectra-e6bce17a7eb8\Enrollment\App_Start\FilterConfig.cs:13
   Enrollment.MvcApplication.Application_Start() in C:\Users\satyavineel.k\Downloads\fhpl-spectra-v1\fhplrepos-fhpl-spectra-e6bce17a7eb8\Enrollment\Global.asax.cs:31

[HttpException (0x80004005): Object reference not set to an instance of an object.]
   System.Web.HttpApplicationFactory.EnsureAppStartCalledForIntegratedMode(HttpContext context, HttpApplication app) +517
   System.Web.HttpApplication.RegisterEventSubscriptionsWithIIS(IntPtr appContext, HttpContext context, MethodInfo[] handlers) +185
   System.Web.HttpApplication.InitSpecial(HttpApplicationState state, MethodInfo[] handlers, IntPtr appContext, HttpContext context) +168
   System.Web.HttpApplicationFactory.GetSpecialApplicationInstance(IntPtr appContext, HttpContext context) +277
   System.Web.Hosting.PipelineRuntime.InitializeApplication(IntPtr appContext) +369

[HttpException (0x80004005): Object reference not set to an instance of an object.]
   System.Web.HttpRuntime.FirstRequestInit(HttpContext context) +532
   System.Web.HttpRuntime.EnsureFirstRequestInit(HttpContext context) +111
   System.Web.HttpRuntime.ProcessRequestNotificationPrivate(IIS7WorkerRequest wr, HttpContext context) +719

Version Information: Microsoft .NET Framework Version:4.0.30319; ASP.NET Version:4.8.9319.0
