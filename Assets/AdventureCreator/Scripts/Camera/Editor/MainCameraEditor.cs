#if !UNITY_5_0 && (UNITY_5 || UNITY_2017) && (UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS)
#define ALLOW_VR
#endif

using UnityEngine;
using UnityEditor;

#if ALLOW_VR
using UnityEngine.VR;
#endif

namespace AC
{

	[CustomEditor(typeof(MainCamera))]
	public class MainCameraEditor : Editor
	{
		
		public override void OnInspectorGUI()
		{
			MainCamera _target = (MainCamera) target;

			EditorGUILayout.BeginVertical ("Button");
			_target.fadeTexture = (Texture2D) CustomGUILayout.ObjectField <Texture2D> ("Fade texture:", _target.fadeTexture, false, "", "The texture to display fullscreen when fading");

			#if ALLOW_VR
			if (PlayerSettings.virtualRealitySupported)
			{
				_target.restoreTransformOnLoadVR = CustomGUILayout.ToggleLeft ("Restore transform when loading?", _target.restoreTransformOnLoadVR, "", "If True, the camera's position and rotation will be restored when loading (Hand for VR)");
			}
			#endif

			EditorGUILayout.EndVertical ();

			if (Application.isPlaying)
			{
				EditorGUILayout.BeginVertical ("Button");
				if (_target.attachedCamera)
				{
					_target.attachedCamera = (_Camera) CustomGUILayout.ObjectField <_Camera> ("Attached camera:", _target.attachedCamera, true, "", "The current active camera, i.e. the one that the MainCamera is attaching itself to");
				}
				else
				{
					EditorGUILayout.LabelField ("Attached camera: None");
				}
				EditorGUILayout.EndVertical ();
			}

			UnityVersionHandler.CustomSetDirty (_target);
		}

	}

}