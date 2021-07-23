using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

[VFXBinder("CAData")]
public class CADataBinder : VFXBinderBase
{
    public CADispatcher _caData;

    [SerializeField, VFXPropertyBinding("UnityEngine.GraphicsBuffer")]
    ExposedProperty _cellBuffer = "CellBuffer";

    [SerializeField, VFXPropertyBinding("System.UInt32")]
    ExposedProperty _cellCount = "CellCount";

    [SerializeField, VFXPropertyBinding("System.UInt32")]
    ExposedProperty _cellSize = "CellSize";

    public override bool IsValid(VisualEffect component)
        => _caData != null
         && component.HasGraphicsBuffer(_cellBuffer)
         && component.HasUInt(_cellCount)
         && component.HasUInt(_cellSize);

    public override void UpdateBinding(VisualEffect component)
    {
        if(!Application.isPlaying) return;

        component.SetGraphicsBuffer(_cellBuffer, _caData.CellBuffer);
        component.SetUInt(_cellCount, (uint)_caData.CellCount);
        component.SetUInt(_cellSize, (uint)_caData.CellSize);
    }
}
