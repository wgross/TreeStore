using System;
using System.IO;
using System.Linq;

namespace TreeStore.PsModule
{
    public static class Guard
    {
        public static class Against
        {
            public static void InvalidNameCharacters(string name, string message)
            {
                if (name.ToCharArray().Any(c => Path.GetInvalidFileNameChars().Contains(c)))
                    throw new InvalidOperationException($"{message}: it contains invalid characters");
            }

            public static void InvalidReservedNodeNames(string name, string message)
            {
                if (TreeStoreCmdletProvider.ReservedNodeNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"{message}: Name '{name}' is reserved for future use.");
            }
        }
    }
}