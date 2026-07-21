using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeShaderTest : MonoBehaviour
{
    public ComputeShader computeshader;
    public MeshRenderer meshrenderer;
    private int kernelIndex = -1;

    // Start is called before the first frame update
    void Start()
    {
        ComputeBuffer computeBuffer;
        Material material;

        RenderTexture mRenderTexture = new RenderTexture(256, 256, 16);
        mRenderTexture.enableRandomWrite = true;
        mRenderTexture.Create();

        meshrenderer.sharedMaterial.mainTexture = mRenderTexture;

        kernelIndex = computeshader.FindKernel("CSMain");
        computeshader.SetTexture(kernelIndex, "Result", mRenderTexture);

        computeshader.Dispatch(kernelIndex, 256 / 8, 256 / 8, 1);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
