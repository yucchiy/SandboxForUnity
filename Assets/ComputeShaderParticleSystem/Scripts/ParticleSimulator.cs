using UnityEngine;
using UnityEditor;

namespace Yucchiy.Sandbox.ComputeShaderParticleSystem
{
    public class ParticleSimulator : MonoBehaviour
    {
        public struct Particle
        {
            public Vector3 Position;
            public float LifeTime;
        }

        private void Start()
        {
            InitializeSimulator();
        }

        private void Update()
        {
            if (_needReset)
            {
                ResetParticles();
                _needReset = false;
            }

            if (_isPlaying)
            {
                ProcessSimulator(Time.deltaTime);
            }
        }

        private void OnDestroy()
        {
            DisposeSimulator();
        }

        private void OnRenderObject()
        {
            RenderParticles();
        }

        private void InitializeSimulator()
        {
            ResetParticles();

            _simulationShaderKernelId = _simulationShader.FindKernel("SimulateParticle");

            _material.SetFloat("particleLifeTime", _particleLifeTime);
        }

        private void ResetParticles()
        {
            if (_particleBuffer != null)
            {
                _particleBuffer.Release();
                _particleBuffer = null;
            }

            var particles = new Particle[_particleCount];

            for (var particleIndex = 0; particleIndex < _particleCount; ++particleIndex)
            {
                const float kRadius = 2f;

                float theta = Random.value * (float)System.Math.PI;
                float phi = Random.value * (float)System.Math.PI * 2f;

                particles[particleIndex].Position = new Vector3(
                    kRadius * (float)System.Math.Sin(theta) * (float)System.Math.Cos(phi),
                    kRadius * (float)System.Math.Sin(theta) * (float)System.Math.Sin(phi),
                    kRadius * (float)System.Math.Cos(theta)
                );

                particles[particleIndex].Position += transform.position;
                particles[particleIndex].LifeTime = Mathf.Max(Random.value * _particleLifeTime, 1f);
            }

            _particleBuffer = new ComputeBuffer(_particleCount, 3 * sizeof(float) + sizeof(float));
            _particleBuffer.SetData(particles);

            _material.SetBuffer("particleBuffer", _particleBuffer);
            _simulationShader.SetBuffer(_simulationShaderKernelId, "particleBuffer", _particleBuffer);
        }

        private void ProcessSimulator(float deltaTime)
        {
            _simulationShader.SetFloat("deltaTime", deltaTime);
            _simulationShader.SetFloat("particleSpeed", _particleSpeed);
            _simulationShader.Dispatch(
                _simulationShaderKernelId,
                Mathf.CeilToInt((float)_particleCount / 256),
                1,
                1
            );
        }

        private void DisposeSimulator()
        {
            _particleBuffer.Release();
            _particleBuffer = null;
        }

        private void RenderParticles()
        {
            _material.SetPass(0);
            Graphics.DrawProcedural(MeshTopology.Points, 1, _particleCount);
        }

        [SerializeField]
        private int _particleCount;
        [SerializeField]
        private Material _material;
        [SerializeField]
        private ComputeShader _simulationShader;
        [SerializeField]
        private float _particleSpeed = 0.05f;
        [SerializeField]
        private float _particleLifeTime = 3f;
        [SerializeField]
        private bool _isPlaying = false;
        [SerializeField]
        private bool _needReset = false;

        ComputeBuffer _particleBuffer;
        private int _simulationShaderKernelId;
    }
}
