using UnityEngine;
using System.Collections;


namespace LemonSpawn.Gamer {

	public class GalaxyInstance {
		private Galaxy galaxy;
		public Galaxy GetGalaxy() { return galaxy; }
		public void SetGalaxy(Galaxy g) { galaxy = g; galaxyName = galaxy.displayName; }
		public float intensityScale=1, redshift=1;
		public Vector3 position = Vector3.zero;
		public Vector3 orientation = new Vector3(0,1,0);
		public Quaternion rotmat;
		public string galaxyName;

        float[] header = new float[13];

        public float[] toFloatArray()
        {
            float[] a = header;
            a[0] = galaxy.param.windingB;
            a[1] = galaxy.param.windingN;
            a[2] = galaxy.param.noArms;
            a[3] = galaxy.param.arm1;
            a[4] = galaxy.param.arm2;
            a[5] = galaxy.param.arm3;
            a[6] = galaxy.param.arm4;
            a[7] = position.x;
            a[8] = position.y;
            a[9] = position.z;
            a[10] = orientation.x;
            a[11] = orientation.y;
            a[12] = orientation.z;

            return a;
        }


        // Given this instancenew
        public float workTheta, workRad, workWinding, workLength;
		public float winding;
		public float currentRadius;
		
		public GalaxyInstance() {
		
		}
		
		
		public GalaxyInstance Clone() {
			Galaxy g = galaxy.Clone();
			return new GalaxyInstance(g,galaxyName, position, orientation, intensityScale, redshift);
		}
		
		
		private void setupQuaternions() {
			rotmat = Quaternion.FromToRotation(Vector3.up, orientation);//Matrix4x4.TRS(Vector3.zero, quat, Vector3.one);	
			
		}
		
		public GalaxyInstance(Galaxy g, string n, Vector3 p, Vector3 o, float ins, float rs) {
		 galaxy = g;
			position = p;
			galaxyName = n;
			orientation = o;
			redshift = rs;
			intensityScale = ins;
			setupQuaternions();
		}		
		
	}
	
}
