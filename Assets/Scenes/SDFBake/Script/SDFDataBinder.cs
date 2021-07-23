using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

[VFXBinder("SDFData")]
public class SDFDataBinder : VFXBinderBase
{
    [SerializeField, VFXPropertyBinding("UnityEngine.Vector3")]
    ExposedProperty _gridCenter = "Center";

    [SerializeField, VFXPropertyBinding("UnityEngine.Vector3")]
    ExposedProperty _cellCouunt = "CellCount";

    [SerializeField, VFXPropertyBinding("System.Single")]
    ExposedProperty _cellSize = "CellSize";

    [SerializeField, VFXPropertyBinding("UnityEngine.GraphicsBuffer")]
    ExposedProperty _sdfBuffer = "SDFBuffer";

    public SDFBaker _baker = null;

    public override bool IsValid(VisualEffect component)
        => _baker != null
         && component.HasVector3(_gridCenter)
         && component.HasVector3(_cellCouunt)
         && component.HasFloat(_cellSize)
         && component.HasGraphicsBuffer(_sdfBuffer);

    public override void UpdateBinding(VisualEffect component)
    {
        if(!Application.isPlaying) return;

        component.SetVector3(_gridCenter, _baker.Center);
        component.SetVector3(_cellCouunt, _baker.CellCount);
        component.SetFloat(_cellSize, _baker.CellSize);
        component.SetGraphicsBuffer(_sdfBuffer, _baker.SDFFloatBuffer);
    }
}
