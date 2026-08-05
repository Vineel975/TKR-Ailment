<img width="1267" height="705" alt="image" src="https://github.com/user-attachments/assets/530ae1f6-e3be-416a-ac18-7463172909e8" />



Server Error in '/' Application.
The transaction log for database 'Mcareplus_AI' is full due to 'LOG_BACKUP'.
Description: An unhandled exception occurred during the execution of the current web request. Please review the stack trace for more information about the error and where it originated in the code.

Exception Details: System.Data.SqlClient.SqlException: The transaction log for database 'Mcareplus_AI' is full due to 'LOG_BACKUP'.

Source Error:

An unhandled exception was generated during the execution of the current web request. Information regarding the origin and location of the exception can be identified using the exception stack trace below.

Stack Trace:


[SqlException (0x80131904): The transaction log for database 'Mcareplus_AI' is full due to 'LOG_BACKUP'.]
   System.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction) +194
   System.Data.SqlClient.SqlInternalConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction) +108
   System.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, Boolean callerHasConnectionLock, Boolean asyncClose) +606
   System.Data.SqlClient.TdsParser.TryRun(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj, Boolean& dataReady) +4330
   System.Data.SqlClient.SqlCommand.FinishExecuteReader(SqlDataReader ds, RunBehavior runBehavior, String resetOptionsString, Boolean isInternal, Boolean forDescribeParameterEncryption, Boolean shouldCacheForAlwaysEncrypted) +256
   System.Data.SqlClient.SqlCommand.RunExecuteReaderTds(CommandBehavior cmdBehavior, RunBehavior runBehavior, Boolean returnStream, Boolean async, Int32 timeout, Task& task, Boolean asyncWrite, Boolean inRetry, SqlDataReader ds, Boolean describeParameterEncryptionRequest) +2664
   System.Data.SqlClient.SqlCommand.RunExecuteReader(CommandBehavior cmdBehavior, RunBehavior runBehavior, Boolean returnStream, String method, TaskCompletionSource`1 completion, Int32 timeout, Task& task, Boolean& usedCache, Boolean asyncWrite, Boolean inRetry) +1183
   System.Data.SqlClient.SqlCommand.InternalExecuteNonQuery(TaskCompletionSource`1 completion, String methodName, Boolean sendToPipe, Int32 timeout, Boolean& usedCache, Boolean asyncWrite, Boolean inRetry) +374
   System.Data.SqlClient.SqlCommand.ExecuteNonQuery() +305
   DBHelper.DBHelper.ExecuteNonQuery() in C:\Users\satyavineel.k\Downloads\Merged-Spectra-Final-working\Enrollment\Models\DBHelper.cs:338
   Enrollment.ViewModel.AdminLoginViewModel.UsersAudit(Int64 userid, String SystemName, Int32 flag, String iPAddress, String SessionID, Int32 logoutStatus) in C:\Users\satyavineel.k\Downloads\Merged-Spectra-Final-working\Enrollment\ViewModel\AdminLoginViewModel.cs:462
   Enrollment.Controllers.AccountController.LogOut() in C:\Users\satyavineel.k\Downloads\Merged-Spectra-Final-working\Enrollment\Controllers\AccountController.cs:1054
   lambda_method(Closure , ControllerBase , Object[] ) +62
   System.Web.Mvc.ActionMethodDispatcher.Execute(ControllerBase controller, Object[] parameters) +14
   System.Web.Mvc.ReflectedActionDescriptor.Execute(ControllerContext controllerContext, IDictionary`2 parameters) +185
   System.Web.Mvc.ControllerActionInvoker.InvokeActionMethod(ControllerContext controllerContext, ActionDescriptor actionDescriptor, IDictionary`2 parameters) +57
   System.Web.Mvc.Async.AsyncControllerActionInvoker.<BeginInvokeSynchronousActionMethod>b__39(IAsyncResult asyncResult, ActionInvocation innerInvokeState) +22
   System.Web.Mvc.Async.WrappedAsyncResult`2.CallEndDelegate(IAsyncResult asyncResult) +29
   System.Web.Mvc.Async.WrappedAsyncResultBase`1.End() +49
   System.Web.Mvc.Async.AsyncResultWrapper.End(IAsyncResult asyncResult, Object tag) +59
   System.Web.Mvc.Async.AsyncControllerActionInvoker.EndInvokeActionMethod(IAsyncResult asyncResult) +17
   System.Web.Mvc.Async.AsyncInvocationWithFilters.<InvokeActionMethodFilterAsynchronouslyRecursive>b__3d() +50
   System.Web.Mvc.Async.<>c__DisplayClass46.<InvokeActionMethodFilterAsynchronouslyRecursive>b__3f() +228
   System.Web.Mvc.Async.<>c__DisplayClass46.<InvokeActionMethodFilterAsynchronouslyRecursive>b__3f() +228
   System.Web.Mvc.Async.<>c__DisplayClass33.<BeginInvokeActionMethodWithFilters>b__32(IAsyncResult asyncResult) +10
   System.Web.Mvc.Async.WrappedAsyncResult`1.CallEndDelegate(IAsyncResult asyncResult) +10
   System.Web.Mvc.Async.WrappedAsyncResultBase`1.End() +49
   System.Web.Mvc.Async.AsyncResultWrapper.End(IAsyncResult asyncResult, Object tag) +59
   System.Web.Mvc.Async.AsyncControllerActionInvoker.EndInvokeActionMethodWithFilters(IAsyncResult asyncResult) +18
   System.Web.Mvc.Async.<>c__DisplayClass2b.<BeginInvokeAction>b__1c() +26
   System.Web.Mvc.Async.<>c__DisplayClass21.<BeginInvokeAction>b__1e(IAsyncResult asyncResult) +109
   System.Web.Mvc.Async.WrappedAsyncResult`1.CallEndDelegate(IAsyncResult asyncResult) +10
   System.Web.Mvc.Async.WrappedAsyncResultBase`1.End() +49
   System.Web.Mvc.Async.AsyncResultWrapper.End(IAsyncResult asyncResult, Object tag) +18
   System.Web.Mvc.Async.AsyncControllerActionInvoker.EndInvokeAction(IAsyncResult asyncResult) +13
   System.Web.Mvc.Controller.<BeginExecuteCore>b__1d(IAsyncResult asyncResult, ExecuteCoreState innerState) +13
   System.Web.Mvc.Async.WrappedAsyncVoid`1.CallEndDelegate(IAsyncResult asyncResult) +29
   System.Web.Mvc.Async.WrappedAsyncResultBase`1.End() +49
   System.Web.Mvc.Async.AsyncResultWrapper.End(IAsyncResult asyncResult, Object tag) +18
   System.Web.Mvc.Controller.EndExecuteCore(IAsyncResult asyncResult) +27
   System.Web.Mvc.Controller.<BeginExecute>b__15(IAsyncResult asyncResult, Controller controller) +12
   System.Web.Mvc.Async.WrappedAsyncVoid`1.CallEndDelegate(IAsyncResult asyncResult) +22
   System.Web.Mvc.Async.WrappedAsyncResultBase`1.End() +49
   System.Web.Mvc.Async.AsyncResultWrapper.End(IAsyncResult asyncResult, Object tag) +18
   System.Web.Mvc.Controller.EndExecute(IAsyncResult asyncResult) +14
   System.Web.Mvc.Controller.System.Web.Mvc.Async.IAsyncController.EndExecute(IAsyncResult asyncResult) +10
   System.Web.Mvc.MvcHandler.<BeginProcessRequest>b__5(IAsyncResult asyncResult, ProcessRequestState innerState) +21
   System.Web.Mvc.Async.WrappedAsyncVoid`1.CallEndDelegate(IAsyncResult asyncResult) +29
   System.Web.Mvc.Async.WrappedAsyncResultBase`1.End() +49
   System.Web.Mvc.Async.AsyncResultWrapper.End(IAsyncResult asyncResult, Object tag) +18
   System.Web.Mvc.Async.AsyncResultWrapper.End(IAsyncResult asyncResult, Object tag) +6
   System.Web.Mvc.MvcHandler.EndProcessRequest(IAsyncResult asyncResult) +13
   System.Web.Mvc.MvcHandler.System.Web.IHttpAsyncHandler.EndProcessRequest(IAsyncResult result) +9
   System.Web.CallHandlerExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute() +488
   System.Web.<>c__DisplayClass285_0.<ExecuteStepImpl>b__0() +24
   System.Web.StepInvoker.Invoke(Action executionStep) +100
   System.Web.<>c__DisplayClass4_0.<Invoke>b__0() +17
   Microsoft.AspNet.TelemetryCorrelation.TelemetryCorrelationHttpModule.OnExecuteRequestStep(HttpContextBase context, Action step) +64
   System.Web.<>c__DisplayClass284_0.<OnExecuteRequestStep>b__0(Action nextStepAction) +54
   System.Web.StepInvoker.Invoke(Action executionStep) +84
   System.Web.<>c__DisplayClass4_0.<Invoke>b__0() +17
   Microsoft.ApplicationInsights.Web.ApplicationInsightsHttpModule.OnExecuteRequestStep(HttpContextBase context, Action step) in /_/WEB/Src/Web/Web/ApplicationInsightsHttpModule.cs:164
   System.Web.<>c__DisplayClass284_0.<OnExecuteRequestStep>b__0(Action nextStepAction) +54
   System.Web.StepInvoker.Invoke(Action executionStep) +84
   System.Web.HttpApplication.ExecuteStepImpl(IExecutionStep step) +108
   System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously) +148

Version Information: Microsoft .NET Framework Version:4.0.30319; ASP.NET Version:4.7.4136.0
