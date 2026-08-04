Server Error in '/' Application.
The specified network name is no longer available
Description: An unhandled exception occurred during the execution of the current web request. Please review the stack trace for more information about the error and where it originated in the code.

Exception Details: System.ComponentModel.Win32Exception: The specified network name is no longer available

Source Error:


Line 53:             {
Line 54:                 
Line 55:                 throw ex;
Line 56:             }
Line 57:            

Source File: C:\Users\satyavineel.k\Downloads\Merged-Spectra-Final\EnrollmentBAL\MasterUtilsBL.cs    Line: 55

Stack Trace:


[Win32Exception (0x80004005): The specified network name is no longer available]

[SqlException (0x80131904): A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)]
   System.Data.SqlClient.SqlInternalConnectionTds..ctor(DbConnectionPoolIdentity identity, SqlConnectionString connectionOptions, SqlCredential credential, Object providerInfo, String newPassword, SecureString newSecurePassword, Boolean redirectedUserInstance, SqlConnectionString userConnectionOptions, SessionData reconnectSessionData, DbConnectionPool pool, String accessToken, Boolean applyTransientFaultHandling, SqlAuthenticationProviderManager sqlAuthProviderManager) +1524
   System.Data.SqlClient.SqlConnectionFactory.CreateConnection(DbConnectionOptions options, DbConnectionPoolKey poolKey, Object poolGroupProviderInfo, DbConnectionPool pool, DbConnection owningConnection, DbConnectionOptions userOptions) +467
   System.Data.ProviderBase.DbConnectionFactory.CreatePooledConnection(DbConnectionPool pool, DbConnection owningObject, DbConnectionOptions options, DbConnectionPoolKey poolKey, DbConnectionOptions userOptions) +70
   System.Data.ProviderBase.DbConnectionPool.CreateObject(DbConnection owningObject, DbConnectionOptions userOptions, DbConnectionInternal oldConnection) +1145
   System.Data.ProviderBase.DbConnectionPool.UserCreateRequest(DbConnection owningObject, DbConnectionOptions userOptions, DbConnectionInternal oldConnection) +111
   System.Data.ProviderBase.DbConnectionPool.TryGetConnection(DbConnection owningObject, UInt32 waitForMultipleObjectsTimeout, Boolean allowCreate, Boolean onlyOneCheckConnection, DbConnectionOptions userOptions, DbConnectionInternal& connection) +1567
   System.Data.ProviderBase.DbConnectionPool.TryGetConnection(DbConnection owningObject, TaskCompletionSource`1 retry, DbConnectionOptions userOptions, DbConnectionInternal& connection) +118
   System.Data.ProviderBase.DbConnectionFactory.TryGetConnection(DbConnection owningConnection, TaskCompletionSource`1 retry, DbConnectionOptions userOptions, DbConnectionInternal oldConnection, DbConnectionInternal& connection) +268
   System.Data.ProviderBase.DbConnectionInternal.TryOpenConnectionInternal(DbConnection outerConnection, DbConnectionFactory connectionFactory, TaskCompletionSource`1 retry, DbConnectionOptions userOptions) +315
   System.Data.SqlClient.SqlConnection.TryOpenInner(TaskCompletionSource`1 retry) +128
   System.Data.SqlClient.SqlConnection.TryOpen(TaskCompletionSource`1 retry) +265
   System.Data.SqlClient.SqlConnection.Open() +133
   System.Data.Entity.Infrastructure.Interception.InternalDispatcher`1.Dispatch(TTarget target, Action`2 operation, TInterceptionContext interceptionContext, Action`3 executing, Action`3 executed) +104
   System.Data.Entity.Infrastructure.Interception.DbConnectionDispatcher.Open(DbConnection connection, DbInterceptionContext interceptionContext) +503
   System.Data.Entity.SqlServer.<>c__DisplayClass1.<Execute>b__0() +18
   System.Data.Entity.SqlServer.DefaultSqlExecutionStrategy.Execute(Func`1 operation) +89

[EntityException: An exception has been raised that is likely due to a transient failure. If you are connecting to a SQL Azure database consider using SqlAzureExecutionStrategy.]
   System.Data.Entity.SqlServer.DefaultSqlExecutionStrategy.Execute(Func`1 operation) +229
   System.Data.Entity.Core.EntityClient.EntityConnection.Open() +321

[EntityException: The underlying provider failed on Open.]
   System.Data.Entity.Core.EntityClient.EntityConnection.Open() +741
   System.Data.Entity.Core.Objects.ObjectContext.EnsureConnection(Boolean shouldMonitorTransactions) +167
   System.Data.Entity.Core.Objects.ObjectContext.ExecuteInTransaction(Func`1 func, IDbExecutionStrategy executionStrategy, Boolean startLocalTransaction, Boolean releaseConnectionOnSuccess) +63
   System.Data.Entity.Core.Objects.<>c__DisplayClass7.<GetResults>b__5() +203
   System.Data.Entity.SqlServer.DefaultSqlExecutionStrategy.Execute(Func`1 operation) +89

[EntityException: An exception has been raised that is likely due to a transient failure. If you are connecting to a SQL Azure database consider using SqlAzureExecutionStrategy.]
   EnrollmentBAL.MasterUtilsBL.GetAllProperties() in C:\Users\satyavineel.k\Downloads\Merged-Spectra-Final\EnrollmentBAL\MasterUtilsBL.cs:55
   Enrollment.Models.MasterUtilModel.LoadTables() in C:\Users\satyavineel.k\Downloads\Merged-Spectra-Final\Enrollment\Models\MasterUtilModel.cs:45
   Enrollment.Models.MasterUtilModel.ReadMasterData() in C:\Users\satyavineel.k\Downloads\Merged-Spectra-Final\Enrollment\Models\MasterUtilModel.cs:40
   Enrollment.MvcApplication.Application_Start() in C:\Users\satyavineel.k\Downloads\Merged-Spectra-Final\Enrollment\Global.asax.cs:79

[HttpException (0x80004005): An exception has been raised that is likely due to a transient failure. If you are connecting to a SQL Azure database consider using SqlAzureExecutionStrategy.]
   System.Web.HttpApplicationFactory.EnsureAppStartCalledForIntegratedMode(HttpContext context, HttpApplication app) +517
   System.Web.HttpApplication.RegisterEventSubscriptionsWithIIS(IntPtr appContext, HttpContext context, MethodInfo[] handlers) +185
   System.Web.HttpApplication.InitSpecial(HttpApplicationState state, MethodInfo[] handlers, IntPtr appContext, HttpContext context) +168
   System.Web.HttpApplicationFactory.GetSpecialApplicationInstance(IntPtr appContext, HttpContext context) +277
   System.Web.Hosting.PipelineRuntime.InitializeApplication(IntPtr appContext) +369

[HttpException (0x80004005): An exception has been raised that is likely due to a transient failure. If you are connecting to a SQL Azure database consider using SqlAzureExecutionStrategy.]
   System.Web.HttpRuntime.FirstRequestInit(HttpContext context) +532
   System.Web.HttpRuntime.EnsureFirstRequestInit(HttpContext context) +111
   System.Web.HttpRuntime.ProcessRequestNotificationPrivate(IIS7WorkerRequest wr, HttpContext context) +719

Version Information: Microsoft .NET Framework Version:4.0.30319; ASP.NET Version:4.8.9319.0
