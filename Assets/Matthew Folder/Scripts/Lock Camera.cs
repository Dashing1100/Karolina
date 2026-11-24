using Unity.Cinemachine;
using UnityEngine;

public class LockCamera : CinemachineExtension
{
    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (enabled && stage == CinemachineCore.Stage.Finalize)
        {
            Vector3 pos = state.GetCorrectedPosition();
            pos.x = 2;
            state.RawPosition = pos;
            state.PositionCorrection = Vector3.zero;
        }
    }
}
