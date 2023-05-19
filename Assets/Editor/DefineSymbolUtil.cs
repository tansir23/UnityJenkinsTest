using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// �� ����
/// </summary>
public class DefineSymbolUtil
{
    private const char DEFINE_SEPARATOR = ';';
    private static readonly List<string> _allDefines = new List<string>();

    /// <summary>
    /// ���
    /// </summary>
    /// <param name="defines"></param>
    public static void Add(params string[] defines)
    {
        _allDefines.Clear();
        _allDefines.AddRange(GetDefines());
        _allDefines.AddRange(defines.Except(_allDefines));
        UpdateDefines(_allDefines);
    }

    /// <summary>
    /// ɾ��
    /// </summary>
    /// <param name="defines"></param>
    public static void Remove(params string[] defines)
    {
        _allDefines.Clear();
        _allDefines.AddRange(GetDefines().Except(defines));
        UpdateDefines(_allDefines);
    }

    public static void Clear()
    {
        _allDefines.Clear();
        UpdateDefines(_allDefines);
    }

    private static IEnumerable<string> GetDefines() => PlayerSettings.GetScriptingDefineSymbolsForGroup(
            EditorUserBuildSettings.selectedBuildTargetGroup).Split(DEFINE_SEPARATOR).ToList();

    private static void UpdateDefines(List<string> allDefines) => PlayerSettings.SetScriptingDefineSymbolsForGroup(
            EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(DEFINE_SEPARATOR.ToString(),
                    allDefines.ToArray()));
}
