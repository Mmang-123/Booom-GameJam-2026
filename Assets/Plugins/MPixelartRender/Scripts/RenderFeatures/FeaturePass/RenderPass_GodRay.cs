using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using Mmang.Util;
using Mmang.Topdown;
using Mmang.PixelartRender.VolumeComponents;

namespace Mmang.PixelartRender
{
    public class RenderPass_GodRay : ScriptableRenderPass
    {
        private static readonly Vector3 s_QuadScale = new(50, 200, 10);
        private static readonly string s_PassTag = "God Ray";

        //
        private readonly Mesh m_Mesh;
        private readonly Shader m_Shader;
        //
        private int m_QuadCount;

        // 实例化绘制
        private Matrix4x4[] m_Matrices;
        private Vector4[] m_FadeParams;
        private MaterialPropertyBlock m_Props = new();

        //
        private Material m_Material;
        private Color m_QuadColor;
        private float m_QuadBottom;
        private float m_QuadHeight;
        private Light m_MainLight;

        private GodRay GetGodRayComponent() => VolumeManager.instance.stack.GetComponent<GodRay>();

        public RenderPass_GodRay(Mesh mesh, Shader shader)
        {
            m_Mesh = mesh;
            m_Shader = shader;
            CheckMaterial();

            var godRayComponent = GetGodRayComponent();
            int quadCount = (godRayComponent == null || !godRayComponent.IsActive()) ? 0 : godRayComponent.QuadCount.value;
            RebuildQuads(quadCount);
        }

        private bool CheckMaterial()
        {
            if (m_Shader == null)
                return false;

            if (m_Material == null)
            {
                m_Material = new(m_Shader);
            }

            m_Material.enableInstancing = true;
            return true;
        }

        private void RebuildQuads(int quadCount)
        {
            m_Matrices = new Matrix4x4[quadCount];
            m_FadeParams = new Vector4[quadCount];
        }

        private void InjectPropertyBlock()
        {
            m_Props.Clear();
            m_Props.SetVectorArray("_FadeParams", m_FadeParams);
        }

        public Vector4 ComputeFadeParams(float distanceFade, float spacingFade, float diffuseFade)
        {
            return new(distanceFade, spacingFade, diffuseFade);
        }

        public Matrix4x4 ComputeMatrix(Vector3 position, Quaternion rotation)
        {
            return Matrix4x4.TRS(position, rotation, s_QuadScale);
        }

        private Light GetMainLight()
        {
            if (m_MainLight != null && m_MainLight.gameObject != null)
                return m_MainLight;

            if (RenderSettings.sun != null)
            {
                m_MainLight = RenderSettings.sun;
            }
            else
            {
                // 找强度最高的 Directional Light
                Light[] lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude);

                float maxIntensity = 0f;

                foreach (var light in lights)
                {
                    if (light.type != LightType.Directional)
                        continue;
                    if (light.intensity > maxIntensity)
                    {
                        maxIntensity = light.intensity;
                        m_MainLight = light;
                    }
                }   
            }

            return m_MainLight;
        }

