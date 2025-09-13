using UnityEditor;

namespace SFEditor.Templates
{
    public static class CreateHLSLMenuItems
    {
        const string TemplatesRoot = "Packages/shatter-fantasy.sf-utilities/Editor/Templates/";

        [MenuItem("Assets/Create/HLSL/Shader Graph/Custom Function")]
        public static void CreateHLSLFile()
        {

            CreateNewFromTemplate("SGCustomFunction", "NewSGCustomFunction.hlsl");
        }

        [MenuItem("Assets/Create/Shader/HLSL/SF Lit Simple")]
        public static void CreateSFLitTextureHLSLFile()
        {

            CreateNewFromTemplate("Shader Templates/SFLitSimpleTemplate", "SF Lit Simple.shader","shader");
        }

        [MenuItem("Assets/Create/Rendering/SF/Base Renderer Feature")]
        public static void CreateSFBaseRendererFeature()
        {

            CreateNewFromTemplate("Renderer Features/SFBaseRendererFeature", "SF Base Renderer Feature.cs", "cs.txt");
        }


        public static void CreateNewFromTemplate(string template, string filename)
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile($"{TemplatesRoot}/{template}.txt", filename);
        }

        public static void CreateNewFromTemplate(
            string template, 
            string filename,
            string fileExtention
            )
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile($"{TemplatesRoot}/{template}.{fileExtention}", filename);
        }
    }
}
