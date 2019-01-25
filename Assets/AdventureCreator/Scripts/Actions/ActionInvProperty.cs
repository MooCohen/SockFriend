/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionVarProperty.cs"
 * 
 *	This action is used to set the value of a Variable as an Inventory item property.
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
	public class ActionInvProperty : Action
	{
		
		public int varParameterID = -1;
		public int variableID;
		public VariableLocation varLocation;

		#if UNITY_EDITOR
		private VariableType varType = VariableType.Boolean;
		private VariableType propertyType = VariableType.Boolean;
		#endif

		private enum SetVarAsPropertyMethod { SpecificItem, SelectedItem };
		[SerializeField] private SetVarAsPropertyMethod setVarAsPropertyMethod = SetVarAsPropertyMethod.SpecificItem;

		public int invID;
		public int invParameterID;

		public int propertyID;

		private LocalVariables localVariables;
		private InventoryManager inventoryManager;


		public ActionInvProperty ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Inventory;
			title = "Property to Variable";
			description = "Sets the value of a Variable as an Inventory item property.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			invID = AssignInvItemID (parameters, invParameterID, invID);
			variableID = AssignVariableID (parameters, varParameterID, variableID);
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


		override public float Run ()
		{
			GVar myVar = (varLocation == VariableLocation.Global) ?
						GlobalVariables.GetVariable (variableID) :
						LocalVariables.GetVariable (variableID, localVariables);
			
			InvItem invItem = (setVarAsPropertyMethod == SetVarAsPropertyMethod.SelectedItem) ?
						KickStarter.runtimeInventory.LastSelectedItem :
						KickStarter.inventoryManager.GetItem (invID);
			
			if (invItem != null && myVar != null)
			{
				InvVar invVar = invItem.GetProperty (propertyID);

				if (myVar.type == VariableType.String)
				{
					myVar.textVal = invVar.GetDisplayValue (Options.GetLanguage ());
				}
				else if (myVar.type == invVar.type)
				{
					if (invVar.type == VariableType.Float)
					{
						myVar.FloatValue = invVar.FloatValue;
					}
					else if (invVar.type == VariableType.Vector3)
					{
						myVar.Vector3Value = invVar.Vector3Value;
					}
					else
					{
						myVar.IntegerValue = invVar.IntegerValue;
					}
				}
				else
				{
					ACDebug.LogWarning ("Cannot assign " + varLocation.ToString () + " Variable " + myVar.label + "'s value from '" + invVar.label + "' property because their types do not match.");
				}
			}

			return 0;
		}


		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			inventoryManager = AdvGame.GetReferences ().inventoryManager;

			// Select Inventory item

			setVarAsPropertyMethod = (SetVarAsPropertyMethod) EditorGUILayout.EnumPopup ("Get property of:", setVarAsPropertyMethod);
			if (setVarAsPropertyMethod == SetVarAsPropertyMethod.SpecificItem)
			{
				ShowInvSelectGUI (parameters);
			}

			// Select item property
			ShowInvPropertyGUI ();

			EditorGUILayout.Space ();

			// Select variable
			if (isAssetFile)
			{
				varLocation = VariableLocation.Global;
			}
			else
			{
				varLocation = (VariableLocation) EditorGUILayout.EnumPopup ("Variable source:", varLocation);
			}
			
			if (varLocation == VariableLocation.Global)
			{
				if (AdvGame.GetReferences ().variablesManager)
				{
					ShowVarGUI (AdvGame.GetReferences ().variablesManager.vars, parameters, ParameterType.GlobalVariable);
				}
			}
			else if (varLocation == VariableLocation.Local)
			{
				if (localVariables)
				{
					ShowVarGUI (localVariables.localVars, parameters, ParameterType.LocalVariable);
				}
				else
				{
					EditorGUILayout.HelpBox ("No 'Local Variables' component found in the scene. Please add an AC GameEngine object from the Scene Manager.", MessageType.Info);
				}
			}

			if (inventoryManager != null && inventoryManager.invVars != null && inventoryManager.invVars.Count > 0)
			{
				if (varType != propertyType && varType != VariableType.String)
				{
					EditorGUILayout.HelpBox ("The chosen Inventory Item Property and Variable must share the same Type, or the Variable must be a String.", MessageType.Warning);
				}
			}

			AfterRunningOption ();
		}


		private void ShowInvPropertyGUI ()
		{
			if (inventoryManager != null)
			{
				// Create a string List of the field's names (for the PopUp box)
				List<string> labelList = new List<string>();

				int i = 0;
				int propertyNumber = 0;

				if (inventoryManager.invVars != null && inventoryManager.invVars.Count > 0)
				{
					foreach (InvVar invVar in inventoryManager.invVars)
					{
						labelList.Add (invVar.label);
						
						// If a property has been removed, make sure selected variable is still valid
						if (invVar.id == propertyID)
						{
							propertyNumber = i;
						}
						
						i++;
					}
					
					if (propertyNumber == -1)
					{
						// Wasn't found (property was possibly deleted), so revert to zero
						ACDebug.LogWarning ("Previously chosen property no longer exists!");
						
						propertyNumber = 0;
						propertyID = 0;
					}

					propertyNumber = EditorGUILayout.Popup ("Inventory property:", propertyNumber, labelList.ToArray());
					propertyID = inventoryManager.invVars[propertyNumber].id;
					propertyType = inventoryManager.invVars[propertyNumber].type;
				}
				else
				{
					EditorGUILayout.HelpBox ("No inventory properties exist!", MessageType.Info);
					propertyID = -1;
					propertyNumber = -1;
				}
			}
		}


		private void ShowInvSelectGUI (List<ActionParameter> parameters)
		{
			if (inventoryManager != null)
			{
				// Create a string List of the field's names (for the PopUp box)
				List<string> labelList = new List<string>();
				
				int i = 0;
				int invNumber = 0;
				if (invParameterID == -1)
				{
					invNumber = -1;
				}
				
				if (inventoryManager.items.Count > 0)
				{
					foreach (InvItem _item in inventoryManager.items)
					{
						labelList.Add (_item.label);
						
						// If an item has been removed, make sure selected variable is still valid
						if (_item.id == invID)
						{
							invNumber = i;
						}
						
						i++;
					}
					
					if (invNumber == -1)
					{
						// Wasn't found (item was possibly deleted), so revert to zero
						ACDebug.LogWarning ("Previously chosen item no longer exists!");
						
						invNumber = 0;
						invID = 0;
					}

					invParameterID = Action.ChooseParameterGUI ("Inventory item:", parameters, invParameterID, ParameterType.InventoryItem);
					if (invParameterID >= 0)
					{
						invNumber = Mathf.Min (invNumber, inventoryManager.items.Count-1);
						invID = -1;
					}
					else
					{
						invNumber = EditorGUILayout.Popup ("Inventory item:", invNumber, labelList.ToArray());
						invID = inventoryManager.items[invNumber].id;
					}
				}
				else
				{
					EditorGUILayout.HelpBox ("No inventory items exist!", MessageType.Info);
					invID = -1;
					invNumber = -1;
				}
			}
		}


		private void ShowVarGUI (List<GVar> vars, List<ActionParameter> parameters, ParameterType parameterType)
		{
			// Create a string List of the field's names (for the PopUp box)
			List<string> labelList = new List<string>();
			
			int i = 0;
			int variableNumber = -1;

			if (vars.Count > 0)
			{
				foreach (GVar _var in vars)
				{
					labelList.Add (_var.label);
					
					// If a GlobalVar variable has been removed, make sure selected variable is still valid
					if (_var.id == variableID)
					{
						variableNumber = i;
					}
					
					i ++;
				}
				
				if (variableNumber == -1 && (parameters == null || parameters.Count == 0 || varParameterID == -1))
				{
					// Wasn't found (variable was deleted?), so revert to zero
					ACDebug.LogWarning ("Previously chosen variable no longer exists!");
					variableNumber = 0;
					variableID = 0;
				}

				string label = varLocation.ToString () + " variable:";
				varParameterID = Action.ChooseParameterGUI (label, parameters, varParameterID, parameterType);
				if (varParameterID >= 0)
				{
					variableNumber = Mathf.Min (variableNumber, vars.Count-1);
					variableID = -1;
				}
				else
				{
					variableNumber = EditorGUILayout.Popup (label, variableNumber, labelList.ToArray());
					variableID = vars [variableNumber].id;
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("No variables exist!", MessageType.Info);
				variableID = -1;
				variableNumber = -1;
			}

			if (variableNumber >= 0)
			{
				varType = vars[variableNumber].type;
			}
		}


		override public string SetLabel ()
		{
			if (varLocation == VariableLocation.Local && !isAssetFile)
			{
				if (localVariables)
				{
					return GetLabelString (localVariables.localVars, variableID);
				}
			}
			else
			{
				if (AdvGame.GetReferences ().variablesManager)
				{
					return GetLabelString (AdvGame.GetReferences ().variablesManager.vars, variableID);
				}
			}
			
			return string.Empty;
		}


		private string GetLabelString (List<GVar> vars, int variableNumber)
		{
			if (vars.Count > 0 && variableNumber > -1 && vars.Count > variableNumber)
			{
				return vars [variableNumber].label;
			}
			return string.Empty;
		}


		public override bool ConvertLocalVariableToGlobal (int oldLocalID, int newGlobalID)
		{
			bool wasAmended = base.ConvertLocalVariableToGlobal (oldLocalID, newGlobalID);

			if (varLocation == VariableLocation.Local && variableID == oldLocalID)
			{
				varLocation = VariableLocation.Global;
				variableID = newGlobalID;
				wasAmended = true;
			}

			return wasAmended;
		}


		public override bool ConvertGlobalVariableToLocal (int oldGlobalID, int newLocalID, bool isCorrectScene)
		{
			bool wasAmended = base.ConvertGlobalVariableToLocal (oldGlobalID, newLocalID, isCorrectScene);

			if (varLocation == VariableLocation.Global && variableID == oldGlobalID)
			{
				wasAmended = true;
				if (isCorrectScene)
				{
					varLocation = VariableLocation.Local;
					variableID = newLocalID;
				}
			}

			return wasAmended;
		}


		public override int GetVariableReferences (List<ActionParameter> parameters, VariableLocation _location, int varID)
		{
			int thisCount = 0;

			if (varLocation == _location && variableID == varID)
			{
				thisCount ++;
			}

			thisCount += base.GetVariableReferences (parameters, _location, varID);
			return thisCount;
		}

		#endif

	}

}