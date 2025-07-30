using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MysticIsle.DreamEditor
{
    public class PlayerPrefsEditorWindow : EditorWindow
    {
        [MenuItem("Dream/PlayerPrefs/Clear PlayerPrefs")]
        private static void ClearPlayerPrefs()
        {
            UnityEngine.PlayerPrefs.DeleteAll();
        }

        [MenuItem("Dream/Clear PersistentDataPath")]
        private static void ClearPersistentDataPath()
        {
            string directoryPath = Application.persistentDataPath;

            // 检查路径是否存在
            if (Directory.Exists(directoryPath))
            {
                // 获取所有文件路径
                string[] files = Directory.GetFiles(directoryPath);
                foreach (string file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Failed to delete file: " + file + "\n" + ex.Message);
                    }
                }

                // 获取所有子目录路径
                string[] directories = Directory.GetDirectories(directoryPath);
                foreach (string dir in directories)
                {
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Failed to delete directory: " + dir + "\n" + ex.Message);
                    }
                }

                Debug.Log("Persistent data path cleared.");
            }
            else
            {
                Debug.LogWarning("Directory does not exist: " + directoryPath);
            }
        }
    }

}


