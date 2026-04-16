using System;
using System.Reflection;

namespace Server.Helper
{
    public class ReferenceLoader
    {
        public string[] LoadReferences(string assemblyPath)
        {
            try
            {
                var assemblyName = AssemblyName.GetAssemblyName(assemblyPath);
                var assembly = Assembly.LoadFrom(assemblyPath);
                var paths = Array.ConvertAll(assembly.GetReferencedAssemblies(), a => a.FullName);
                return paths;
            }
            catch { return null; }
        }

        public void AppDomainSetup(string assemblyPath)
        {
            // Validates that the file is a valid .NET assembly
            AssemblyName.GetAssemblyName(assemblyPath);
        }
    }
}
