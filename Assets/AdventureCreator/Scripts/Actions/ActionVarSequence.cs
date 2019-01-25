/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionVarSequence.cs"
 * 
 *	This action runs an Integer Variable through a sequence
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
	public class ActionVarSequence : ActionCheckMultiple
	{
		
		public int parameterID = -1;
		public int variableID;
		public bool doLoop = false;

		public bool saveToVariable = true;
		private int ownVarValue = 0;

		public VariableLocation location = VariableLocation.Global;
		private LocalVariables localVariables;

		
		public ActionVarSequence ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Variable;
			title = "Run sequence";
			description = "Uses the value of an integer Variable to determine which Action is run next. The value is incremented by one each time (and reset to zero when a limit is reached), allowing for different subsequent Actions to play each time the Action is run.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			variableID = AssignVariableID (parameters, parameterID, variableID);
		}


		override public void AssignParentList (ActionList actionList)
		{
			if (actionList != null)
			{
				localVariables = UnityVersionHandler.GetLocalVariablesOfGameObject (actionList.gameObject);
			}
			if (localVariables == null)
			{
				localVariables = KickStarter.localVariables;
			}
		}
		
		
		override public ActionEnd End (List<Action> actions)
		{
			if (numSockets <= 0)
			{
				ACDebug.LogWarning ("Could not compute Random check because no values were possible!");
				return GenerateStopActionEnd ();
			}

			if (!saveToVariable)
			{
				int value = ownVarValue;
				ownVarValue ++;
				if (ownVarValue >= numSockets)
				{
					if (doLoop)
					{
						ownVarValue = 0;
					}
					else
					{
						ownVarValue = numSockets - 1;
					}
				}

				return ProcessResult (value, actions);
			}
			
			if (variableID == -1)
			{
				return GenerateStopActionEnd ();
			}
			
			GVar var = null;
			
			if (location == VariableLocation.Local && !isAssetFile)
			{
				var = LocalVariables.GetVariable (variableID, localVariables);
			}
			else
			{
				var = GlobalVariables.GetVariable (variableID);
			}
			
			if (var != null)
			{
				if (var.type == VariableType.Integer)
				{
					var.Download ();
					if (var.val < 1)
					{
						var.val = 1;
					}
					int originalValue = var.val-1;
					var.val ++;
					if (var.val > numSockets)
					{
						if (doLoop)
						{
							var.val = 1;
						}
						else
						{
							var.val = numSockets;
						}
					}
					var.Upload ();
					return ProcessResult (originalValue, actions);
				}
				else
				{
					ACDebug.LogWarning ("'Variable: Run sequence' Action is referencing a Variable that does not exist or is not an Integer!");
				}
			}
			
			return GenerateStopActionEnd ();
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			numSockets = EditorGUILayout.DelayedIntField ("# of possible values:", numSockets);
			numSockets = Mathf.Clamp (numSockets, 1, 20);
		
			doLoop = EditorGUILayout.Toggle ("Run on a loop?", doLoop);

			saveToVariable = EditorGUILayout.Toggle ("Save sequence value?", saveToVariable);
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
				AutoCreateVariableWindow.Init ("Sequence/New integer", location, VariableType.Integer, this);
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

			if (saveToVariable && location == VariableLocation.Local && variableID == oldLocalID)
			{
				location = VariableLocation.Global;
				variableID = newGlobalID;
				wasAmended = true;
			}
			return wasAmended;
		}


		public override bool ConvertGlobalVariableToLocal (int oldGlobalID, int newLocalID, bool isCorrectScene)
		{
			bool wasAmended = base.ConvertGlobalVariableToLocal (oldGlobalID, newLocalID, isCorrectScene);

			if (saveToVariable && location == VariableLocation.Global && variableID == oldGlobalID)
			{
				wasAmended = true;
				if (isCorrectScene)
				{
					location = VariableLocation.Local;
					variableID = newLocalID;
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

		#endif
		
	}
	
}