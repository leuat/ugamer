using UnityEngine;
using System.Collections;

public class examplescene : MonoBehaviour {

	// Use this for initialization
	void Start () {
        current = (Material)Resources.Load("Skybox2_5/Skybox2_5");
        RenderSettings.skybox = current; 
        GameObject.Find("Main Camera").AddComponent<SmoothMouseLook>();	
	}
	
	// Update is called once per frame
	void Update () {
        if (current == null)
            return;
        Color c = Color.white;
        c.r *= tint;
        c.g *= tint;
        c.b *= tint;

        current.SetColor("_Tint", c);
        current.SetFloat("_Exposure", exposure);
        current.SetFloat("_Rotation", rotation);
    }

    Material current;

    float tint = 0.5f, exposure=1f, rotation;

	void OnGUI() {
		int x = 50;
		int y = 50;
		int dy = 40;
		int cnt = 0;
		int sx = 300;
		int sy = 30;
        for (int i=1;i<=7;i++)
        
		if (GUI.Button(new Rect(x, y+dy*cnt++, sx, sy), "Skybox " +i)) {
                current = (Material)Resources.Load("Skybox2_" + i + "/Skybox2_" + i);
                RenderSettings.skybox = current;
		}

        cnt = 0;
        float ssx = 150;
        GUI.Label(new Rect(x, Screen.height - cnt * dy - y, ssx, dy), "Tint:");
        tint = GUI.HorizontalSlider(new Rect(x + ssx, Screen.height - cnt * dy - y, 2 * sx, dy), tint,0,1);
        cnt++;
        GUI.Label(new Rect(x, Screen.height - cnt * dy - y, ssx, dy), "Exposure:");
        exposure = GUI.HorizontalSlider(new Rect(x + ssx, Screen.height - cnt * dy - y, 2 * sx, dy), exposure, 0, 8);
        cnt++;
        GUI.Label(new Rect(x, Screen.height - cnt * dy - y, ssx, dy), "Rotation:");
        rotation = GUI.HorizontalSlider(new Rect(x + ssx, Screen.height - cnt * dy - y, 2 * sx, dy), rotation, 0, 360);

    }
}
