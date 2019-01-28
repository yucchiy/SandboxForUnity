using UnityEngine;

namespace Yucchiy.Sandbox.SimpleComputeShader
{
    public class ParabolaCurveCalculator : MonoBehaviour
    {
        public ComputeShader Shader;
        public float a, p, q;
        public uint CurveLength = 32;
        ComputeBuffer Buffer;

        private void Start()
        {
            var handler = Shader.FindKernel("CalculateParabolaCurve");

            Buffer = new ComputeBuffer((int)CurveLength, sizeof(float));
            Shader.SetBuffer(handler, "buffer", Buffer);

            Shader.SetFloat("a", a);
            Shader.SetFloat("p", p);
            Shader.SetFloat("q", q);

            uint sizeX, sizeY, sizeZ;
            Shader.GetKernelThreadGroupSizes(
                handler,
                out sizeX,
                out sizeY,
                out sizeZ
            );

            Shader.Dispatch(handler, (int)(CurveLength / sizeX), 1, 1);

            var result = new float[CurveLength];
            Buffer.GetData(result);
            foreach (var eachResult in result)
            {
                Debug.Log(eachResult);
            }
        }

        private void OnDestroy()
        {
            Buffer.Release();
            Buffer = null;
        }
    }
}
