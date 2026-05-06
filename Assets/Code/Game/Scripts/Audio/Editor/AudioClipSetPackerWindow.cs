using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public class AudioClipSetPackerWindow : EditorWindow
    {
        private const string k_OutputDir = "Assets/Configs/Audios";

        private DefaultAsset m_SourceFolder;
        private bool m_OneSetPerFolder = true;
        private AudioClipSet.EPlayMode m_PlayMode = AudioClipSet.EPlayMode.Random;

        private enum EHarmonyPattern3 { Interleaved, Chunked }
        private EHarmonyPattern3 m_HarmonyPattern3 = EHarmonyPattern3.Interleaved;

        [MenuItem("Sloane/Audio/AudioClipSet Packer")]
        private static void Open() =>
            GetWindow<AudioClipSetPackerWindow>("AudioClipSet Packer").minSize = new Vector2(340, 180);

        private void OnGUI()
        {
            EditorGUILayout.Space(4);
            m_SourceFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                "Source Folder", m_SourceFolder, typeof(DefaultAsset), false);

            m_OneSetPerFolder = EditorGUILayout.Toggle(
                new GUIContent("One Set Per Sub-Folder",
                    "勾选：每个子目录生成一个 SO；\n不勾选：整个目录所有 wav 生成一个 SO"),
                m_OneSetPerFolder);

            m_PlayMode = (AudioClipSet.EPlayMode)EditorGUILayout.EnumPopup("Play Mode", m_PlayMode);

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox($"输出目录：{k_OutputDir}", MessageType.Info);

            GUI.enabled = m_SourceFolder != null;
            if (GUILayout.Button("Pack", GUILayout.Height(30)))
                Pack();
            GUI.enabled = true;

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Harmony", EditorStyles.boldLabel);
            m_HarmonyPattern3 = (EHarmonyPattern3)EditorGUILayout.EnumPopup(
                new GUIContent("0~3 Pattern",
                    "Interleaved: 0 1 2 1 0 1 3 1 ...\nChunked: 0 0 0 0 1 1 1 1 2 2 2 2 ..."),
                m_HarmonyPattern3);
            EditorGUILayout.HelpBox(
                "将选定目录中 0.wav ~ 8.wav 按照序列打包为 Sequence AudioClipSet。",
                MessageType.None);
            GUI.enabled = m_SourceFolder != null;
            if (GUILayout.Button("Pack Harmony", GUILayout.Height(30)))
                PackHarmony();
            GUI.enabled = true;
        }

        private void Pack()
        {
            string rootPath = AssetDatabase.GetAssetPath(m_SourceFolder);
            if (!AssetDatabase.IsValidFolder(rootPath))
            {
                EditorUtility.DisplayDialog("错误", "请选择一个文件夹。", "OK");
                return;
            }

            // 确保输出目录存在
            EnsureDirectory(k_OutputDir);

            if (m_OneSetPerFolder)
            {
                // 收集所有包含 wav 的目录（含根目录本身）
                var dirs = new HashSet<string> { rootPath };
                foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { rootPath }))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.EndsWith(".wav", System.StringComparison.OrdinalIgnoreCase))
                        dirs.Add(Path.GetDirectoryName(path).Replace('\\', '/'));
                }

                int count = 0;
                foreach (var dir in dirs)
                {
                    var clips = CollectWavsInDir(dir, recursive: false);
                    if (clips.Count == 0) continue;

                    string setName = Path.GetFileName(dir);
                    CreateSet(setName, clips);
                    count++;
                }
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("完成", $"已生成 {count} 个 AudioClipSet。", "OK");
            }
            else
            {
                var clips = CollectWavsInDir(rootPath, recursive: true);
                if (clips.Count == 0)
                {
                    EditorUtility.DisplayDialog("提示", "未找到任何 wav 文件。", "OK");
                    return;
                }
                string setName = Path.GetFileName(rootPath);
                CreateSet(setName, clips);
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("完成", $"已生成 AudioClipSet：{setName}（共 {clips.Count} 条）。", "OK");
            }

            AssetDatabase.Refresh();
        }

        private List<AudioClip> CollectWavsInDir(string dir, bool recursive)
        {
            var searchOption = recursive
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;

            return Directory.GetFiles(
                    Path.GetFullPath(dir), "*.wav", searchOption)
                .Select(p =>
                {
                    // 转为相对于 Assets 的路径
                    string rel = "Assets" + p.Replace(Path.GetFullPath("Assets"), "").Replace('\\', '/');
                    return AssetDatabase.LoadAssetAtPath<AudioClip>(rel);
                })
                .Where(c => c != null)
                .ToList();
        }

        private void CreateSet(string name, List<AudioClip> clips)
        {
            string assetPath = $"{k_OutputDir}/{name}.asset";

            // 若已存在则覆盖内容
            var set = AssetDatabase.LoadAssetAtPath<AudioClipSet>(assetPath);
            if (set == null)
            {
                set = CreateInstance<AudioClipSet>();
                AssetDatabase.CreateAsset(set, assetPath);
            }

            // 通过 SerializedObject 写入字段
            var so = new SerializedObject(set);
            so.FindProperty("m_PlayMode").enumValueIndex = (int)m_PlayMode;

            var clipsProp = so.FindProperty("m_Clips");
            clipsProp.ClearArray();
            for (int i = 0; i < clips.Count; i++)
            {
                clipsProp.InsertArrayElementAtIndex(i);
                clipsProp.GetArrayElementAtIndex(i).objectReferenceValue = clips[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(set);
        }

        private void PackHarmony()
        {
            string rootPath = AssetDatabase.GetAssetPath(m_SourceFolder);
            if (!AssetDatabase.IsValidFolder(rootPath))
            {
                EditorUtility.DisplayDialog("错误", "请选择一个文件夹。", "OK");
                return;
            }

            // 加载 0~8 号 clip（存在则加载，不存在则留 null）
            var indexed = new AudioClip[9];
            int maxIndex = 0;
            for (int i = 0; i <= 8; i++)
            {
                string path = $"{rootPath}/{i}.wav";
                indexed[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (indexed[i] != null) maxIndex = i;
            }

            // 根据最大有效编号选择排列规则
            int[] sequence;
            string patternDesc;
            if (maxIndex <= 3)
            {
                if (m_HarmonyPattern3 == EHarmonyPattern3.Interleaved)
                {
                    // 0 1 2 1 0 1 3 1 0 1 2 1 0 1 3 1
                    sequence = new int[] { 0, 1, 2, 1, 0, 1, 3, 1, 0, 1, 2, 1, 0, 1, 3, 1 };
                    patternDesc = "0 1 2 1 0 1 3 1 0 1 2 1 0 1 3 1";
                }
                else
                {
                    // 0 0 0 0 1 1 1 1 2 2 2 2 3 3 3 3
                    sequence = new int[] { 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3 };
                    patternDesc = "0 0 0 0 1 1 1 1 2 2 2 2 3 3 3 3";
                }
            }
            else if (maxIndex <= 4)
            {
                // 只有 0~4：1 0 1 0 2 0 2 0 3 0 3 0 4 0 4 0
                sequence = new int[] { 1, 0, 1, 0, 2, 0, 2, 0, 3, 0, 3, 0, 4, 0, 4, 0 };
                patternDesc = "1 0 1 0 2 0 2 0 3 0 3 0 4 0 4 0";
            }
            else
            {
                // 0~8：1 0 2 0 3 0 4 0 5 0 6 0 7 0 8 0
                sequence = new int[] { 1, 0, 2, 0, 3, 0, 4, 0, 5, 0, 6, 0, 7, 0, 8, 0 };
                patternDesc = "1 0 2 0 3 0 4 0 5 0 6 0 7 0 8 0";
            }

            Debug.Log($"[Harmony] 检测到最大编号 {maxIndex}，使用排列：{patternDesc}");

            var clips = sequence
                .Select(idx => idx < indexed.Length ? indexed[idx] : null)
                .Where(c => c != null)
                .ToList();

            if (clips.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "未找到任何有效 clip。", "OK");
                return;
            }

            EnsureDirectory(k_OutputDir);

            string folderName = Path.GetFileName(rootPath);
            string assetPath = $"{k_OutputDir}/{folderName}.asset";

            var set = AssetDatabase.LoadAssetAtPath<AudioClipSet>(assetPath);
            if (set == null)
            {
                set = CreateInstance<AudioClipSet>();
                AssetDatabase.CreateAsset(set, assetPath);
            }

            var so = new SerializedObject(set);
            so.FindProperty("m_PlayMode").enumValueIndex = (int)AudioClipSet.EPlayMode.Sequence;
            var clipsProp = so.FindProperty("m_TheHarmony");
            clipsProp.ClearArray();
            for (int i = 0; i < clips.Count; i++)
            {
                clipsProp.InsertArrayElementAtIndex(i);
                clipsProp.GetArrayElementAtIndex(i).objectReferenceValue = clips[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("完成",
                $"Harmony AudioClipSet 已写入：\n{assetPath}\n共 {clips.Count} 条（序列含 null 项已跳过）。", "OK");
        }

        private static void EnsureDirectory(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
