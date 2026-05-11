using UnityEngine;

namespace Game
{
    public class CircularProgressBar : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer m_Renderer;
        [SerializeField] private Color m_Color;
        [SerializeField] private float m_T;

        [Header("动画设置")]
        [SerializeField] private float m_AnimateOutDuration = 0.15f;
        [SerializeField] private float m_ScaleMultiplier = 1.3f;

        private enum EAnimState { Idle, Active, AnimatingOut }
        private EAnimState m_AnimState = EAnimState.Idle;
        private float m_AnimTimer;
        private Vector3 m_TargetScale;

        private MaterialPropertyBlock m_PropBlock;
        private MaterialPropertyBlock PropBlock => m_PropBlock ??= new MaterialPropertyBlock();
        private static readonly int s_FadeID = Shader.PropertyToID("_Fade");
        private static readonly int s_ProgressID = Shader.PropertyToID("_Progress");

        private float m_Fade = 0f;

        public void AnimateIn()
        {
            gameObject.SetActive(true);
            m_AnimState = EAnimState.Active;
            m_Fade = 1f;
            FlushBlock();
            transform.localScale = m_TargetScale;
        }

        public void AnimateOut()
        {
            if (m_AnimState == EAnimState.Idle || m_AnimState == EAnimState.AnimatingOut)
                return;
            m_AnimState = EAnimState.AnimatingOut;
            m_AnimTimer = 0f;
        }

        private void Update()
        {
            if (m_AnimState != EAnimState.AnimatingOut)
                return;

            m_AnimTimer += Time.deltaTime;
                float t = Mathf.Clamp01(m_AnimTimer / m_AnimateOutDuration);
            float ease = Mathf.Sin(t * Mathf.PI * 0.5f); // OutSine
            transform.localScale = Vector3.Lerp(m_TargetScale, m_TargetScale * m_ScaleMultiplier, ease);
            m_Fade = 1f - t; // 线性反向
            FlushBlock();
            if (t >= 1f)
            {
                m_AnimState = EAnimState.Idle;
                gameObject.SetActive(false);
            }
        }

        // 一次性写入所有 per-renderer 属性，避免分次读写的竞争
        private void FlushBlock()
        {
            PropBlock.SetFloat(s_FadeID, m_Fade);
            PropBlock.SetFloat(s_ProgressID, Mathf.SmoothStep(0f, 1f, m_T));
            m_Renderer.SetPropertyBlock(PropBlock);
        }

        public void SetT(float t)
        {
            m_T = t;
            FlushBlock();
        }

        public void SetColor(Color color)
        {
            m_Color = color;
            m_Renderer.color = new(color.r, color.g, color.b, 1f);
        }

        public void SetFish(Fish fish, ControlFishConfig config)
        {
            SetColor(fish.BodyColor);

            if (fish.FishTypeTag.Equals(FishUtils.JellyGleamTag))
                m_TargetScale = new Vector3(4, 4, 4);
            else
                m_TargetScale = config.InfectRadius * Vector3.one;
            

            /*
            m_TargetScale = fish.FishTypeTag.Equals(FishUtils.GolemFishTag)
                ? new Vector3(7, 7, 7)
                : new Vector3(4, 4, 4);
            */
        }
    }
}