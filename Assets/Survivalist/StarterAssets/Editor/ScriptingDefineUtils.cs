using UnityEditor;
using UnityEditor.Build;

namespace StarterAssets
{
    public static class ScriptingDefineUtils
    {
        private static NamedBuildTarget GetNamedBuildTarget()
        {
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            return NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
        }

        public static bool CheckScriptingDefine(string scriptingDefine)
        {
            var namedTarget = GetNamedBuildTarget();
            var defines = PlayerSettings.GetScriptingDefineSymbols(namedTarget);
            return defines.Contains(scriptingDefine);
        }

        public static void SetScriptingDefine(string scriptingDefine)
        {
            var namedTarget = GetNamedBuildTarget();
            var defines = PlayerSettings.GetScriptingDefineSymbols(namedTarget);

            if (!defines.Contains(scriptingDefine))
            {
                // on ajoute un ; proprement si besoin
                if (!string.IsNullOrEmpty(defines) && !defines.EndsWith(";"))
                    defines += ";";

                defines += scriptingDefine;
                PlayerSettings.SetScriptingDefineSymbols(namedTarget, defines);
            }
        }

        public static void RemoveScriptingDefine(string scriptingDefine)
        {
            var namedTarget = GetNamedBuildTarget();
            var defines = PlayerSettings.GetScriptingDefineSymbols(namedTarget);

            if (defines.Contains(scriptingDefine))
            {
                string newDefines = defines.Replace(scriptingDefine, "");

                // nettoyage des ;; et ; en trop
                while (newDefines.Contains(";;"))
                    newDefines = newDefines.Replace(";;", ";");

                newDefines = newDefines.Trim(' ', ';');

                PlayerSettings.SetScriptingDefineSymbols(namedTarget, newDefines);
            }
        }
    }
}