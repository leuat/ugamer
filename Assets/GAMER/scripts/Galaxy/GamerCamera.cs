﻿using UnityEngine;
using System.Collections;

namespace LemonSpawn.Gamer {
	[ System.Serializable ]
	public class GamerCamera {

		
		public Vector3 camera, target, up;
		public float perspective;

        Matrix4x4 viewMatrix, viewMatrixInv;
        private Matrix4x4 rotMatrix;

        public void setRotMatrix(Matrix4x4 mat)
        {
            rotMatrix = mat;
        }		
        public Matrix4x4 getRotMatrix()
        {
            return rotMatrix;
        }
		
        public void setRotation(Vector3 v)
        {
            rotMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.EulerAngles(v), Vector3.one);
            setupViewmatrix();
        }


        public GamerCamera copy()
        {
            GamerCamera g = new GamerCamera();
            g.camera = camera;
            g.target = target;
            g.perspective = perspective;
            g.rotMatrix = rotMatrix;
            g.up = up;
            return g;
        }


        public GamerCamera()
        {
            rotMatrix = Matrix4x4.identity;
        }

		public void TranslateXY(Vector3 delta) {
			Vector3 right = Vector3.Cross((camera-target), up).normalized;
			Vector3 d =  -delta.y*up.normalized + right*delta.x;
			camera = camera + d;
			target = target + d;
		}

		public void ZoomXY(Vector3 delta) {
			camera = camera - (camera-target).normalized*delta.y;
		}

		public void RotateUp(float r) {
			Vector3 d = (camera-target).normalized;
			Vector3 right = Vector3.Cross(d, up).normalized;
			up = Vector3.Cross(right, d).normalized;
			Quaternion q = Quaternion.AngleAxis(r, d);
			up = q*up;

//			camera = camera - (camera-target).normalized*delta.y;
		}



        /*        public void RotateDegrees(float t, float p)
                {
                    Quaternion q = Quaternion.ro

                }
                */

        public void RotateXY(Vector3 delta) {
			gamer.rast.RP.camera.RotateVertical(delta.x*10);
			gamer.rast.RP.camera.RotateHorisontal(delta.y*10);

		}

        public Matrix4x4 GetRotationMatrix()
        {
            Vector3 zaxis = (camera-target).normalized;
            Vector3 xaxis = (Vector3.Cross(up, zaxis)).normalized*-1;
            Vector3 yaxis = (Vector3.Cross(zaxis, xaxis)).normalized;

            Matrix4x4 M = Matrix4x4.identity;
            //			Matrix4x4.
            /*				M[0,0] = xaxis.x;       
                            M[1,0] = yaxis.x;      
                            M[2,0] = zaxis.x;

                            M[0,1] = xaxis.y;       
                            M[1,1] = yaxis.y;      
                            M[2,1] = zaxis.y;

                            M[0,2] = xaxis.z;
                            M[1,2] = yaxis.z;      
                            M[2,2] = zaxis.z;

                            M[0,3] = -camera.x;     
                            M[1,3] = -camera.y;
                            M[2,3] = -camera.z;
            */
            M[0, 0] = xaxis.x;
            M[0, 1] = yaxis.x;
            M[0, 2] = zaxis.x;

            M[1, 0] = xaxis.y;
            M[1, 1] = yaxis.y;
            M[1, 2] = zaxis.y;

            M[2, 0] = xaxis.z;
            M[2, 1] = yaxis.z;
            M[2, 2] = zaxis.z;

            M[3, 0] = 0;
            M[3, 1] = 0;
            M[3, 2] = 0;

            return M;

        }



        public void UpdateMaterial(Material m)
        {
            m.SetMatrix("_GamerViewMatrix", viewMatrix);
            m.SetVector("_GamerCamera", camera);
        }


        public void setupViewmatrix() {
			Vector3 zaxis = (target-camera).normalized;
			Vector3 xaxis = (Vector3.Cross(up,zaxis)).normalized;
			Vector3 yaxis = (Vector3.Cross(zaxis,xaxis)).normalized;
				
			Matrix4x4 M = Matrix4x4.identity;
//			Matrix4x4.
/*				M[0,0] = xaxis.x;       
				M[1,0] = yaxis.x;      
				M[2,0] = zaxis.x;
				
				M[0,1] = xaxis.y;       
				M[1,1] = yaxis.y;      
				M[2,1] = zaxis.y;
				
				M[0,2] = xaxis.z;
				M[1,2] = yaxis.z;      
				M[2,2] = zaxis.z;
				
				M[0,3] = -camera.x;     
				M[1,3] = -camera.y;
				M[2,3] = -camera.z;
*/
			M[0,0] = xaxis.x;       
			M[0,1] = yaxis.x;      
			M[0,2] = zaxis.x;
			
			M[1,0] = xaxis.y;       
			M[1,1] = yaxis.y;      
			M[1,2] = zaxis.y;
			
			M[2,0] = xaxis.z;
			M[2,1] = yaxis.z;      
			M[2,2] = zaxis.z;
			
			M[3,0] = -camera.x;     
			M[3,1] = -camera.y;
			M[3,2] = -camera.z;

//            Debug.Log(rotMatrix);

			viewMatrix = rotMatrix*M;				
			
			
		}
		
		public void RotateVertical(float angle) {
			Vector3 d = camera - target;
			Vector3 side = Vector3.Cross (up, d);
			Quaternion q = Quaternion.AngleAxis(angle, side);
			camera = q*(camera-target) + target;
 
            d = camera - target;
			up = Vector3.Cross (d, side).normalized;
		}
		public void RotateHorisontal(float angle) {
			Vector3 d = camera - target;
			Vector3 side = Vector3.Cross (up, d).normalized;
			Quaternion q = Quaternion.AngleAxis(angle, up);
            camera = q * (camera - target) + target;
            d = camera - target;
			up = Vector3.Cross (d, side).normalized;
		}
		
		public Vector3 coord2ray(float x, float y, float width, float height) {
			
			float aspect_ratio = 1;
			float FOV = perspective / 360.0f * 2 * Mathf.PI; // convert to radians
			float dx=Mathf.Tan(FOV*0.5f)*(x/(width/2)-1.0f)/aspect_ratio;
			float dy=Mathf.Tan(FOV*0.5f)*(1- y/(width/2));
			
			
			float far = 10f;
			
			Vector3 Pfar = new Vector3(dx*far, dy*far, far);
			Vector3 res = viewMatrix*Pfar;
			res = res.normalized;
			
			return res;
		}		
		

	}
}
