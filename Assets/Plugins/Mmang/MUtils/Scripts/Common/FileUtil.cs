using System.Text.RegularExpressions;
using UnityEngine;

namespace Mmang.Util
{
    public static class FileUtil
    {
        public static string GetPathInProject(string path)
        {
            return Regex.Replace(path, "^" + Application.dataPath, "Assets");
        }
    }
}