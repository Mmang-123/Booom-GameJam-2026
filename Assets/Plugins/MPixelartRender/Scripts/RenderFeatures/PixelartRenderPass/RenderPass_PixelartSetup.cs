using Mmang.Topdown;
using Mmang.Util;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Mmang.PixelartRender
{
    public class RenderPass_PixelartRenderSetup : ScriptableRenderPass
    {
        private static readonly string s_PassTag = "Pixelart Setup";
        private class PassData
        {
            public Matrix4x4 ViewMatrix;
            public Matrix4x4 ProjectionMatrix;
            public float UnitSize;
            public float CameraScale;
            public int AdditionalLightsCount;
            public Vector3 FocusPosition;
            public bool IsDebugLUTOn;
        }

        public bool IsDebugLUTOn = false;

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalLightData lightData = frameData.Get<UniversalLightData>();
            var resourceData = frameData.Get<UniversalResourceData>();
            var camera = cameraData.camera;

            //
            float unitSize, cameraScale;
            var pixelartCamera = PixelartManager.Instance.GetPixelartCamera(camera, EPixelartCameraType.Cast);
            if (pixelartCamera == null)
            {
                return;
            }

            unitSize = pixelartCamera.UnitSize;
            cameraScale = pixelartCamera.CameraScale;

            //
            var topdownCameraController = PixelartCameraUtils.GetTopdownCameraController(camera);
            Vector3 focusPosition = topdownCameraController == null ? Vector3.zero : topdownCameraController.CenterPosition.Floor();

            using (var builder = renderGraph.AddUnsafePass<PassData>(s_PassTag, out var passData))
            {
                builder.AllowPassCulling(false);
                passData.ViewMatrix = cameraData.GetViewMatrix();
                passData.ProjectionMatrix = cameraData.GetProjectionMatrix();
                passData.UnitSize = unitSize;
                passData.CameraScale = cameraScale;
                passData.FocusPosition = focusPosition;
                passData.AdditionalLightsCount = lightData.additionalLightsCount;

                passData.IsDebugLUTOn = IsDebugLUTOn;

                builder.SetRenderFunc((PassData data, UnsafeGraphContext context) => ExecutePass(data, context));
            }
        }

        private static void ExecutePass(PassData data, UnsafeGraphContext context)
        {
            Matrix4x4 viewMatrix = data.ViewMatrix;
            float unitSize = data.UnitSize;
            float cameraScale = data.CameraScale;
            Matrix4x4 proj = data.ProjectionMatrix;

            var cmd = context.cmd;

            viewMatrix.m03 = Mathf.Round(viewMatrix.m03 / unitSize) * unitSize;
            viewMatrix.m13 = Mathf.Round(viewMatrix.m13 / unitSize) * unitSize;
            var viewProjMat = proj * viewMatrix;

            // Camera Matrix
            cmd.SetGlobalMatrix(PShaderPropertyID.CameraViewMatrix, viewMatrix);
            cmd.SetGlobalMatrix(PShaderPropertyID.CameraInvViewMatrix, viewMatrix.inverse);
            cmd.SetGlobalMatrix(PShaderPropertyID.CameraViewProjectionMatrix, viewProjMat);
            cmd.SetGlobalMatrix(PShaderPropertyID.CameraInvViewProjectionMatrix, viewProjMat.inverse);
            
            // Camera Params
            cmd.SetGlobalFloat(PShaderPropertyID.UnitSize, unitSize);
            cmd.SetGlobalFloat(PShaderPropertyID.CameraScale, cameraScale);

            // Lighting
            cmd.SetGlobalInt(PShaderPropertyID.AdditionalLightCount, data.AdditionalLightsCount);

            //
            cmd.SetGlobalVector(PShaderPropertyID.CenterFocusPosition, data.FocusPosition);

            // Shader Keyword
            cmd.EnableKeyword(PShaderKeyword.Pixelart);

            if (data.IsDebugLUTOn)
            {
                cmd.EnableKeyword(PShaderKeyword.DebugLUT);
            }
        }
    }

    public class RenderPass_PixelartRenderCleanup : ScriptableRenderPass
    {
        private static readonly string s_PassTag = "Pixelart Cleanup";
        private class PassData { }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddUnsafePass<PassData>(s_PassTag, out var passData))
            {
                builder.AllowPassCulling(false);
                builder.SetRenderFunc((PassData data, UnsafeGraphContext context) => ExecutePass(data, context));
            }
        }

        private static void ExecutePass(PassData data, UnsafeGraphContext context)
        {
            var cmd = context.cmd;
            cmd.DisableKeyword(PShaderKeyword.Pixelart);
#if UNITY_EDITOR
            cmd.DisableKeyword(PShaderKeyword.DebugLUT); 
#endif
        }
    }
}