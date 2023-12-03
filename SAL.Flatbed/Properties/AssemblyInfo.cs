using System.Reflection;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: Guid("50fc27e8-4c97-4201-bdf6-d9bd8ff77fed")]
[assembly: System.CLSCompliant(true)]

#if !NETSTANDARD
[assembly: AssemblyTitle("SAL.Flatbed")]
[assembly: AssemblyProduct("Software Abstraction Layer Core")]
[assembly: AssemblyCompany("Danila Korablin")]
[assembly: AssemblyCopyright("Copyright © Danila Korablin 2009-2023")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
//[assembly: AssemblyVersion("1.2.9.2")]
#endif