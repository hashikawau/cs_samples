using System;
using System.Collections.Generic;
using System.IO;

namespace Mtp {
    static class PathUtils {
        public static string[] SplitPath(string path) {
            if (string.IsNullOrEmpty(path))
                return Array.Empty<string>();
            return
                SplitPathHelper(
                    path,
                    new List<string>()
                )
                .ToArray();
        }

        private static List<string> SplitPathHelper(
            string path,
            List<string> accum
        ) {
            if (string.IsNullOrEmpty(path))
                return accum;
            var fileName = Path.GetFileName(path);
            if (string.IsNullOrEmpty(fileName))
                return SplitPathHelper(
                    Path.GetDirectoryName(path),
                    accum
                );
            var newAccum = new List<string>(accum);
            newAccum.Insert(0, Path.GetFileName(path));
            return SplitPathHelper(
                Path.GetDirectoryName(path),
                newAccum
            );
        }
    }
}
