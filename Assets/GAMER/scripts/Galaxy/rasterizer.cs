using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.UI;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;

namespace LemonSpawn.Gamer {

	public class ThreadRenderState {
		public int from, to;
		public int current;
		public bool trsDone = false;
		public List<GalaxyInstance> galaxies;
	}



	public class Rasterizer : ThreadQueue {

		public List<GalaxyInstance> galaxies = new List<GalaxyInstance>();
			// All spectra
	//		CSpectra spectra;
			// Rendering parameters. Duh.
		public RenderingParams RP = new RenderingParams();
		public RenderingParams RPold = new RenderingParams();
		public int[] renderList;
		public enum RenderState {Idle, Performing, Postprocessing, RequestCancel, Cleanup};
		public static RenderState currentState = RenderState.Idle;
		
		public static List<Task> taskList = new List<Task>();
		public static Task currentTask = null;
        public GamerCamera resetCamera = null;

        Stopwatch stopWatch = new Stopwatch();
        public static bool lowlevel = true;

		Material material;
		public ColorBuffer2D buffer;
		ColorBuffer2D stars;
		const int maxTimer = 1;
		int timer = maxTimer;
		ThreadRenderState[]  threadRenderStates;
		public float time;
		
		public void ClearBuffers() {
			buffer.Clear();
		}
		
/*		private bool galaxySort (CGalaxy* a,CGalaxy* b) { 
			return ((a->params.position-a->params.camera).Length()> (b->params.position-b->params.camera).Length());
		}
*/		

		private void prepareRenderList() {
//            if (renderList == null || renderList.Length != RP.size * RP.size)
            {
                renderList = new int[RP.size * RP.size];
                for (int i = 0; i < RP.size * RP.size; i++)
                    renderList[i] = i;
                renderList = Util.ShuffleArray(renderList);
            }
				
		}
		
        public void Abort()
        {
            currentState = Rasterizer.RenderState.RequestCancel;
            AbortAll();
        }
        public void setNewSize(int s)
        {
            if (s == RP.size)
                return;

            Abort();
            RP.size = s;
            prepareBuffer();
            AssembleImage();
        }

		public void SetupStars() {


          
			if (stars!=null && RPold.noStars == RP.noStars && RPold.starStrength == RP.starStrength && RPold.starSize == RP.starSize && RP.starSizeSpread == RPold.starSizeSpread)
				if (RP.size == stars._width)
					return;
		
			stars = new ColorBuffer2D(RP.size, RP.size);
			stars.RenderStars(RP.noStars, RP.starSize/100f, RP.starSizeSpread/100f,RP.starStrength);
			RPold.starSizeSpread = RP.starSizeSpread;
			RPold.starSize = RP.starSize;
			RPold.noStars = RP.noStars;
			RPold.starStrength = RP.starStrength;
			
		}

        public void prepareBuffer()
        {
            if (buffer == null || buffer._width != RP.size)
            {
                buffer = new ColorBuffer2D(RP.size, RP.size);
                SetupStars();
                buffer.Clear();
            }

        }


        public void Prepare() {
			//sort (galaxies.begin(), galaxies.end(), galaxySort);
			
			galaxies.Sort(
				delegate(GalaxyInstance i1, GalaxyInstance i2) 
				{ 
				float d = (i1.position-RP.camera.camera).magnitude - (i2.position-RP.camera.camera).magnitude;
				if (d<0) return 1;
				else return -1;
//				return (i1.param.position-RP.camera.camera).magnitude> (i2.param.position-RP.camera.camera).magnitude;
				}
			);
            //            buffer = new ColorBuffer2D(RP.size, RP.size);

            prepareBuffer();
			buffer.reference.Set(0);
            prepareRenderList();

            if (threadRenderStates != null)
//            foreach (ThreadRenderState trs in threadRenderStates)
  //              UnityEngine.Debug.Log(trs.trsDone);


			foreach (GalaxyInstance g in galaxies)
				g.GetGalaxy().SetupSpectra();
            time = 0;
            stopWatch.Reset();
            stopWatch.Start();

            RP.camera.setupViewmatrix();
		}
		

