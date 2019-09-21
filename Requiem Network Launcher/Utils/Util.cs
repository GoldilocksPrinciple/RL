using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Requiem_Network_Launcher.Utils
{
    public class Util
    {
        public static string GetRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
            {
                throw new ArgumentNullException("No root directory");
            }

            if (string.IsNullOrEmpty(toPath))
            {
                throw new ArgumentNullException("No file path");
            }

            Uri fromUri = new Uri(AppendDirectorySeparatorChar(fromPath));
            Uri toUri = new Uri(AppendDirectorySeparatorChar(toPath));

            if (fromUri.Scheme != toUri.Scheme)
            {
                return toPath;
            }

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (string.Equals(toUri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        private static string AppendDirectorySeparatorChar(string path)
        {
            // Append a slash only if the path is a directory and does not have a slash.
            if (!Path.HasExtension(path) &&
                !path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return path + Path.DirectorySeparatorChar;
            }

            return path;
        }

        public static string GiveName(string name, string pad)
        {
            var wellhello = Encoding.UTF8.GetBytes(name);
            var wellhowareyou = Encoding.UTF8.GetBytes(pad);

            return Convert.ToBase64String(wellhello.Select((b, i) => (byte)(b ^ wellhowareyou[i % wellhowareyou.Length])).ToArray());
        }

        public static string GetName(string name, string pad)
        {
            var namie = Convert.FromBase64String(name);
            var padie = Encoding.UTF8.GetBytes(pad);

            return Encoding.UTF8.GetString(namie.Select((b, i) => (byte)(b ^ padie[i % padie.Length])).ToArray());
        }
    }
}
