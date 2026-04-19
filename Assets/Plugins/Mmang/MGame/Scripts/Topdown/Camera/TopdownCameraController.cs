using System.Collections.Generic;
using UnityEngine;

namespace Mmang.Topdown
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class TopdownCameraController : MonoBehaviour
    {
        static Dictionary<Camera, TopdownCameraController> s_CameraMap;

        [SerializeField] private bool m_InitCenterByRaycast = false;
        [SerializeField] private float m_OffsetDistance = 25f;

        public Camera Camera { get; private set; }
        public Vector3 CenterPosition { get; set; }
        public float OffsetDistance { get => m_OffsetDistance; set => m_OffsetDistance = value; }

        public static TopdownCameraController Get(Camera camera)
        {
            if (camera != null && s_CameraMap != null
            && s_CameraMap.TryGetValue(camera, out var result))
                return result;
            return null;
        }

        private void OnEnable()
        {
            if (Camera == null)
                Camera = GetComponent<Camera>();
            if (Camera == null)
            {
                enabled = false;
                return;
            }

            s_CameraMap ??= new();
            s_CameraMap.Add(Camera, this);
        }

        private void OnDisable()
        {
            if (s_CameraMap != null && s_CameraMap.ContainsKey(Camera))
                s_CameraMap.Remove(Camera);
        }

        private void Start()
        {
            if (m_InitCenterByRaycast)
                RaycastCenter();

            InitComponents();
        }

        private void LateUpdate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            FocusCenterUpdate();
        }

        // todo: 写一个根据平面初始化相机位置
        private void RaycastCenter()
        {
            Ray ray = new(transform.position, transform.forward);
            float maxDistance = OffsetDistance * 2f;
            if (Physics.Raycast(ray, out var hitInfo, maxDistance, ~0))
            {
                CenterPosition = hitInfo.point;
            }
            else
            {
                CenterPosition = transform.position + transform.forward * maxDistance;
            }
        }

        private void FocusCenterUpdate()
        {
            transform.position = CenterPosition - OffsetDistance * transform.forward;
        }


        #region Components

        private void InitComponents()
        {
            var components = GetComponents<TopdownCameraComponent>();
            foreach (var component in components)
            {
                component.Init(this);
            }
        }

        #endregion
    }
}