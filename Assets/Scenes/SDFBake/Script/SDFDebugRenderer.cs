using UnityEngine;

public class SDFDebugRenderer : MonoBehaviour
{
    public SDFBaker _baker;
    Material _mat;

    void Start()
        => _mat = this.GetComponent<MeshRenderer>().material;

    void LateUpdate()
    {
        _mat.SetBuffer("_SDFBuffer", _baker.SDFBuffer);
        _mat.SetFloat("_cellSize", _baker.CellSize);
        _mat.SetVector("_cellCount", _baker.CellCount);
        _mat.SetVector("_gridCenter", _baker.Center);
    }
}