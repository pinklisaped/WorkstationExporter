using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace WorkstationState
{
    /// <summary>
    /// Helper class for reading embedded resources
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Get one resource
        /// </summary>
        /// <param name="resourceName">Resource name: Pings/Processes/HTTP</param>
        /// <param name="assembly">Default this assembly</param>
        /// <returns>Stream resource</returns>
        public static Stream GetResource(string resourceName, System.Reflection.Assembly assembly = null)
        {
            if (assembly == null)
                assembly = new System.Diagnostics.StackFrame(1).GetMethod().Module.Assembly;

            string resource = assembly.GetManifestResourceNames().First(n => n.EndsWith(resourceName));
            return assembly.GetManifestResourceStream(resource);
        }

        /// <summary>
        /// Get some resources by mask end file
        /// </summary>
        /// <param name="resourceMask">Mask "s" return Pings/Processes</param>
        /// <param name="assembly">Default this assembly</param>
        /// <returns>Stream resource</returns>
        public static IEnumerable<Stream> GetResources(string resourceMask, System.Reflection.Assembly assembly = null)
        {
            if (assembly == null)
                assembly = new System.Diagnostics.StackFrame(1).GetMethod().Module.Assembly;
            IEnumerable<string> resources = assembly.GetManifestResourceNames().Where(n => n.EndsWith(resourceMask));
            foreach (string resource in resources)
                yield return assembly.GetManifestResourceStream(resource);
        }
    }
}
