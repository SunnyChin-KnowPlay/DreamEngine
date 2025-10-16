using Assets.DreamEngine.Editor.Layers.Templates;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MysticIsle.DreamEditor.Utilities
{
    public class LayersEnumGenerator
    {
        [MenuItem("Dream/Utility/Layers/Generate Layers Enum", priority = 21)]
        public static void GenerateLayersEnum()
        {
            List<string> layerNames = null;
            List<string> layerIds = null;

            string[] layers = UnityEditorInternal.InternalEditorUtility.layers;
            if (null != layers && layers.Length > 0)
            {
                layerNames = new List<string>(layers.Length);
                layerIds = new List<string>(layers.Length);

                for (int i = 0; i < layers.Length; i++)
                {
                    layerNames.Add(layers[i].Replace(" ", "_"));
                    layerIds.Add(i.ToString());
                }
            }

            List<string> sortingLayerNames = new(SortingLayer.layers.Length);
            List<string> sortingLayerIds = new(SortingLayer.layers.Length);

            foreach (SortingLayer layer in SortingLayer.layers)
            {
                sortingLayerNames.Add(layer.name.Replace(" ", "_"));
                sortingLayerIds.Add(layer.id.ToString());
            }

            LayersTemplate layersTemplate = new()
            {
                Session = new Dictionary<string, object>
                {
                    ["layerNames"] = layerNames.ToArray(),
                    ["layers"] = layerIds.ToArray(),
                    ["sortingLayerNames"] = sortingLayerNames.ToArray(),
                    ["sortingLayers"] = sortingLayerIds.ToArray(),
                    ["namespaceName"] = $"{Application.productName}.Utilities",
                }
            };

            layersTemplate.Initialize();
            string data = layersTemplate.TransformText();

            string filePath = Path.Combine(Application.dataPath, $"{Application.productName}/Utilities/Layers/Layers.cs");
            filePath = filePath.Replace("\\", "/");

            string folderPath = System.IO.Path.GetDirectoryName(filePath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using FileStream stream = File.Open(filePath, FileMode.OpenOrCreate);
            using StreamWriter writer = new(stream);

            writer.Write(data);
            writer.Flush();

            AssetDatabase.Refresh();
        }
    }
}
