Server Error in '/' Application.
Could not load file or assembly 'System.Memory' or one of its dependencies. The located assembly's manifest definition does not match the assembly reference. (Exception from HRESULT: 0x80131040)
Description: An unhandled exception occurred during the execution of the current web request. Please review the stack trace for more information about the error and where it originated in the code.

Exception Details: System.IO.FileLoadException: Could not load file or assembly 'System.Memory' or one of its dependencies. The located assembly's manifest definition does not match the assembly reference. (Exception from HRESULT: 0x80131040)

Source Error:

An unhandled exception was generated during the execution of the current web request. Information regarding the origin and location of the exception can be identified using the exception stack trace below.

Assembly Load Trace: The following information can be helpful to determine why the assembly 'System.Memory' could not be loaded.


=== Pre-bind state information ===
LOG: DisplayName = System.Memory
 (Partial)
WRN: Partial binding information was supplied for an assembly:
WRN: Assembly Name: System.Memory | Domain ID: 2
WRN: A partial bind occurs when only part of the assembly display name is provided.
WRN: This might result in the binder loading an incorrect assembly.
WRN: It is recommended to provide a fully specified textual identity for the assembly,
WRN: that consists of the simple name, version, culture, and public key token.
WRN: See whitepaper http://go.microsoft.com/fwlink/?LinkId=109270 for more information and common solutions to this issue.
LOG: Appbase = file:///C:/Users/satyavineel.k/Downloads/Merged-Spectra-Final/Enrollment/
LOG: Initial PrivatePath = C:\Users\satyavineel.k\Downloads\Merged-Spectra-Final\Enrollment\bin
Calling assembly : (Unknown).
===
LOG: This bind starts in default load context.
LOG: Using application configuration file: C:\Users\satyavineel.k\Downloads\Merged-Spectra-Final\Enrollment\web.config
LOG: Using host configuration file: C:\Users\satyavineel.k\Documents\IISExpress\config\aspnet.config
LOG: Using machine configuration file from C:\Windows\Microsoft.NET\Framework64\v4.0.30319\config\machine.config.
LOG: Policy not being applied to reference at this time (private, custom, partial, or location-based assembly bind).
LOG: Attempting download of new URL file:///C:/Users/satyavineel.k/AppData/Local/Temp/Temporary ASP.NET Files/vs/028d8304/fe67bead/System.Memory.DLL.
LOG: Attempting download of new URL file:///C:/Users/satyavineel.k/AppData/Local/Temp/Temporary ASP.NET Files/vs/028d8304/fe67bead/System.Memory/System.Memory.DLL.
LOG: Attempting download of new URL file:///C:/Users/satyavineel.k/Downloads/Merged-Spectra-Final/Enrollment/bin/System.Memory.DLL.
LOG: Using application configuration file: C:\Users\satyavineel.k\Downloads\Merged-Spectra-Final\Enrollment\web.config
LOG: Using host configuration file: C:\Users\satyavineel.k\Documents\IISExpress\config\aspnet.config
LOG: Using machine configuration file from C:\Windows\Microsoft.NET\Framework64\v4.0.30319\config\machine.config.
LOG: Redirect found in application configuration file: 4.0.1.1 redirected to 4.0.5.0.
LOG: Post-policy reference: System.Memory, Version=4.0.5.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
LOG: Attempting download of new URL file:///C:/Users/satyavineel.k/AppData/Local/Temp/Temporary ASP.NET Files/vs/028d8304/fe67bead/System.Memory.DLL.
LOG: Attempting download of new URL file:///C:/Users/satyavineel.k/AppData/Local/Temp/Temporary ASP.NET Files/vs/028d8304/fe67bead/System.Memory/System.Memory.DLL.
LOG: Attempting download of new URL file:///C:/Users/satyavineel.k/Downloads/Merged-Spectra-Final/Enrollment/bin/System.Memory.DLL.
WRN: Comparing the assembly name resulted in the mismatch: Revision Number
ERR: Failed to complete setup of assembly (hr = 0x80131040). Probing terminated.

Stack Trace:


[FileLoadException: Could not load file or assembly 'System.Memory' or one of its dependencies. The located assembly's manifest definition does not match the assembly reference. (Exception from HRESULT: 0x80131040)]

[FileLoadException: Could not load file or assembly 'System.Memory, Version=4.0.5.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51' or one of its dependencies. The located assembly's manifest definition does not match the assembly reference. (Exception from HRESULT: 0x80131040)]
   System.Reflection.RuntimeAssembly._nLoad(AssemblyName fileName, String codeBase, Evidence assemblySecurity, RuntimeAssembly locationHint, StackCrawlMark& stackMark, IntPtr pPrivHostBinder, Boolean throwOnFileNotFound, Boolean forIntrospection, Boolean suppressSecurityChecks) +0
   System.Reflection.RuntimeAssembly.InternalLoadAssemblyName(AssemblyName assemblyRef, Evidence assemblySecurity, RuntimeAssembly reqAssembly, StackCrawlMark& stackMark, IntPtr pPrivHostBinder, Boolean throwOnFileNotFound, Boolean forIntrospection, Boolean suppressSecurityChecks) +232
   System.Reflection.RuntimeAssembly.InternalLoad(String assemblyString, Evidence assemblySecurity, StackCrawlMark& stackMark, IntPtr pPrivHostBinder, Boolean forIntrospection) +113
   System.Reflection.RuntimeAssembly.InternalLoad(String assemblyString, Evidence assemblySecurity, StackCrawlMark& stackMark, Boolean forIntrospection) +23
   System.Reflection.Assembly.Load(String assemblyString) +35
   System.Web.Configuration.CompilationSection.LoadAssemblyHelper(String assemblyName, Boolean starDirective) +48

[ConfigurationErrorsException: Could not load file or assembly 'System.Memory, Version=4.0.5.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51' or one of its dependencies. The located assembly's manifest definition does not match the assembly reference. (Exception from HRESULT: 0x80131040)]
   System.Web.Configuration.CompilationSection.LoadAssemblyHelper(String assemblyName, Boolean starDirective) +767
   System.Web.Configuration.CompilationSection.LoadAllAssembliesFromAppDomainBinDirectory() +256
   System.Web.Configuration.CompilationSection.LoadAssembly(AssemblyInfo ai) +58
   System.Web.Compilation.BuildManager.GetReferencedAssemblies(CompilationSection compConfig) +287
   System.Web.Compilation.BuildManager.GetPreStartInitMethodsFromReferencedAssemblies() +69
   System.Web.Compilation.BuildManager.CallPreStartInitMethods(String preStartInitListPath, Boolean& isRefAssemblyLoaded) +137
   System.Web.Compilation.BuildManager.ExecutePreAppStart() +172
   System.Web.Hosting.HostingEnvironment.Initialize(ApplicationManager appManager, IApplicationHost appHost, IConfigMapPathFactory configMapPathFactory, HostingEnvironmentParameters hostingParameters, PolicyLevel policyLevel, Exception appDomainCreationException) +854

[HttpException (0x80004005): Could not load file or assembly 'System.Memory, Version=4.0.5.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51' or one of its dependencies. The located assembly's manifest definition does not match the assembly reference. (Exception from HRESULT: 0x80131040)]
   System.Web.HttpRuntime.FirstRequestInit(HttpContext context) +532
   System.Web.HttpRuntime.EnsureFirstRequestInit(HttpContext context) +111
   System.Web.HttpRuntime.ProcessRequestNotificationPrivate(IIS7WorkerRequest wr, HttpContext context) +719

Version Information: Microsoft .NET Framework Version:4.0.30319; ASP.NET Version:4.8.9319.0
