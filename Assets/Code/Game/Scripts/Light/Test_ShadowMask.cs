

using UnityEngine;

namespace Game
{
    public class Test_ShadowMask : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer m_Renderer;

        private void FixedUpdate()
        {
            float strength = LightingTextureManager.Instance.GetLightStrength(transform.position);
            if (strength <= 0.01f)
            {
                m_Renderer.color = Color.red;
            }
            else
            {
                m_Renderer.color = Color.green;
            }
        }

        [ContextMenu("Test")]
        private void Test()
        {
            //m_Result = ShadowMaskManager.Instance.ReadShadowTexture(m_ChunkIndex);
        }
    }
}