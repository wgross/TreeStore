using System;
using System.Collections.Generic;
using System.Linq;

namespace PSKosmograph
{
    public class KosmographPath
    {
        private readonly int hashCode;

        public IEnumerable<string> Items { get; }
        public bool HasParentNode { get; }
        public string Drive { get; }
        public bool IsRoot { get; }

        public KosmographPath(string drive, IEnumerable<string> pathItems)
        {
            this.Drive = drive;
            this.Items = pathItems.ToArray();
            this.IsRoot = !pathItems.Any();
            this.HasParentNode = pathItems.Any();
        }

        /// <summary>
        /// Tries to parses a KosmographPath instance from its string representation.
        /// Type of the path items is specified by the type parameter.
        /// </summary>
        /// <returns>true on success, false otherwise</returns>
        public static (bool, KosmographPath) TryParse(string path, string? driveSeperator = null, string? directorySeperator = null)
        {
            string directorySeparatorSafe = directorySeperator ?? "\\";
            string driveSeparatorSafe = driveSeperator ?? ":";
            var driveSplit = path.Split(driveSeparatorSafe.ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);

            if (driveSplit.Length == 2)
            {
                var pathSplit = driveSplit[1].Split(directorySeparatorSafe.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                return (true, KosmographPath.Create(driveSplit[0], pathSplit));
            }
            else if (driveSplit.Length == 1)
            {
                var pathSplit = driveSplit[0].Split(directorySeparatorSafe.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                return (true, KosmographPath.Create(drive: null, pathSplit));
            }
            else
            {
                return (true, KosmographPath.Create(drive: null));
            }
        }

        /// <summary>
        /// Creates a KosmographPath instance.
        /// </summary>
        /// <param name="pathItems">collection of items to build a path from</param>
        /// <returns></returns>
        public static KosmographPath Create(string? drive, params string[] pathItems)
        {
            if (pathItems is null)
                throw new ArgumentNullException(nameof(pathItems));

            return new KosmographPath(drive, pathItems);
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.Drive))
                return string.Join("\\", this.Items);
            else
                return $@"{this.Drive}:\{string.Join("\\", this.Items)}";
        }
    }
}