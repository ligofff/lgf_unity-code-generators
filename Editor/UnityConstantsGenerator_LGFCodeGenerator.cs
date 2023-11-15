using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Editor;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Need parameters:
/// resourcesFolderName - string
/// resourceConstantTypes - List<string>
/// </summary>
public class UnityConstantsGenerator_LGFCodeGenerator : ILGFCodeGenerator
{
    private Dictionary<string, object> parameters;
    private class ObjTypeContainer
    {
        public Type Type;
        public string obj;
    }
    
    public IEnumerable<LGF_CodeGeneratorFile> Generate(Dictionary<string, object> @params)
    {
        parameters = @params;
        return new []
        {
            new LGF_CodeGeneratorFile("RS.cs", GetCodeString())
        };
    }
    
    private string GetCodeString()
    {
        List<ObjTypeContainer> resources = new List<ObjTypeContainer>();

        var targetInterfaceType = typeof(IResourcesGeneratorTypes);
        
        var interfaceRealizations = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => targetInterfaceType.IsAssignableFrom(p) && !p.IsInterface);

        var generateTypes = new List<Type>();

        foreach (var interfaceRealization in interfaceRealizations)
        {
            var instance = (IResourcesGeneratorTypes) Activator.CreateInstance(interfaceRealization);
            generateTypes = generateTypes.Concat(instance.Types()).ToList();
        }
        
        if (parameters.TryGetValue("resourceConstantTypes", out var resourceConstantTypes))
        {
            var resourceConstantTypesList = (List<string>)resourceConstantTypes;

            foreach (var resourceConstantType in resourceConstantTypesList)
            {
                var type = Type.GetType(resourceConstantType);
                if (type != null)
                {
                    generateTypes.Add(type);
                }
                else
                {
                    throw new Exception($"Type {resourceConstantType} not found!");
                }
            }
        }
        
        foreach (var type in generateTypes)
        {
            var rawRs = Resources.LoadAll((string)parameters["resourcesFolderName"], type);

            foreach (var o in rawRs)
            {
                if (o.GetType() != type) continue;
                
                resources.Add(new ObjTypeContainer()
                {
                    Type = type, obj = AssetDatabase.GetAssetPath(o)
                });
            }
        }

        string code = $@"using UnityEngine;

public static partial class RS
{{
{String.Join("\n\n   ", resources.Select(ResourceToResourceLoadString)) + GetSceneNames() + GetSceneNamesListString() + GetLayersString()}
}}";
        return code;
    }
    
    private string ResourceToResourceLoadString(ObjTypeContainer resource)
    {
        var str = "";
        str += $"private static {resource.Type} prvt_{GetResourceVariableName(resource)}; \n";
        
        str += $"public static {resource.Type} {GetResourceVariableName(resource)} {{ get \n{{";
        str += $"if (prvt_{GetResourceVariableName(resource)} is null) ";
        str += $"prvt_{GetResourceVariableName(resource)} = Resources.Load<{resource.Type}>(\"" + $"{PrepareResourcePath(resource)}\"); \n";
        str += $"return prvt_{GetResourceVariableName(resource)};}}\n}}";
        return str;
    }

    private string GetResourceVariableName(ObjTypeContainer resource)
    {
        return resource.obj.Split('/').Last().Split('.').First().Replace(' ', '_');
    }

    private string PrepareResourcePath(ObjTypeContainer resource)
    {
        return resource.obj.Remove(0, resource.obj.IndexOf((string)parameters["resourcesFolderName"], StringComparison.CurrentCulture))
            .Split('.').First();
    }

    private string GetLayersString()
    {
        List<string> layerNames = new List<string>();

        for(int i = 0; i <= 31; i++)
        {
            var layerN = LayerMask.LayerToName(i);

            if (layerN.Length > 0) layerNames.Add(layerN);
        }
        
        return "\n\n    " + String.Join("\n    ",
            layerNames.Select((layer => $"public const string Layer_{layer.Replace(' ', '_')} = \"{layer}\";")));
    }

    private string GetSceneNames()
    {
        return "\n\n    " + String.Join("\n    ", GetSceneNamesFromBuildSettings().Select((scene => $"public const string Scene_{scene.Replace(' ', '_')} = \"{scene}\";")));
    }

    private string GetSceneNamesListString()
    {
        return "\n\n    public static string[] SceneNames = new string[]{\n       " + string.Join(", ", GetSceneNamesFromBuildSettings().Select(sceneName => $"\"{sceneName}\"")) + "\n   };";
    }

    private List<string> GetSceneNamesFromBuildSettings()
    {
        return EditorBuildSettings.scenes
            .Select(scene => Path.GetFileName(scene.path).Replace(".unity", "")).ToList();
    }
}