using UnityEngine;
using System.Collections;

namespace LemonSpawn.Gamer {

[System.Serializable]
public class GalaxyComponentBulge : GalaxyComponent {
		
		public override void componentIntensity(RasterPixel rp, Vector3 p, float ival) {
			//    return I;
			float rho_0 = componentParams.strength*ival;
			Vector3 pos = currentGI.rotmat*(p);
			
			float rad = (pos.magnitude+0.01f)*componentParams.r0;
			rad+=0.01f;
			
			float i = rho_0 * (Mathf.Pow(rad,-0.855f)*Mathf.Exp(- Mathf.Pow(rad,1/4.0f)) -0.05f) *currentGI.intensityScale;
			
//			float i = rho_0*componentParams.arm * (*spline)[rad] *params->intensityScale;
//			i = 1;
		 	//i = 1;
			
			if (i<0) i=0;
			rp.I += i*spectrum.spectrum*rp.scale;
			
		}
		

		public  override void calculateIntensity(RasterPixel rp, Vector3 p, GalaxyInstance gi, float weight) {
			Vector3 P;
			float z = 1;
			currentGI = gi;
			
			currentRadius = getRadius(p, out P, out z, gi);
			componentIntensity(rp,p, weight);
			
		}
		
}

	public class GalaxyComponentDisk : GalaxyComponent {
	
		public override void componentIntensity(RasterPixel rp, Vector3 p, float ival) {
			float p2 = 0.5f;
			// Level = 14
			if (ival<0.0005)
				return;
//			if (componentParams.scale!=0.0)
				p2 = Mathf.Abs(getPerlinCloudNoise(p, 1.0f, winding, 9, componentParams.scale, componentParams.ks));

/*			if (Util.rnd.NextDouble()>0.99)
				Debug.Log (p2);
*/			p2 = Mathf.Max (p2, 0.01f);
			if (p2!=0)
				p2=0.1f/p2;
			
			p2 = Mathf.Pow(p2,componentParams.noiseTilt);
			
			//params->diskStarModifier = p2;//CMath::Minmax(p2, 0.1, 1.0);
			
			p2+=componentParams.noiseOffset;
			
			if (p2<0)
				return;
			
	//		ival = 1;
			rp.I+= ival*p2*spectrum.spectrum*rp.scale;
		}
		
	
	}
		

	public class GalaxyComponentDust : GalaxyComponent {
		
		public override void componentIntensity(RasterPixel rp, Vector3 p, float ival) {
			if (ival<0.0005)
				return;
			
			float p2 = getPerlinCloudNoise(p, currentRadius, winding, 10, componentParams.scale, componentParams.ks);
			//if (p2!=0)
			p2=0.1f/(Mathf.Abs(p2) + 0.1f);
			
			p2 = p2 + componentParams.noiseOffset;
			if (p2<0)
				return;
			p2 = Mathf.Pow(5*p2,componentParams.noiseTilt);
			//keep = p2;
			
			float s = gamer.rast.RP.rayStepNormal*100;
			
			rp.I.x*=Mathf.Exp(-p2*ival*spectrum.spectrum.x*s);
			rp.I.y*=Mathf.Exp(-p2*ival*spectrum.spectrum.y*s);
			rp.I.z*=Mathf.Exp(-p2*ival*spectrum.spectrum.z*s);
			
			
		}
		
		
	}

	public class GalaxyComponentStars : GalaxyComponent {
		
		public override void componentIntensity(RasterPixel rp, Vector3 r, float ival) {
	/*		if (ival<0.0005)
				return;
		*/	
			float perlinnoise = Mathf.Abs(Util.Simplex.octave_noise_3d(14,componentParams.ks,0.01f*componentParams.scale,r.x,r.y,r.z));

            float addNoise = 0;
            if (componentParams.noiseOffset != 0)
            {
                addNoise = (componentParams.noiseOffset * getPerlinCloudNoise(r * 2, currentRadius, winding, 4, 2, -2));
                addNoise += 0.5f * (componentParams.noiseOffset * getPerlinCloudNoise(r * 2, currentRadius, winding * 0.5f, 4, 4, -2));
            }
            float val = Mathf.Abs(Mathf.Pow(perlinnoise+1 + addNoise,componentParams.noiseTilt));
            //            val = perlinnoise;

            rp.I +=  ival * val * spectrum.spectrum *rp.scale;
            			
			
		}
		
		
	}

		
	
	
}