        private void UpdateQuads(TopdownCameraController topdownCamera, Transform lightTrans, float orthographicSize)
        {
            //
            var godRayComponent = GetGodRayComponent();
            Vector2 quadSpacingRange = godRayComponent.QuadSpacingRange.value;
            float quadSpacing = (quadSpacingRange.y - quadSpacingRange.x) / (m_QuadCount - 1);

            //
            var cameraTrans = topdownCamera.transform;
            //面片朝向
            Vector3 normal = Vector3.Cross(cameraTrans.rotation * Vector3.right, lightTrans.rotation * Vector3.forward).normalized;
            Quaternion quadRotation = Quaternion.LookRotation(normal);

            //排列方向
            Vector3 sortDir = new Vector3(cameraTrans.forward.x, 0f, cameraTrans.forward.z).normalized;
            Quaternion sortRotation = Quaternion.LookRotation(sortDir);

            //
            Vector3 centerPositionWS = topdownCamera.CenterPosition;

            //坐标计算
            TransformUtil.Setup(Vector3.zero, sortRotation);

            // 光照方向
            Vector3 lightDirectionVS = TransformUtil.DirectionWorldToLocal(lightTrans.forward);

            // 摄像机
            Vector3 cameraUpDirectionVS = TransformUtil.DirectionWorldToLocal(cameraTrans.up);
            Vector3 cameraDirectionVS = TransformUtil.DirectionWorldToLocal(cameraTrans.forward);
            Vector3 cameraPositionVS = TransformUtil.PositionWorldToLocal(cameraTrans.position);
            Vector2 cameraLinePA = (cameraPositionVS + orthographicSize * cameraUpDirectionVS).GetZY();
            Vector2 cameraLinePB = (cameraPositionVS - 10f * orthographicSize * cameraUpDirectionVS).GetZY(); // 反方向延申以杜绝比相机平面低的光线 (有必要吗?)

            Vector3 centerPositionVS = TransformUtil.PositionWorldToLocal(centerPositionWS);
            float groundHeight = 0f;
            float centerPosYOnCameraRay = centerPositionWS.y * (cameraDirectionVS.y / cameraDirectionVS.z);
            float centerPosZ = centerPositionVS.z + (groundHeight - centerPosYOnCameraRay) * (lightDirectionVS.z / lightDirectionVS.y);

            float snappedCenterPosZ = Mathf.Floor(centerPosZ / quadSpacing) * quadSpacing;


            for (int i = 0; i < m_QuadCount; i++)
            {
                float t = ((float)i) / (m_QuadCount - 1);
                //
                float posZ = snappedCenterPosZ + Mathf.Lerp(quadSpacingRange.x, quadSpacingRange.y, t);

                Vector3 point = new(centerPositionVS.x, groundHeight, posZ);
                float distanceFade;

                Vector2 lightLinePA = point.GetZY();
                Vector2 lightLinePB = (point - lightDirectionVS * 200f).GetZY();

                if (MathUtil.GetSegmentsIntersection(cameraLinePA, cameraLinePB, lightLinePA, lightLinePB, out Vector2 _))
                {
                    distanceFade = 0f;
                }
                else if (MathUtil.GetLineIntersection(cameraLinePA, cameraLinePB, lightLinePA, lightLinePB, out Vector2 pointOnPlane))
                {
                    distanceFade = MathUtil.Saturate
                    (
                        0.1f * (Vector2.Distance(cameraPositionVS.GetZY(), pointOnPlane) - orthographicSize)
                    );
                }
                else
                {
                    // 两线平行
                    distanceFade = 1f;
                }

                float spacingFade = Mathf.Max(0f, posZ - (centerPositionVS.z + quadSpacingRange.x)) / Mathf.Abs(quadSpacingRange.x);

                float diffuseFade = 1f - MathUtil.Clamp(Vector3.Dot(lightTrans.forward, cameraTrans.forward), 0f, 0.8f);


                m_FadeParams[i] = ComputeFadeParams(distanceFade, spacingFade, diffuseFade);
                Vector3 quadPosition = TransformUtil.PositionLocalToWorld(point);
                m_Matrices[i] = ComputeMatrix(quadPosition, quadRotation);
            }
        }

        private class PassData
        {
            public Mesh Mesh;
            public Material Material;
            //public List<GodRayQuad> Quads;
            public Matrix4x4[] Matrices;
            public MaterialPropertyBlock Props;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (!CheckMaterial())
            {
                return;
            }

            var mainLight = GetMainLight();
            if (mainLight == null || mainLight.gameObject == null)
            {
                return;
            }

            var godRayComponent = GetGodRayComponent();
            if (godRayComponent == null || !godRayComponent.IsActive())
            {
                return;
            }

            // Material Parameter
            // Color
            Color color = godRayComponent.Color.value;
            color.a *= godRayComponent.Intensity.value * godRayComponent.QuadAlpha.value;
            if (color != m_QuadColor)
            {
                m_QuadColor = color;
                m_Material.SetColor("_BaseColor", color);
            }
            // BottomY
            float bottomY = godRayComponent.GradientBottom.value;
            if (bottomY != m_QuadBottom)
            {
                m_QuadBottom = bottomY;
                m_Material.SetFloat("_BottomY", bottomY);
            }
            // Height
            float height = godRayComponent.GradientHeight.value;
            if (height != m_QuadHeight)
            {
                m_QuadHeight = height;
                m_Material.SetFloat("_Height", height);
            }

            // quad count changed
            if (godRayComponent.QuadCount.value != m_QuadCount)
            {
                m_QuadCount = godRayComponent.QuadCount.value;
                RebuildQuads(m_QuadCount);
            }

            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            if (cameraData.isSceneViewCamera)
                return;

            Camera camera = cameraData.camera;

            var topdownCamera = PixelartCameraUtils.GetTopdownCameraController(camera);
            if (topdownCamera == null)
                return;
            
            UpdateQuads(topdownCamera, mainLight.transform, camera.orthographicSize);
            InjectPropertyBlock();

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(s_PassTag, out var passData))
            {
                passData.Mesh = m_Mesh;
                passData.Material = m_Material;
                passData.Matrices = m_Matrices;
                passData.Props = m_Props;
                
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.ReadWrite);
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }

        private static void ExecutePass(PassData data, RasterGraphContext context)
        {
            context.cmd.DrawMeshInstanced
            (
                data.Mesh, 
                0, 
                data.Material, 
                0, 
                data.Matrices,
                data.Matrices.Length, 
                data.Props
            );
        }
    }

}