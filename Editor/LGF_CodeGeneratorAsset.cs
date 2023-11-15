using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CreateAssetMenu(menuName = nameof(LGF_CodeGeneratorAsset), fileName = nameof(LGF_CodeGeneratorAsset))]
    public class LGF_CodeGeneratorAsset : ScriptableObject
    {
        public string generatedFolderPath = "Generated";
        
        [Space]
        public string resourceConstantAssetsFolderName = "_ResourceConstants";
        
        private string FolderPath => $"{Application.dataPath}/{generatedFolderPath}";
        
        [ContextMenu("Generate")]
        private void GenerateAll()
        {
            ValidateOutputFolder();
            
            var parameters = new Dictionary<string, object>();
            parameters["resourcesFolderName"] = resourceConstantAssetsFolderName;

            var generatorTypes = TypeCache.GetTypesDerivedFrom<ILGFCodeGenerator>();

            var outputFiles = new List<LGF_CodeGeneratorFile>();
            
            foreach (var generatorType in generatorTypes)
            {
                var generator = (ILGFCodeGenerator)Activator.CreateInstance(generatorType);
                Debug.Log($"Generating {generatorType.Name}...");

                try
                {
                    var generatedFiles = generator.Generate(parameters);
                    outputFiles.AddRange(generatedFiles);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error while generating {generatorType.Name}:\n{e}");
                    throw;
                }
            }
            
            var groupedByFilename = outputFiles.GroupBy(x => x.fileName);
            
            if (groupedByFilename.Any(group => group.Count() > 1))
            {
                throw new Exception("Duplicated file names!");
            }
            
            foreach (var outputFile in outputFiles)
            {
                File.WriteAllText($"{FolderPath}/{outputFile.fileName}", outputFile.fileContent);
                Debug.Log($"Generated {outputFile.fileName}");
            }
            
            AssetDatabase.Refresh();
        }
        
        private void ValidateOutputFolder()
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
        }
    }
}