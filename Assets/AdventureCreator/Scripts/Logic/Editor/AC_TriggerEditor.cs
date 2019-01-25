using UnityEngine;
using UnityEditor;

namespace AC
{

	[CustomEditor(typeof(AC_Trigger))]
	[System.Serializable]
	public class AC_TriggerEditor : ActionListEditor
	{

		public override void OnInspectorGUI ()
		{
			AC_Trigger _target = (AC_Trigger) target;
			PropertiesGUI (_target);
			base.DrawSharedElements (_target);

			UnityVersionHandler.CustomSetDirty (_target);
		}


		public static void PropertiesGUI (AC_Trigger _target)
		{
			string[] Options = { "On enter", "Continuous", "On exit" };

			if (Application.isPlaying)
			{
				if (!_target.IsOn ())
				{
					EditorGUILayout.HelpBox ("Current state: OFF", MessageType.Info);
				}
			}

			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Trigger properties", EditorStyles.boldLabel);
			_target.source = (ActionListSource) CustomGUILayout.EnumPopup ("Actions source:", _target.source, "", "Where the Actions are stored");
			if (_target.source == ActionListSource.AssetFile)
			{
				_target.assetFile = (ActionListAsset) CustomGUILayout.ObjectField <ActionListAsset> ("ActionList asset:", _target.assetFile, false, "", "The ActionList asset that stores the Actions");
				_target.syncParamValues = CustomGUILayout.Toggle ("Sync parameter values?", _target.syncParamValues, "", "If True, the ActionList asset's parameter values will be shared amongst all linked ActionLists");
			}
			_target.actionListType = (ActionListType) CustomGUILayout.EnumPopup ("When running:", _target.actionListType, "", "The effect that running the Actions has on the rest of the game");
			if (_target.actionListType == ActionListType.PauseGameplay)
			{
				_target.isSkippable = CustomGUILayout.Toggle ("Is skippable?", _target.isSkippable, "", "If True, the Actions will be skipped when the user presses the 'EndCutscene' Input button");
			}
			_target.triggerType = CustomGUILayout.Popup ("Trigger type:", _target.triggerType, Options, "", "What kind of contact the Trigger reacts to");
			_target.triggerReacts = (TriggerReacts) CustomGUILayout.EnumPopup ("Reacts:", _target.triggerReacts, "", "The state of the game under which the trigger reacts");
			_target.cancelInteractions = CustomGUILayout.Toggle ("Cancels interactions?", _target.cancelInteractions, "", "If True, and the Player sets off the Trigger while walking towards a Hotspot Interaction, then the Player will stop and the Interaction will be cancelled");
			_target.tagID = ShowTagUI (_target.actions.ToArray (), _target.tagID);
			_target.useParameters = CustomGUILayout.Toggle ("Set collider as parameter?", _target.useParameters, "", "If True, the colliding object will be provided as a GameObject parameter");
			
			EditorGUILayout.Space ();
			_target.detects = (TriggerDetects) CustomGUILayout.EnumPopup ("Trigger detects:", _target.detects, "", "What the Trigger will react to");
			if (_target.detects == TriggerDetects.AnyObjectWithComponent)
			{
				_target.detectComponent = CustomGUILayout.TextField ("Component name:", _target.detectComponent, "", "The component that must be attached to an object for the Trigger to react to");
				EditorGUILayout.HelpBox ("Multiple component names should be separated by a colon ';'", MessageType.Info);
			}
			else if (_target.detects == TriggerDetects.AnyObjectWithTag)
			{
				_target.detectComponent = CustomGUILayout.TextField ("Tag name:", _target.detectComponent, "", "The tag that an object must have for the Trigger to react to");
				EditorGUILayout.HelpBox ("Multiple tags should be separated by a colon ';'", MessageType.Info);
			}
			else if (_target.detects == TriggerDetects.SetObject)
			{
				_target.obToDetect = (GameObject) CustomGUILayout.ObjectField <GameObject> ("Object to detect:", _target.obToDetect, true, "", "The GameObject that the Trigger reacts to");
			}
			EditorGUILayout.EndVertical ();

			if (_target.useParameters)
			{
				if (_target.parameters.Count != 1)
				{
					ActionParameter newParameter = new ActionParameter (0);
					newParameter.parameterType = ParameterType.GameObject;
					newParameter.label = "Collision object";
					_target.parameters.Clear ();
					_target.parameters.Add (newParameter);
				}
			}
	    }

	}

}