Shader "Custom/ComputeShaderParticleSystem/Particle"
{
	Properties
	{
		_Color ("Color", Color) = (1,0.5,0.2,1)
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

            struct particle
            {
                float3 position;
                float lifeTime;
            };

			struct v2f
			{
                float4 position : SV_POSITION;
                float4 color : COLOR;
			};

            StructuredBuffer<particle> particleBuffer;
            float particleLifeTime;
            fixed4 _Color;
			
			v2f vert (uint vertex_id : SV_VertexID, uint instance_id : SV_InstanceID)
			{
                v2f o;

                o.color = fixed4(_Color.xyz, 1.0f) * step(0.1f, (particleBuffer[instance_id].lifeTime / particleLifeTime));
                o.position = UnityObjectToClipPos(float4(particleBuffer[instance_id].position, 1.0f));

                return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}
	}
}
