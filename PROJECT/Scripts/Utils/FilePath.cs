using Godot;
using System;

// Author : Camille SMOLARSKI

namespace Com.IsartDigital.OneButtonGame.Utils
{
    public static class FilePath
    {
        public const string UNWANTED_RESOURCE_EXTENSION = ".import";
        public const string UNWANTED_PACKEDSCENE_EXTENSION = ".remap";
        public const string PACKEDSCENE_EXTENSION = ".tscn";

        public const string LEVELS_FOLDER = "res://Scenes/Levels/";

        private static readonly string[] unwantedExtensions = new string[] {
            UNWANTED_RESOURCE_EXTENSION,
            UNWANTED_PACKEDSCENE_EXTENSION
        };

        public static string FetchFilePathFromFolder(string pFolderPath, int pFileIndex)
        {
            DirAccess lDir = DirAccess.Open(pFolderPath);
            int lFileNumber = 0;
            if (lDir != null)
            {
                lDir.ListDirBegin();
                string lFilePath = lDir.GetNext();
                while (lFilePath != "")
                {
                    if (FileExtensionCheck(ref lFilePath))
                    {
                        if (lFileNumber == pFileIndex)
                            return pFolderPath + lFilePath;
                        lFileNumber++;
                    }
                    lFilePath = lDir.GetNext();
                }
            }
            return null;
        }

        private static bool FileExtensionCheck(ref string pFilePath)
        {
            foreach (string lExtension in unwantedExtensions)
                if (pFilePath.Contains(lExtension))
                {
                    pFilePath = pFilePath[..^lExtension.Length];
                    return true;
                }
            return pFilePath.Contains(PACKEDSCENE_EXTENSION);
        }
    }
}