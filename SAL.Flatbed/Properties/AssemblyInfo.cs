using System.Reflection;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: Guid("50fc27e8-4c97-4201-bdf6-d9bd8ff77fed")]
[assembly: System.CLSCompliant(true)]

#if NETSTANDARD || NETCOREAPP
[assembly: AssemblyMetadata("ProjectUrl", "https://dkorablin.ru/project/Default.aspx?File=76")]
#else

[assembly: AssemblyTitle("SAL.Flatbed")]
[assembly: AssemblyDescription("Base SAL Interface")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("Danila Korablin")]
[assembly: AssemblyProduct("Software Abstraction Layer")]
[assembly: AssemblyCopyright("Copyright © Danila Korablin 2009-2024")]
#endif

//[assembly: AssemblyVersion("1.2.10")]