		public Galaxy AddGalaxy(string file, Vector3 position, Vector3 orientation, float iscale, float redshift, string name) {
			Galaxy g = Galaxy.Load(file, name);//  new CGalaxy();
			if (g!=null)
				galaxies.Add ( new GalaxyInstance(g, name, position, orientation, iscale, redshift)  );
			return g;
		}
		
		public Galaxy AddGalaxy(GalaxyInstance gi) {
			galaxies.Add (gi);
			return gi.GetGalaxy();
		}
		
		public static void SaveGalaxyList(string filename, List<GalaxyInstance> gi) {
			XmlSerializer serializer = new XmlSerializer(typeof(List<GalaxyInstance>));
			TextWriter textWriter = new StreamWriter(filename);
			serializer.Serialize(textWriter, gi);
			textWriter.Close();
			
		}
		public static List<GalaxyInstance> LoadGalaxyList(string filename) {
			XmlSerializer deserializer = new XmlSerializer(typeof(List<GalaxyInstance>));
         //   UnityEngine.Debug.Log("Loading galaxy list: " + filename);
			TextReader textReader = new StreamReader(filename);
			List<GalaxyInstance> g = (List<GalaxyInstance>)deserializer.Deserialize(textReader);
			textReader.Close();
			foreach (GalaxyInstance gi in g) {
                UnityEngine.Debug.Log(Settings.GalaxyDirectory + gi.galaxyName + ".xml");
                Galaxy gx = Galaxy.Load(Settings.GalaxyDirectory + gi.galaxyName + ".xml", gi.galaxyName);
                UnityEngine.Debug.Log(gx);
                gi.SetGalaxy(gx);
				gi.GetGalaxy().displayName = gi.galaxyName;
			}
			return g;
		}
		
		
		
		private void ThreadRenderPixels(ThreadRenderState trs) {
            threadDone = false;
            trs.trsDone = false;
			// Clone galaxies
			trs.current = 0;
			for (int k=trs.from;k<trs.to;k++) {
				int idx = renderList[ k ];
				Vector3 dir = setupCamera(idx);
				RasterPixel rp = renderPixel(dir, trs.galaxies);
                //rp.I.Set (1,1,0);
                //buffer.SetFill (idx, rp);
//                rp.I.x = 500;
                buffer.SetSingle(idx, rp);
                if (currentState == RenderState.RequestCancel) {
                    threadDone = true;
                    trs.trsDone = true;
                    return;
				}
				trs.current++;
			}
            threadDone = true;
            trs.trsDone = true;

        }

        public float GetPercentage() {
			int cur = 0;
			if (threadRenderStates == null)
				return 100;
			for (int i=0; i<threadRenderStates.Length;i++) 
				cur+=threadRenderStates[i].current;
			return 100f * (float)cur / (RP.size*RP.size);
		}
		
		
		public void GenerateGalaxyList(int N, float size, string[] galaxies) {
			for (int i=0;i<N;i++) {
				Vector3 p = new Vector3();
				p.x = (float)((Util.rnd.NextDouble()-0.5)*size);
				p.y = (float)((Util.rnd.NextDouble()-0.5)*size);
				p.z = (float)((Util.rnd.NextDouble()-0.5)*size);
				Vector4 orient = new Vector3();
				orient.x = (float)((Util.rnd.NextDouble()-0.5));
				orient.y = (float)((Util.rnd.NextDouble()-0.5));
				orient.z = (float)((Util.rnd.NextDouble()-0.5));
				orient = orient.normalized;
				string n = galaxies[Util.rnd.Next()%galaxies.Length];
				AddGalaxy(Settings.GalaxyDirectory + n  + ".xml", p, orient, 0.5f + (float)Util.rnd.NextDouble(), 1, n);
					
			}			
		
		}
		
		
		public void AssembleImage() {
			if (buffer==null)
				return;
				
			float gamma = RP.gamma;
			
//			if (!RP.continuousPostprocessing)
	//			gamma = -100;


			buffer.CreateColorBuffer(RP.exposure, gamma, RP.color, RP.saturation, stars);
/*			if (stars!=null)
				buffer.Add (stars);
*/				
			buffer.Assemble();			
			material.mainTexture = buffer.image;
			
		}
		
		
		public override void PostThread() {
		}
		
