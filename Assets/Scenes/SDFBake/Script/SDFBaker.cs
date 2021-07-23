using System.Runtime.InteropServices;
using UnityEngine;

public class SDFBaker : MonoBehaviour
{
    public ComputeShader CS;
    int _initKernel;
    int _bakeKernel;
    int _finalizeKernel;
    int _groupSize;

    MeshFilter _sourceMesh;

    GraphicsBuffer _PositionBuffer;
    GraphicsBuffer _IndicesBuffer;

    Vector3 _center;
    public Vector3 Center => _center;

    Vector3 _meshSize;

    Vector3 _cellCount;
    public Vector3 CellCount => _cellCount;

    float _cellSize;
    public float CellSize => _cellSize;

    Vector3 _gridSize;

    [SerializeField, Range(.01f, .5f)] float _targetCellSize = .05f;
    [SerializeField, Range(.25f, 5f)] float _marginScale = 1.5f;

    GraphicsBuffer _SDFBuffer;
    public GraphicsBuffer SDFBuffer => _SDFBuffer;

    GraphicsBuffer _SDFFloatBuffer;
    public GraphicsBuffer SDFFloatBuffer => _SDFFloatBuffer;

    void Start()
    {      
        _sourceMesh = this.GetComponentInChildren<MeshFilter>();

        // init some mesh data
        int vertCount = _sourceMesh.mesh.vertexCount;
        Vector3[] meshPositions = _sourceMesh.mesh.vertices;
        _PositionBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, vertCount, Marshal.SizeOf(typeof(Vector3)));
        _PositionBuffer.SetData(meshPositions);

        int indexCount = (int)_sourceMesh.mesh.GetIndexCount(0);
        int[] meshIndices = _sourceMesh.mesh.GetIndices(0);
        _IndicesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, indexCount, sizeof(int));
        _IndicesBuffer.SetData(meshIndices);

        // init coumpute kernel
        _initKernel = CS.FindKernel("InitSDF");
        _bakeKernel = CS.FindKernel("BakeSDF");
        _finalizeKernel = CS.FindKernel("FinalizeSDF");

        CS.SetBuffer(_bakeKernel, "_PositionBuffer", _PositionBuffer);
        CS.SetBuffer(_bakeKernel, "_IndicesBuffer", _IndicesBuffer);
    }

    void Update()
    {
        InitSDFGrid();

        CS.SetBuffer(_bakeKernel, "_SDFBuffer", _SDFBuffer);
        CS.Dispatch(_bakeKernel, Mathf.CeilToInt((int)_sourceMesh.mesh.GetIndexCount(0) / 3 / (float)64), 1, 1);

        CS.SetBuffer(_finalizeKernel, "_SDFBuffer", _SDFBuffer);
        CS.SetBuffer(_finalizeKernel, "_SDFFloatBuffer", _SDFFloatBuffer);
        CS.Dispatch(_finalizeKernel, Mathf.CeilToInt(_cellCount.x*_cellCount.y*_cellCount.z / (float)64), 1, 1);
    }

    void OnDestroy()
    {
        _PositionBuffer?.Release();
        _IndicesBuffer?.Release();
        _SDFBuffer?.Release();
        _SDFFloatBuffer?.Release();
    }


    void InitSDFGrid()
    {
        var meshBounds = _sourceMesh.mesh.bounds;
        _center = _sourceMesh.transform.localToWorldMatrix.MultiplyPoint3x4(meshBounds.center);
        _meshSize = Vector3.Scale(meshBounds.size, _sourceMesh.transform.lossyScale) + Vector3.one * _marginScale;

        _center -= _meshSize / 2;

        _cellCount = new Vector3(Mathf.Ceil(_meshSize.x / _targetCellSize) + 1,
                                 Mathf.Ceil(_meshSize.y / _targetCellSize) + 1,
                                 Mathf.Ceil(_meshSize.z / _targetCellSize) + 1);

        Vector3 csize = new Vector3(_meshSize.x / (_cellCount.x - 1),
                                    _meshSize.y / (_cellCount.y - 1),
                                    _meshSize.z / (_cellCount.z - 1));

        _cellSize = Mathf.Max(csize.x, Mathf.Max(csize.y, csize.z));
        _gridSize = new Vector3(_cellCount.x*_cellSize, _cellCount.y*_cellSize, _cellCount.z*_cellSize);

        _center += Vector3.one * _cellSize;

        int numCells = (int)_cellCount.x * (int)_cellCount.y * (int)_cellCount.z;

        _SDFBuffer?.Release();
        _SDFFloatBuffer?.Release();

        _SDFBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, numCells, sizeof(uint));
        _SDFFloatBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, numCells, sizeof(float));

        CS.SetBuffer(_initKernel, "_SDFBuffer", _SDFBuffer);
        CS.SetVector("_gridSize", _gridSize);
        CS.SetVector("_gridCenter", _center);
        CS.SetVector("_cellCount", _cellCount);
        CS.SetFloat("_cellSize", _cellSize);
        CS.SetMatrix("_worldTransform", _sourceMesh.transform.localToWorldMatrix.transpose);

        _groupSize = Mathf.CeilToInt(numCells / (float)64);
        CS.Dispatch(_initKernel, _groupSize, 1, 1);
    }
}