using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ObserverMass : MonoBehaviour
    {
        [SerializeField] private List<Transform> m_CheckPoints = new();
        [SerializeField] private GameObject m_DisplayRoot;

        [Header("生成设置")]
        [SerializeField] private List<Vector2> m_ControlPoints = new();
        [SerializeField] private Transform m_GenerationRoot;
        [SerializeField] private float m_PointDistance = 0.2f;
        public List<Vector2> ControlPoints => m_ControlPoints;

        // Runtime
        private bool m_Active = true;

        private void FixedUpdate()
        {
            SetActive(CheckLightStrength());
        }

        private void SetActive(bool active)
        {
            if (m_Active == active)
                return;
            m_Active = active;

            m_DisplayRoot.SetActive(active);
        }

        private bool CheckLightStrength()
        {
            float strength = 0f;
            foreach (var point in m_CheckPoints)
            {
                strength += LightingTextureManager.Instance.GetLightStrength(point.position);
            }

            return strength >= 0.0625f;
        }

#if UNITY_EDITOR

        public void Editor_GeneratePoints()
        {
            if (m_GenerationRoot == null || m_PointDistance < 0.01f)
                return;

            foreach (var point in m_CheckPoints)
            {
                if (point == null)
                    continue;
                DestroyImmediate(point.gameObject);
            }
            m_CheckPoints.Clear();

            int count = m_ControlPoints.Count;
            for (int i = 0; i < count - 1; i++)
            {
                var point = m_ControlPoints[i] + (Vector2)transform.position;
                var nextPoint = m_ControlPoints[i + 1] + (Vector2)transform.position;

                Vector2 direction = (nextPoint - point).normalized;

                float distance = Vector2.Distance(point, nextPoint);
                float currentDistance = 0f;
                while (currentDistance < distance)
                {
                    Vector2 position = point + currentDistance * direction;
                    GameObject go = new("Point");
                    go.transform.parent = m_GenerationRoot;
                    go.transform.position = position;
                    m_CheckPoints.Add(go.transform);

                    currentDistance += m_PointDistance;
                }
            }
        }

#endif
    }
}