		public void Render() {
            Prepare();
            InitializeRendering(gamer.material);
		}
		
		
		public void RenderSkybox() {
		
			Vector3[] planes = new Vector3[6];
			Vector3[] ups = new Vector3[6];
			planes[0] = new Vector3(0,0,-1);
			planes[1] = new Vector3(0,0,1);
			planes[2] = new Vector3(0,1,0);
			planes[3] = new Vector3(0,-1,0);
			planes[4] = new Vector3(1,0,0);
			planes[5] = new Vector3(-1,0,0);
			
			ups[0] = new Vector3(0,-1f,0);
			ups[1] = new Vector3(0,-1f,0);
			ups[2] = new Vector3(0,0,1);
			ups[3] = new Vector3(0,0,-1);
			ups[4] = new Vector3(0,-1f,0);
			ups[5] = new Vector3(0,-1f,0);
			
			string[] names = new string[6];
			names[0] ="Z-";
			names[1] ="Z+";
			names[2] ="Y+";
			names[3] ="Y-";
			names[4] ="X-";
			names[5] ="X+";

            resetCamera = RP.camera.copy();
            
			time = 0;
			currentState = RenderState.RequestCancel;
			//			currentTask = null;
			Abort(); // threads
            //RP.camera.setRotation(new Vector3(0, 45, 45));
            RP.camera.setRotMatrix(resetCamera.GetRotationMatrix());
            //UnityEngine.Debug.Log(RP.camera.rotMatrix);
            

			for (int i=0;i<6;i++) {
				RenderImageSaveTask r = new RenderImageSaveTask("skybox" + names[i] + ".png", this);
                
				r.camera = resetCamera.camera;
				r.target = resetCamera.camera + planes[i];
				r.up =  ups[i];

				r.FOV = 90;
				taskList.Add (r);
			}
           
            //			RP.camera = oldCam;
           

            currentState = RenderState.Idle;
		}
		
		
		public void InitializeRendering(Material mat) {
			material = mat;
			time = 0;
//			currentTask = null;
		    
			int N = RP.no_procs;
		    float T = RP.size*RP.size/(float)N;
			currentState = RenderState.Performing;
			if (threadRenderStates == null || threadRenderStates.Length != N)		
				threadRenderStates = new ThreadRenderState[N];




            //    abort = false;

            //	    if (threadQueue.Count==0)	
			for (int k=0;k<N;k++) {
				ThreadRenderState trs = new ThreadRenderState();
				threadRenderStates[k] = trs;
				trs.from = (int)(k*T);
				trs.to = (int)((k+1)*T);
				
				// setup galaxy list (clone)
				List<GalaxyInstance> gals = new List<GalaxyInstance>();
				foreach (GalaxyInstance g in galaxies) 
					gals.Add (g.Clone());
				trs.galaxies = gals;		
				
				TQueue tq = new TQueue();
				tq.thread = new Thread(() => ThreadRenderPixels(trs));
				tq.gt = this;

				threadQueue.Add (tq);	
			}
            

		}
		
		public void UpdateRendering() {


            if (currentState == RenderState.Performing)
            {

                timer--;
                if (timer <= 0)
                {
                    timer = 3 * maxTimer * Mathf.Max(RP.size / 512, 1);
                    if (RP.continuousPostprocessing)
                        AssembleImage();
                }
                bool done = true;
                for (int i = 0; i < threadRenderStates.Length; i++)
                    if (threadRenderStates[i].trsDone == false)
                        done = false;

                if (done)
                {
//                    Thread.Sleep(100);
                    AssembleImage();
              //      UnityEngine.Debug.Log("DONE");
                    if (currentTask != null)
                    {
                        currentTask.PostTask();
                        taskList.Remove(currentTask);
                    }
                    currentTask = null;

                    currentState = RenderState.Idle;
                    threadRenderStates = null;
                    // Reset camera after task list is done
                    if (taskList.Count == 0 && resetCamera != null)
                    {
                        RP.camera = resetCamera;
                        resetCamera = null;

                    }
                }

                //   if (currentState == RenderState.Performing)
                time = stopWatch.ElapsedMilliseconds;

            }
            if (currentState == RenderState.Idle)
            {
                if (taskList.Count > 0)
                {
                    currentTask = taskList[0];
                    currentTask.Perform();
                    Prepare();
                    InitializeRendering(gamer.material);
                    stars = null;
                    SetupStars();

                }
            }

        }

