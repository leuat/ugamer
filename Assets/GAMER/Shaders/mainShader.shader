
Shader "LemonSpawn/GAMER" {

	Properties{
	}

		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 400



		Lighting Off
		Cull off
		ZWrite off
		ZTest off
//		Blend SrcAlpha OneMinusSrcAlpha
		Pass
	{

		Tags{ "LightMode" = "ForwardBase" }

		CGPROGRAM
// Upgrade NOTE: excluded shader from DX11, Xbox360, OpenGL ES 2.0 because it uses unsized arrays
//#pragma exclude_renderers d3d11 xbox360 gles

#pragma target 4.0
#pragma fragmentoption ARB_precision_hint_fastest


#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fwdbase


//#include "UnityCG.cginc"
//#include "AutoLight.cginc"
#include "Util.cginc"

struct vertexInput {
		float4 vertex : POSITION;
		float4 texcoord : TEXCOORD0;
		float3 normal : NORMAL;
		float4 tangent : TANGENT;
	};

	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 texcoord : TEXCOORD0;
		float3 normal : TEXCOORD1;
		float3 worldPosition : TEXCOORD2;
	};

	struct GalaxyInstance
	{
		float2 winding;
		float noArms;
		float4 arms;
		float3 position;
		float3 orientation;
	};


	float4x4 _GamerViewMatrix;
	float3 _GamerCamera;
	uniform float _GalaxyArray[13];


	void ArrayToGalaxyInstance(in float a[13], out GalaxyInstance gi, int add) {
		gi.winding.x = a[0+add];
		gi.winding.y = a[1+add];
		gi.noArms = a[2 + add];
		gi.arms.x = a[3 + add];
		gi.arms.y = a[4 + add];
		gi.arms.z = a[5 + add];
		gi.arms.w = a[6 + add];
		gi.position = float3(a[7 + add], a[8 + add], a[9 + add]);
		gi.orientation = float3(a[10 + add], a[11 + add], a[12 + add]);

	}


	v2f vert(vertexInput v)
	{
		v2f o;

		float4x4 modelMatrix = unity_ObjectToWorld;
		float4x4 modelMatrixInverse = unity_WorldToObject;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.normal = v.normal;
		o.texcoord = v.texcoord;
		o.worldPosition = mul(modelMatrix, v.vertex);
		return o;
	}

	inline float hh(in float n) { return frac(sin(n)*653.52351823); }
	inline float noise(in float3 x) { float3 p = floor(x); float3 f = frac(x); f = f*f*(3.0 - 2.0*f); float n = p.x + p.y*157.0 + 113.0*p.z; return lerp(lerp(lerp(hh(n + 0.0), hh(n + 1.0), f.x), lerp(hh(n + 157.0), hh(n + 158.0), f.x), f.y), lerp(lerp(hh(n + 113.0), hh(n + 114.0), f.x),lerp(hh(n + 270.0), hh(n + 271.0), f.x), f.y), f.z); }

	/*
	inline float getClouds(float3 p) {
		float ss = _CloudSubtract;
		float A = 0;
		float n = 0;
		p += float3(_XShift, _YShift, _ZShift);
		p.y *= _CloudHeightScatter;
		[unroll]
		for (int i = 1; i < 8; i++) {
			float f = pow(2, i);
			float amp = 1.0 / (2 * pow(i,_CloudScattering));
			float3 t = float3(0.3234, 0.0923, 0.25234)*_CloudTime*cos(i*3.1234)*0.003;
			n += noise((p + t)*f * 10) *amp;
			A += amp;
		}
		return clamp(n - ss*A,0, 1);
	}
	*/

	float3 coord2ray(float x, float y, float width, float height) {

		float aspect_ratio = 1;
		float perspective = 90;
		float FOV = perspective / 360.0 * 2 * 3.14159265; // convert to radians
		float dx = tan(FOV*0.5)*(x / (width / 2) - 1.0) / aspect_ratio;
		float dy = tan(FOV*0.5)*(1 - y / (width / 2));


		float far = 10;

		float3 Pfar = float3(dx*far, dy*far, far);
		float3 res = mul(_GamerViewMatrix, float4(Pfar,1)).xyz;
		res = normalize(res);

		return res;
	}

	float getRadius(in float3 p, out float3 P, out float dott, GalaxyInstance gi) {
		dott =  dot(p, gi.orientation);
		P = p - gi.orientation*dott;
		return length(P);
	}


	float getHeightModulation(in float height, in float z0) {
		float h = abs(height / z0);
		float val = 0;
		// cos(h) is slow, so better to just test if it is really close to zero.
		if (abs(h)>2.0) {
			height = 0;
			return 0;
		}

		val = 1.0f / cosh(h);
		height = val*val;
		return height;
	}

	void Integrate(float3 p, float3 val, in GalaxyInstance gi) {
		float3 P;
		float z;
		float3 currentRadius = getRadius(p, P, z, gi);
		//z = getHeightModulation(z, 0.1);
		float armVal = 1;
		if (z > 0.01) 
		{
			val.x +=1;

		}
		val = gi.orientation;
//		return val;
	}


	float3 rayTrace(float3 start, float3 end, float steplength, inout float3 val, in GalaxyInstance gi) {

		float N = length(end - start) / steplength;
		float3 dir = normalize(end - start)*steplength;
		float3 p = start;
		for (int i = 0; i < N; i++) {
			Integrate(p, val, gi);
			p = p + dir;
			//float v = (noise(p * 10) - 0.5)*0.1;
			//val.x += max(0, v);
		}
		
		return val;
	}

	float3 rayTraceIntersect(float3 start, float3 dir) {
		float3 value = float3(0, 0, 0);

		float t1, t2;
	    GalaxyInstance gi;
		ArrayToGalaxyInstance(_GalaxyArray, gi, 0);
//		gi.winding.x = 1;

		if (intersectSphere(float4(gi.position,1), start, dir, t1, t2)) {
			float3 s = start + dir*t1;
			float3 e = start + dir*t2;
			rayTrace(s, e, 0.01, value, gi);
			value.x = gi.winding.x;
			//value.x = 1;
		}
//		value = gi.position;
		return value;
	}


	fixed4 frag(v2f IN) : COLOR{

		float3 viewDirection = normalize(
			_WorldSpaceCameraPos - IN.worldPosition.xyz)*-1;

		float3 v3CameraPos = _WorldSpaceCameraPos;
		
		float4 c;
		c.a = 1;
		float3 dir = coord2ray(IN.texcoord.x, IN.texcoord.y, 1, 1);
		c.rgb = rayTraceIntersect(_GamerCamera, dir);
	//	c.rgb = float3(IN.texcoord.x, IN.texcoord.y, 0);
	//	c.r = 1;
		c.r = _GalaxyArray[2];

		return c;

	}

	


		ENDCG
	}
	}
		Fallback "Diffuse"
}