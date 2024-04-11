using UnityEngine.Rendering;
using UnityEngine;
#if HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

#if HDRP
class FGUIHDRPRenderPass : CustomPass
{
    public LayerMask layer;

    Matrix4x4 worldToCameraMatrix;
    Matrix4x4 projectionMatrix;
    Matrix4x4 cullingMatrix;
    Plane[] planes = new Plane[6];

    public void ApplyChange(Transform stage)
    {
        worldToCameraMatrix = Matrix4x4.TRS(-stage.position, Quaternion.identity, new Vector3(1, 1, -1));
        projectionMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(2 / (stage.position.x * 2 - 0), 2 / -(stage.position.y * 2 - 0), 0));
        cullingMatrix = projectionMatrix * worldToCameraMatrix;
        GeometryUtility.CalculateFrustumPlanes(cullingMatrix, planes);
    }
    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        
    }
    protected override void AggregateCullingParameters(ref ScriptableCullingParameters cullingParameters, HDCamera hdCamera)
    {
        if (hdCamera.camera.cameraType == CameraType.Game)
        {
            cullingParameters.cullingMask |= (uint)layer.value;
            cullingParameters.origin = new Vector3(0, 0, -100);
            cullingParameters.cullingMatrix = cullingMatrix;

            for (int i = 0; i < 6; i++)
                cullingParameters.SetCullingPlane(i, planes[i]);
        }
    }
    protected override void Execute(CustomPassContext ctx)
    {
        if (ctx.hdCamera.camera.cameraType == CameraType.Game)
        {
            var pm = ctx.hdCamera.camera.projectionMatrix;
            var vm = ctx.hdCamera.camera.worldToCameraMatrix;

            ctx.cmd.SetViewProjectionMatrices(worldToCameraMatrix, projectionMatrix);

            CustomPassUtils.DrawRenderers(ctx, layer);
            CoreUtils.SetRenderTarget(ctx.cmd, ctx.cameraColorBuffer, ClearFlag.None);

            //还原矩阵
            ctx.cmd.SetViewProjectionMatrices(vm, pm);
        }
    }

    protected override void Cleanup()
    {
        
    }
}
#endif