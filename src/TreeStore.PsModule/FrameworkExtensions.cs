using System;
using System.IO;
using System.Linq;

namespace TreeStore.PsModule
{
    public static class FrameworkExtensions
    {
        public static bool EnsureValidName(this string name) => !name
            .ToCharArray()
            .Any(c => Path.GetInvalidFileNameChars().Contains(c));
    }
}