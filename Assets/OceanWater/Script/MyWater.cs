using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DSUAssets{

	[ExecuteInEditMode]
	public class MyWater : MonoBehaviour {		
		
		private RenderTexture reflection_tex = null;
		private Camera reflection_cam = null,  main_cam;
		private Transform maincam_t;
		private int  old_size = 256;
		public int tex_size = 256;
		public bool DisablePixelLights = true;
		public float clipplaneoffset = 0.08f;

		// Use this for initialization
		void Start () 
		{			
			tex_size = 256;
			old_size = 256;
		}

		public void OnWillRenderObject()
		{
			if (!enabled || !GetComponent<Renderer>() || !GetComponent<Renderer>().sharedMaterial ||
				!GetComponent<Renderer>().enabled)
			{
				return;
			}

			main_cam = Camera.current;
			if (!main_cam)
				return;
			maincam_t = main_cam.transform;

			if (maincam_t.position.y < transform.position.y)
				return;
			
			int oldcount = QualitySettings.pixelLightCount;
			if(DisablePixelLights)
			{
				QualitySettings.pixelLightCount = 0;
			}
			
				Load ();
			UpdateCamera (ref reflection_cam);

			Vector4 v_side, v_up, v_pos, v_look;
			Matrix4x4 reflection_mat;
			float water_height;

			water_height = transform.position.y;
			float pos_y_inv = -maincam_t.position.y  + (2 * water_height);
			reflection_cam.transform.position = new Vector3 (maincam_t.position.x, pos_y_inv, maincam_t.position.z);

			v_pos = reflection_cam.transform.position;
			v_pos.w = 1;

			Vector3 euler = maincam_t.eulerAngles;
			reflection_cam.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);

			v_side = reflection_cam.transform.right.normalized;
			v_up = reflection_cam.transform.up.normalized * -1f;
			v_look = reflection_cam.transform.forward.normalized;

			reflection_mat = Matrix4x4.zero;
			reflection_mat.SetColumn (0, v_side);
			reflection_mat.SetColumn (1, v_up);
			reflection_mat.SetColumn (2, v_look);
			reflection_mat.SetColumn (3, v_pos);
			reflection_mat = Matrix4x4.Inverse(reflection_mat);

			reflection_mat.m20 *= -1f;
			reflection_mat.m21 *= -1f;
			reflection_mat.m22 *= -1f;
			reflection_mat.m23 *= -1f;
			reflection_cam.worldToCameraMatrix =  reflection_mat;

			Vector4 clip_plane = Calc_plane (transform.up, transform.position, 1f);

			reflection_cam.projectionMatrix = reflection_cam.CalculateObliqueMatrix (clip_plane);
			reflection_cam.cullingMask = ~(1 << 4); 
			reflection_cam.targetTexture = reflection_tex;

			bool temp = GL.invertCulling;
			GL.invertCulling = !temp;
			reflection_cam.Render ();
			reflection_cam.transform.position = main_cam.transform.position;
			GL.invertCulling = temp;
			GetComponent<Renderer>().sharedMaterial.SetTexture("_ReflectionTex", reflection_tex);

			if(DisablePixelLights)
			{
				QualitySettings.pixelLightCount = oldcount;
			}

		}

		void OnDisable()
		{
			DestroyImmediate (reflection_tex);
			reflection_tex = null;
		
		}

		// Update is called once per frame
		void Update () 
		{
			if (!GetComponent<Renderer>() || !GetComponent<Renderer>().sharedMaterial ||
				!GetComponent<Renderer>().enabled)
			{
				return;
			}
			Renderer rend = GetComponent<Renderer> ();	
			float t = Time.timeSinceLevelLoad/10;
			rend.sharedMaterial.SetFloat ("_Timeelapsed", t);

			
		}

		Vector4 Calc_plane(Vector3 plane_norm, Vector3 plane_pos, float clip_side)
		{
			Matrix4x4 mat = reflection_cam.worldToCameraMatrix;
			plane_pos = plane_pos + plane_norm * clipplaneoffset;
			Vector3 cpos = mat.MultiplyPoint (plane_pos);
			Vector3 cnorm = mat.MultiplyVector (plane_norm).normalized * clip_side;

			float distance = -Vector3.Dot (cpos, cnorm);
			return new Vector4 (cnorm.x, cnorm.y, cnorm.z, distance);// Vector3.Dot (cnorm, cpos));
		}

		void Load()
		{
			
			if (!reflection_tex || old_size != tex_size)
			{
				if (reflection_tex)
					DestroyImmediate (reflection_tex);
				reflection_tex = new RenderTexture (tex_size, tex_size, 16);
				reflection_tex.isPowerOfTwo = true;
				reflection_tex.hideFlags = HideFlags.DontSave;
				old_size = tex_size;
			}
			
			if(!reflection_cam)	
			{
				GameObject reflect_go;
				reflect_go = new GameObject ();
				reflect_go.hideFlags = HideFlags.HideAndDontSave;
				reflect_go.AddComponent<Camera>();
				reflect_go.AddComponent<FlareLayer> ();
				reflection_cam = reflect_go.GetComponent<Camera> ();
				reflection_cam.enabled = false;	
				reflection_cam.transform.position = transform.position;
			}
		}

		void UpdateCamera(ref Camera targetcam)
		{
			targetcam.clearFlags = main_cam.clearFlags;
			targetcam.backgroundColor = main_cam.backgroundColor;
			targetcam.aspect = main_cam.aspect;
			targetcam.nearClipPlane = main_cam.nearClipPlane;
			targetcam.farClipPlane = main_cam.farClipPlane;
			targetcam.fieldOfView = main_cam.fieldOfView;
			targetcam.orthographic = main_cam.orthographic;
			targetcam.orthographicSize = main_cam.orthographicSize;
		}
	}
}