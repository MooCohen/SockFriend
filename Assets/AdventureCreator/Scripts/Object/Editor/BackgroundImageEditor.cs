#if !UNITY_2017_3_OR_NEWER
#define ALLOW_LEGACY_UI
#endif

using UnityEngine;
using UnityEditor;

namespace AC
{

	#if UNITY_STANDALONE && (UNITY_5 || UNITY_2017_1_OR_NEWER || UNITY_PRO_LICENSE)
	[CustomEditor (typeof (BackgroundImage))]
	public class BackgroundImageEditor : Editor
	{
		
		private BackgroundImage _target;
		

		private void OnEnable ()
		{
			_target = (BackgroundImage) target;
		}
		
		
		public override void OnInspectorGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");

			#if ALLOW_LEGACY_UI
			_target.backgroundMethod25D = (BackgroundImage.BackgroundMethod25D) CustomGUILayout.EnumPopup ("Method:", _target.backgroundMethod25D, "", "How 2.5D backgrounds are renderered");
			if (_target.backgroundMethod25D == BackgroundImage.BackgroundMethod25D.GUITexture)
			{
				if (Object.FindObjectOfType <BackgroundImageUI>() != null)
				{
					BackgroundImageUI.Instance.ClearTexture (null);
				}

				if (_target.GUITexture == null)
				{
					EditorGUILayout.HelpBox ("A GUITexture component must be attached to this object for the chosen method to work.", MessageType.Warning);
				}
			}
			else if (_target.backgroundMethod25D == BackgroundImage.BackgroundMethod25D.UnityUI)
			{
			#endif
				_target.backgroundTexture = (Texture) CustomGUILayout.ObjectField <Texture> ("Background texture:", _target.backgroundTexture, false, "", "The texture to display full-screen");
			#if ALLOW_LEGACY_UI
			}
			#endif

			#if UNITY_STANDALONE && (UNITY_5 || UNITY_2017_1_OR_NEWER || UNITY_PRO_LICENSE) && !UNITY_2018_2_OR_NEWER
			EditorGUILayout.LabelField ("When playing a MovieTexture:");
			_target.loopMovie = CustomGUILayout.Toggle ("Loop clip?", _target.loopMovie, "", "If True, then any MovieTexture set as the background will be looped");
			_target.restartMovieWhenTurnOn = CustomGUILayout.Toggle ("Restart clip each time?", _target.restartMovieWhenTurnOn, "", "If True, then any MovieTexture set as the background will start from the beginning when the associated Camera is activated");
			#endif

			EditorGUILayout.EndVertical ();
			UnityVersionHandler.CustomSetDirty (_target);
		}

	}
	#endif

}