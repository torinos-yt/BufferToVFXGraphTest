using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CADispatcher : MonoBehaviour
{
    public ComputeShader _shader;

    [SerializeField, Range(20f, 300f)] int _countPerAxis = 100;

    GraphicsBuffer _cellBuffer;
    GraphicsBuffer _copyBuffer;
    public GraphicsBuffer CellBuffer => _cellBuffer;
    public int CellCount => (int)Mathf.Pow(_countPerAxis, 3);
    public int CellSize => _countPerAxis;

    int threadCount => Mathf.CeilToInt(_countPerAxis / 8);

    int _initKernel;
    int _updateKernel;
    int _copyKernel;

    void Start()
    {
        _cellBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, CellCount, sizeof(float));
        _copyBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, CellCount, sizeof(float));

        _initKernel = _shader.FindKernel("Init");
        _updateKernel = _shader.FindKernel("Update");
        _copyKernel = _shader.FindKernel("Copy");

        _shader.SetInts("_CellSize", _countPerAxis, _countPerAxis, _countPerAxis);
        _shader.SetInt("_CellCount", CellCount);

        _shader.SetBuffer(_initKernel,   "_CellBuffer", _cellBuffer);
        _shader.SetBuffer(_updateKernel, "_CellBuffer", _cellBuffer);
        _shader.SetBuffer(_copyKernel,   "_CellBuffer", _cellBuffer);

        _shader.SetBuffer(_initKernel,   "_CopyBuffer", _copyBuffer);
        _shader.SetBuffer(_updateKernel, "_CopyBuffer", _copyBuffer);
        _shader.SetBuffer(_copyKernel,   "_CopyBuffer", _copyBuffer);

        _shader.Dispatch(_initKernel, threadCount, threadCount, threadCount);
    }

    float d = 5;
    void Update()
    {
        d+=Time.deltaTime;
        if(d < .05) return;

        if(Input.GetKey(KeyCode.Space)) 
            _shader.Dispatch(_initKernel, threadCount, threadCount, threadCount);

        _shader.Dispatch(_updateKernel, threadCount, threadCount, threadCount);
        _shader.Dispatch(_copyKernel, threadCount, threadCount, threadCount);
        d = 0;
    }

    void OnDestroy()
    {
        _cellBuffer?.Release();
        _copyBuffer?.Release();
    }
}
