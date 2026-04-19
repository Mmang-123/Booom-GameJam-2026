using Mmang.Game;
using UnityEngine;

namespace Mmang.Test
{
    public class Test_GameplayTag : MonoBehaviour
    {
        [SerializeField] private int m_AA;
        [SerializeField] private GameplayTag m_TagA;
        [SerializeField] private GameplayTag m_TagB;

        [SerializeField] private GameplayTagContainer m_ContainerA;
        [SerializeField] private GameplayTagContainer m_ContainerB;

        [ContextMenu("Test")]
        private void TestFunc()
        {
            Debug.Log($"A Valid: {m_TagA.IsValid()}");
            Debug.Log($"B Valid: {m_TagB.IsValid()}");
            Debug.Log($"A Contains B: {m_TagA.Contains(m_TagB)}");
            Debug.Log($"ContainerA Contains A: {m_ContainerA.Contains(m_TagA)}");
            Debug.Log($"ContainerA Contains B: {m_ContainerA.Contains(m_TagB)}");
            Debug.Log($"ContainerA ContainsAll ContainerB: {m_ContainerA.ContainsAll(m_ContainerB)}");
        }

        [ContextMenu("Normalize Container")]
        private void NormalizeContainer()
        {
            m_ContainerA.Normalize();
            m_ContainerB.Normalize();
        }

        [ContextMenu("Refresh Config")]
        private void RefreshConfig()
        {
            GameplayTagsSettings.Refresh();     
        }
    }
}