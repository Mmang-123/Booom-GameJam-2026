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
