using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Sloane.Editor
{
    public class FrameAnimationCreator : EditorWindow
    {
        // ── 单条 Clip 配置 ────────────────────────────────────────────
        [System.Serializable]
        private class ClipConfig
        {
            public string ClipName = "NewAnimation";
            public bool Loop = false;
            public int FrameCount = 4;
            public bool Fold = true;
        }

        // ── 窗口状态 ─────────────────────────────────────────────────
        private GameObject m_Target;
        private float m_FrameRate = 10f;
        private string m_SavePath = "Assets/Animations";
        // 用于追踪每个 Renderer 已消耗的帧偏移（生成时计算，不持久化）
        // private int m_NextStartFrame = 0;

        private List<SpriteRenderer> m_Renderers = new List<SpriteRenderer>();
        // 全局：每个 Renderer 对应一张 Multiple Sprite 纹理
        private List<Texture2D> m_RendererTextures = new List<Texture2D>();
        private List<ClipConfig> m_Clips = new List<ClipConfig>();

        private Vector2 m_ScrollPos;

        [MenuItem("Sloane/Frame Animation Creator")]
        public static void Open() => GetWindow<FrameAnimationCreator>("Frame Animation Creator");

        // ── GUI ───────────────────────────────────────────────────────
        private void OnGUI()
        {
            // 顶部固定区域
            DrawHeader();

            if (m_Target == null || m_Renderers.Count == 0)
            {
                EditorGUILayout.HelpBox("请选择一个根节点，其子物体上需要有 SpriteRenderer。", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(4);

            // Clip 列表撑满剩余高度
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            DrawClipList();
            EditorGUILayout.EndVertical();

            // 底部按钮固定
            EditorGUILayout.Space(4);
            using (new EditorGUI.DisabledScope(m_Clips.Count == 0))
            {
                if (GUILayout.Button("批量生成 Clips", GUILayout.Height(34)))
                    CreateAllClips();
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            m_Target = (GameObject)EditorGUILayout.ObjectField("Root (Animator)", m_Target, typeof(GameObject), true);
            if (EditorGUI.EndChangeCheck()) RefreshRenderers();

            if (m_Target != null && m_Target.GetComponent<Animator>() == null)
                EditorGUILayout.HelpBox("Root 上没有 Animator 组件。", MessageType.Warning);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("全局设置", EditorStyles.boldLabel);
            m_FrameRate = EditorGUILayout.FloatField("Frame Rate (FPS)", Mathf.Max(1f, m_FrameRate));
            m_SavePath  = EditorGUILayout.TextField("Save Path", m_SavePath);

            if (m_Renderers.Count > 0)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Renderer → Texture (Multiple Sprite)", EditorStyles.boldLabel);
                for (int r = 0; r < m_Renderers.Count; r++)
                {
                    EditorGUI.BeginChangeCheck();
                    var tex = (Texture2D)EditorGUILayout.ObjectField(
                        m_Renderers[r].gameObject.name,
                        m_RendererTextures[r],
                        typeof(Texture2D), false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (tex != null)
                        {
                            var imp = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex)) as TextureImporter;
                            if (imp == null || imp.spriteImportMode != SpriteImportMode.Multiple)
                                Debug.LogWarning($"[FrameAnimationCreator] {tex.name} 的导入模式不是 Multiple。");
                        }
                        m_RendererTextures[r] = tex;
                    }
                    if (m_RendererTextures[r] != null)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField($"检测到 {GetSpriteCount(m_RendererTextures[r])} 帧", EditorStyles.miniLabel);
                        EditorGUI.indentLevel--;
                    }
                }
            }
        }

        private void DrawClipList()
        {
            EditorGUILayout.LabelField("Clips", EditorStyles.boldLabel);

            m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);

            for (int c = 0; c < m_Clips.Count; c++)
            {
                var cfg = m_Clips[c];

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // ── Clip header ──
                EditorGUILayout.BeginHorizontal();
                cfg.Fold = EditorGUILayout.Foldout(cfg.Fold, cfg.ClipName, true, EditorStyles.foldoutHeader);
                if (GUILayout.Button("×", GUILayout.Width(22)))
                {
                    m_Clips.RemoveAt(c);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                EditorGUILayout.EndHorizontal();

                if (cfg.Fold)
                {
                    float labelW = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 80f;
                    EditorGUI.BeginChangeCheck();
                    cfg.ClipName   = EditorGUILayout.TextField("Clip Name", cfg.ClipName);
                    if (EditorGUI.EndChangeCheck())
                    {
                        string lower = cfg.ClipName.ToLower();
                        if (lower.Contains("idle") || lower.Contains("move"))
                            cfg.Loop = true;
                        else
                            cfg.Loop = false;
                    }
                    cfg.FrameCount = Mathf.Max(1, EditorGUILayout.IntField("Frame Count", cfg.FrameCount));
                    cfg.Loop       = EditorGUILayout.Toggle("Loop", cfg.Loop);
                    EditorGUIUtility.labelWidth = labelW;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("+ 添加 Clip"))
                AddClip();
        }

        // ── 逻辑 ─────────────────────────────────────────────────────
        private void RefreshRenderers()
        {
            m_Renderers.Clear();
            m_RendererTextures.Clear();
            if (m_Target != null)
            {
                m_Renderers.AddRange(m_Target.GetComponentsInChildren<SpriteRenderer>(true));
                for (int i = 0; i < m_Renderers.Count; i++)
                    m_RendererTextures.Add(null);
            }
        }

        private void AddClip()
        {
            m_Clips.Add(new ClipConfig { ClipName = $"Animation_{m_Clips.Count}" });
        }

        private static int GetSpriteCount(Texture2D tex)
        {
            string path = AssetDatabase.GetAssetPath(tex);
            var all = AssetDatabase.LoadAllAssetsAtPath(path);
            int count = 0;
            foreach (var a in all)
                if (a is Sprite) count++;
            return count;
        }

        private static Sprite[] GetSpritesFromTexture(Texture2D tex)
        {
            string path = AssetDatabase.GetAssetPath(tex);
            var all = AssetDatabase.LoadAllAssetsAtPath(path);
            var sprites = new List<Sprite>();
            foreach (var a in all)
                if (a is Sprite s) sprites.Add(s);
            // 按名称尾部数字排序（Unity 默认命名为 textureName_0, _1 ...）
            sprites.Sort((a, b) =>
            {
                int ia = ExtractTrailingNumber(a.name);
                int ib = ExtractTrailingNumber(b.name);
                return ia.CompareTo(ib);
            });
            return sprites.ToArray();
        }

        private static int ExtractTrailingNumber(string name)
        {
            int i = name.Length - 1;
            while (i >= 0 && char.IsDigit(name[i])) i--;
            string num = name.Substring(i + 1);
            return num.Length > 0 ? int.Parse(num) : 0;
        }

        private void CreateAllClips()
        {
            if (!Directory.Exists(m_SavePath))
                Directory.CreateDirectory(m_SavePath);

            var controller = GetAnimatorController();
            // 每个 Renderer 的起始帧偏移，按 clip 顺序顺延
            int[] startFrames = new int[m_Renderers.Count];

            foreach (var cfg in m_Clips)
            {
                if (string.IsNullOrWhiteSpace(cfg.ClipName)) continue;
                var clip = CreateClip(cfg, startFrames);
                if (clip != null && controller != null)
                    AddClipToController(controller, clip);
                // 推进每个 Renderer 的偏移
                for (int r = 0; r < m_Renderers.Count; r++)
                    startFrames[r] += cfg.FrameCount;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private AnimationClip CreateClip(ClipConfig cfg, int[] startFrames)
        {
            var clip = new AnimationClip { name = cfg.ClipName, frameRate = m_FrameRate };

            var clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
            clipSettings.loopTime = cfg.Loop;
            AnimationUtility.SetAnimationClipSettings(clip, clipSettings);

            for (int r = 0; r < m_Renderers.Count; r++)
            {
                var tex = m_RendererTextures[r];
                if (tex == null) continue;
                if (m_Renderers[r].sprite == null) continue;

                Sprite[] allSprites = GetSpritesFromTexture(tex);
                int start = startFrames[r];
                int count = Mathf.Min(cfg.FrameCount, allSprites.Length - start);
                if (count <= 0) continue;

                string bindPath = AnimationUtility.CalculateTransformPath(
                    m_Renderers[r].transform, m_Target.transform);

                var binding = new EditorCurveBinding
                {
                    path         = bindPath,
                    type         = typeof(SpriteRenderer),
                    propertyName = "m_Sprite"
                };

                var keyframes = new ObjectReferenceKeyframe[count];
                for (int f = 0; f < count; f++)
                {
                    keyframes[f] = new ObjectReferenceKeyframe
                    {
                        time  = f / m_FrameRate,
                        value = allSprites[start + f]
                    };
                }

                AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
            }

            string fullPath = $"{m_SavePath}/{cfg.ClipName}.anim";
            AnimationClip result;
            var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(fullPath);
            if (existing != null)
            {
                EditorUtility.CopySerialized(clip, existing);
                EditorGUIUtility.PingObject(existing);
                Debug.Log($"[FrameAnimationCreator] Updated: {fullPath}");
                result = existing;
            }
            else
            {
                AssetDatabase.CreateAsset(clip, fullPath);
                EditorGUIUtility.PingObject(clip);
                Debug.Log($"[FrameAnimationCreator] Created: {fullPath}");
                result = clip;
            }
            return result;
        }

        private AnimatorController GetAnimatorController()
        {
            if (m_Target == null) return null;
            var animator = m_Target.GetComponent<Animator>();
            if (animator == null || animator.runtimeAnimatorController == null) return null;
            string path = AssetDatabase.GetAssetPath(animator.runtimeAnimatorController);
            return AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        }

        private static void AddClipToController(AnimatorController controller, AnimationClip clip)
        {
            // 检查是否已存在
            foreach (var existing in controller.animationClips)
                if (existing.name == clip.name) return;

            controller.AddMotion(clip);
            EditorUtility.SetDirty(controller);
            Debug.Log($"[FrameAnimationCreator] Added '{clip.name}' to {controller.name}");
        }
    }
}

