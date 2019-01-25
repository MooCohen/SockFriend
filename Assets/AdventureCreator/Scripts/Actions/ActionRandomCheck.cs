/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionRandomCheck.cs"
 * 
 *	This action checks the value of a random number
 *	and performs different follow-up Actions accordingly.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionRandomCheck : ActionCheckMultiple
	{

		public bool disallowSuccessive = false;
		public bool saveToVariable = true;
		private int ownVarValue = -1;

		public int parameterID = -1;
		public int variableID;
		public VariableLocation location = VariableLocation.Global;


		public ActionRandomCheck ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Variable;
			title = "Check random number";
			description = "Picks a number at random between zero and a specified integer – the value of which determine which subsequent Action is run next.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			variableID = AssignVariableID (parameters, parameterID, variableID);
		}
		
		
		override public ActionEnd End (List<Action> actions)
		{
			if (numSockets <= 0)
			{
				ACDebug.LogWarning ("Could not compute Random check because no values were possible!");
				return GenerateStopActionEnd ();
			}

			GVar linkedVariable = null;
			if (saveToVariable)
			{
				if (location == VariableLocation.Local && !isAssetFile)
				{
					linkedVariable = LocalVariables.GetVariable (variableID);
				}
				else
				{
					linkedVariable = GlobalVariables.GetVariable (variableID);
				}
			}

			int randomResult = Random.Range (0, numSockets);
			if (numSockets > 1 && disallowSuccessive)
			{
				if (saveToVariable)
				{
					if (linkedVariable != null && linkedVariable.type == VariableType.Integer)
					{
						ownVarValue = linkedVariable.val;
					}
					else
					{
						ACDebug.LogWarning ("'Variable: Check random number' Action is referencing a Variable that does not exist or is not an Integer!");
					}
				}

				while (ownVarValue == randomResult)
				{
					randomResult = Random.Range (0, numSockets);
				}

				ownVarValue = randomResult;

				if (saveToVariable && linkedVariable != null && linkedVariable.type == VariableType.Integer)
				{
					linkedVariable.SetValue (ownVarValue);
				}
			}

			return ProcessResult (randomResult, actions);
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			numSockets = EditorGUILayout.DelayedIntField ("# of possible values:", numSockets);
			numSockets = Mathf.Clamp (numSockets, 1, 100);

			disallowSuccessive = EditorGUILayout.Toggle ("Prevent same value twice?", disallowSuccessive);

			if (disallowSuccessive)
			{
				saveToVariable = EditorGUILayout.Toggle ("Save last value?", saveToVariable);
				if (saveToVariable)
				{
					if (isAssetFile)
					{
						location = VariableLocation.Global;
					}
					else
					{
						location = (VariableLocation) EditorGUILayout.EnumPopup ("Variable source:", location);
					}
					
					if (location == VariableLocation.Global)
					{
						if (AdvGame.GetReferences ().variablesManager)
						{
							parameterID = Action.ChooseParameterGUI ("Integer variable:", parameters, parameterID, ParameterType.GlobalVariable);
							if (parameterID >= 0)
							{
								variableID = ShowVarGUI (variableID, false);
							}
							else
							{
								EditorGUILayout.BeginHorizontal ();
								variableID = ShowVarGUI (variableID, true);
								if (GUILayout.Button ("", CustomStyles.IconCog))
								{
									SideMenu ();
								}
								EditorGUILayout.EndHorizontal ();
							}
						}
					}
					else if (location == VariableLocation.Local)
					{
						if (KickStarter.localVariables)
						{
							parameterID = Action.ChooseParameterGUI ("Integer variable:", parameters, parameterID, ParameterType.LocalVariable);
							if (parameterID >= 0)
							{
								variableID = ShowVarGUI (variableID, false);
							}
							else
							{
								EditorGUILayout.BeginHorizontal ();
								variableID = ShowVarGUI (variableID, true);
								if (GUILayout.Button ("", CustomStyles.IconCog))
								{
									SideMenu ();
								}
								EditorGUILayout.EndHorizontal ();
							}
						}
						else
						{
							EditorGUILayout.HelpBox ("No 'Local Variables' component found in the scene. Please add an AC GameEngine object from the Scene Manager.", MessageType.Info);
						}
					}
				}
			}
		}


		private void SideMenu ()
		{
			GenericMenu menu = new GenericMenu ();

			menu.AddItem (new GUIContent ("Auto-create " + location.ToString () + " variable"), false, Callback, "AutoCreate");
			menu.ShowAsContext ();
		}
		
		
		private void Callback (object obj)
		{
			switch (obj.ToString ())
			{
			case "AutoCreate":
				AutoCreateVariableWindow.Init ("Random/New integer", location, VariableType.Integer, this);
				break;

			case "Show":
				if (AdvGame.GetReferences () != null && AdvGame.GetReferences ().variablesManager != null)
				{
					AdvGame.GetReferences ().variablesManager.ShowVariable (variableID, location);
				}
				break;
			}
		}


		private int ShowVarGUI (int ID, bool changeID)
		{
			if (changeID)
			{
				if (location == VariableLocation.Global)
				{
					ID = AdvGame.GlobalVariableGUI ("Global integer:", ID, VariableType.Integer);
				}
				else if (location == VariableLocation.Local)
				{
					ID = AdvGame.LocalVariableGUI ("Local integer:", ID, VariableType.Integer);
				}
			}

			return ID;
		}


		public override bool ConvertLocalVariableToGlobal (int oldLocalID, int newGlobalID)
		{
			bool wasAmended = base.ConvertLocalVariableToGlobal (oldLocalID, newGlobalID);

			if (saveToVariable)
			{
				if (location == VariableLocation.Local && variableID == oldLocalID)
				{
					location = VariableLocation.Global;
					variableID = newGlobalID;
					wasAmended = true;
				}
			}

			return wasAmended;
		}


		public override int GetVariableReferences (List<ActionParameter> parameters, VariableLocation _location, int varID)
		{
			int thisCount = 0;
			if (saveToVariable && location == _location && variableID == varID)
			{
				thisCount ++;
			}
			thisCount += base.GetVariableReferences (parameters, _location, varID);
			return thisCount;
		}
 

		public override bool ConvertGlobalVariableToLocal (int oldGlobalID, int newLocalID, bool isCorrectScene)
		{
			bool isAffected = base.ConvertGlobalVariableToLocal (oldGlobalID, newLocalID, isCorrectScene);

			if (saveToVariable)
			{
				if (location == VariableLocation.Global && variableID == oldGlobalID)
				{
					isAffected = true;

					if (isCorrectScene)
					{
						location = VariableLocation.Local;
						variableID = newLocalID;
					}
				}
			}

			return isAffected;
		}
		
		#endif
		
	}

}