        public bool isIdle() {
			return currentState != RenderState.Performing; //currentState == RenderState.Idle && currentThreads.Count==0;
		}

		Vector3 setupCamera(int idx) {
			// Converts from index to 2D (i,j) coordinates
			int i = idx%(int)RP.size;
			int j = (idx-i)/(int)RP.size;
//            int j = (int)Mathf.Floor((idx - i) / RP.size);
        
            Vector3 p = new Vector3(i,j,0);
//            UnityEngine.Debug.Log(idx + " : " + p);
			// Projects 2D screen coordinates through camera to 3D ray
			return RP.camera.coord2ray(p.x, p.y, RP.size, RP.size)*-1;
//			Debug.Log (RP.direction);
		}
		
		
		
		RasterPixel renderPixel(Vector3 dir, List<GalaxyInstance> gals) {
			Vector3 isp1, isp2;
			RasterPixel rp = new RasterPixel();
			for (int i=0;i<gals.Count;i++) {

				GalaxyInstance gi = gals[i];
												
				Galaxy g = gi.GetGalaxy();
				float t1, t2;
				bool intersects = Util.IntersectSphere(RP.camera.camera-gi.position, dir, g.param.axis, out isp1, out isp2, out t1, out t2);
				//if (intersects) 
				//	Debug.Log (intersects);				
				if (t1<0) {
					isp2 = RP.camera.camera-gi.position;// + RP.direction*
				}
				if (t1>0 && t2>0)
					intersects = false;
				if (intersects) {
					getIntensity(gi, rp, isp1, isp2, RP.camera.camera);
				}
			}
			return rp;
		}

        /*
 * Loops through galaxy components and integrates the intensity
 *
*/

        /*
 * Calculates number of steps N to integrate a ray through a galaxy. Calls getIntensityAtPoint to combine intensities.
 *
 *
*/
        void getIntensityOrg(GalaxyInstance gi, RasterPixel rp, Vector3 isp1, Vector3 isp2)
        {
            Vector3 origin = isp1;
            float length = (isp1 - isp2).magnitude;
            Vector3 dir = (isp1 - isp2).normalized;
            Galaxy g = gi.GetGalaxy();
            float step = RP.rayStep;
            if (lowlevel)
                step = 0.1f;
            int N = (int)(length / step);
            Vector3 p = origin;
            //Debug.Log (N);
            //		RasterPixel rrp = new RasterPixel(rp);
            rp.scale = step;

            for (int i = 0; i < N; i++)
            {
                //Debug.Log (p.magnitude);
                for (int j = 0; j < g.getComponents().Count; j++)
                {
                    GalaxyComponent gc = (g.getComponents())[j];
                    //if (p.magnitude<0.1)
                    //	rp.I = 1;//1f/Mathf.Pow (p.magnitude,2);



                    if (gc.componentParams.active == 1)
                        gc.calculateIntensity(rp, p, gi, 1);
                    //rp.I.Set (1,1,1);
                }
                // Propagate ray
                p = p - dir * step;
                //				Debug.Log ( "  : " + p);
                // Negative intensities should never happen. But just to be sure.
                rp.Floor(0);
            }
        }
        void getIntensity(GalaxyInstance gi, RasterPixel rp, Vector3 isp1, Vector3 isp2, Vector3 camera) {
			Vector3 origin = isp1;
			float length = (isp1-isp2).magnitude;
			Vector3 dir = (isp1-isp2).normalized;
			Galaxy g = gi.GetGalaxy();
            float step = RP.rayStep;
//            int N = (int)(length/step);
			Vector3 p = origin;
			rp.scale = step;

            //for (int i=0;i<N;i++)
            int n = 0;
            while(Vector3.Dot(p-origin,(isp2-origin).normalized)<length)
            {
                step = Mathf.Clamp((p-camera).magnitude*0.01f, 0.001f, 0.01f);

//                step = RP.rayStep;
				for (int j=0;j<g.getComponents().Count;j++) {
					GalaxyComponent gc = (g.getComponents())[j];
					if (gc.componentParams.active==1)
						gc.calculateIntensity( rp, p, gi, step*200);
				}
				p=p-dir*step;
				rp.Floor(0);
                n++;
			}
            //UnityEngine.Debug.Log(n);
		}
		
		
		
		
	}

}