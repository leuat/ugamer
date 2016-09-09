bool intersectSphere(in float4 sp, in float3 ro, inout float3 rd, out float t1, out float t2)
{
//	bool flip = false;
	if (length(sp.xyz - ro) < sp.w) {
		//rd *= -1;
	//	flip = true;
	}

	bool  r = false;
	float3  d = ro - sp.xyz;
	float b = dot(rd, d);
	float c = dot(d, d) - sp.w*sp.w;
	float t = b*b - c;

	if (t > 0.0)
	{
			t1 = (-b - sqrt(t));
			t2 = (-b + sqrt(t));
		return true;
	}
	
	return false;



}
