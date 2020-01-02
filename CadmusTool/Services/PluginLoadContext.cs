using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace CadmusTool.Services
{
    // https://github.com/dotnet/samples/blob/master/core/extensions/AppWithPlugin/AppWithPlugin/PluginLoadContext.cs
    public sealed class PluginLoadContext : AssemblyLoadContext
    {
        private AssemblyDependencyResolver _resolver;

        public PluginLoadContext(string path)
        {
            _resolver = new AssemblyDependencyResolver(path);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }
    }
}
