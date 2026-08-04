Server Error in '/' Application.
Column 'MEMBERSIID' does not belong to table .
Description: An unhandled exception occurred during the execution of the current web request. Please review the stack trace for more information about the error and where it originated in the code.

Exception Details: System.ArgumentException: Column 'MEMBERSIID' does not belong to table .

Source Error:


Line 265:                errorLog.ApplicationName = System.Web.Configuration.WebConfigurationManager.AppSettings["AppName"].ToString();
Line 266:                errorLog.Log(new Elmah.Error(ex));
Line 267:                throw ex;
Line 268:            }
Line 269:        }

Source File: C:\Users\satyavineel.k\Downloads\fhpl-spectra-v1\fhplrepos-fhpl-spectra-e6bce17a7eb8\Enrollment\Controllers\MedicalScrutinyController.cs    Line: 267

Stack Trace:


[ArgumentException: Column 'MEMBERSIID' does not belong to table .]
   Enrollment.Controllers.MedicalScrutinyController.Index() in C:\Users\satyavineel.k\Downloads\fhpl-spectra-v1\fhplrepos-fhpl-spectra-e6bce17a7eb8\Enrollment\Controllers\MedicalScrutinyController.cs:267
   lambda_method(Closure , ControllerBase , Object[] ) +87
   System.Web.Mvc.ReflectedActionDescriptor.Execute(ControllerContext controllerContext, IDictionary`2 parameters) +229
   System.Web.Mvc.ControllerActionInvoker.InvokeActionMethod(ControllerContext controllerContext, ActionDescriptor actionDescriptor, IDictionary`2 parameters) +35
   System.Web.Mvc.Async.AsyncControllerActionInvoker.<BeginInvokeSynchronousActionMethod>b__39(IAsyncResult asyncResult, ActionInvocation innerInvokeState) +39
   System.Web.Mvc.Async.WrappedAsyncResult`2.CallEndDelegate(IAsyncResult asyncResult) +77
   System.Web.Mvc.Async.AsyncControllerActionInvoker.EndInvokeActionMethod(IAsyncResult asyncResult) +42
   System.Web.Mvc.Async.AsyncInvocationWithFilters.<InvokeActionMethodFilterAsynchronouslyRecursive>b__3d() +72
   System.Web.Mvc.Async.<>c__DisplayClass46.<InvokeActionMethodFilterAsynchronouslyRecursive>b__3f() +414
   System.Web.Mvc.Async.<>c__DisplayClass46.<InvokeActionMethodFilterAsynchronouslyRecursive>b__3f() +414
   System.Web.Mvc.Async.AsyncControllerActionInvoker.EndInvokeActionMethodWithFilters(IAsyncResult asyncResult) +42
   System.Web.Mvc.Async.<>c__DisplayClass2b.<BeginInvokeAction>b__1c() +38
   System.Web.Mvc.Async.<>c__DisplayClass21.<BeginInvokeAction>b__1e(IAsyncResult asyncResult) +188
   System.Web.Mvc.Async.AsyncControllerActionInvoker.EndInvokeAction(IAsyncResult asyncResult) +38
   System.Web.Mvc.Controller.<BeginExecuteCore>b__1d(IAsyncResult asyncResult, ExecuteCoreState innerState) +38
   System.Web.Mvc.Async.WrappedAsyncVoid`1.CallEndDelegate(IAsyncResult asyncResult) +73
   System.Web.Mvc.Controller.EndExecuteCore(IAsyncResult asyncResult) +52
   System.Web.Mvc.Async.WrappedAsyncVoid`1.CallEndDelegate(IAsyncResult asyncResult) +39
   System.Web.Mvc.Controller.EndExecute(IAsyncResult asyncResult) +38
   System.Web.Mvc.MvcHandler.<BeginProcessRequest>b__5(IAsyncResult asyncResult, ProcessRequestState innerState) +52
   System.Web.Mvc.Async.WrappedAsyncVoid`1.CallEndDelegate(IAsyncResult asyncResult) +73
   System.Web.Mvc.MvcHandler.EndProcessRequest(IAsyncResult asyncResult) +38
   System.Web.CallHandlerExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute() +652
   System.Web.<>c__DisplayClass285_0.<ExecuteStepImpl>b__0() +36
   System.Web.HttpApplication.ExecuteStepImpl(IExecutionStep step) +121
   System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously) +134

Version Information: Microsoft .NET Framework Version:4.0.30319; ASP.NET Version:4.8.9319.0
