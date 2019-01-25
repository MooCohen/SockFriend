/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionDocumentCheck.cs"
 * 
 *	This action checks to see if a Document is being carried
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
	public class ActionDocumentCheck : ActionCheck
	{

		public int documentID;

		
		public ActionDocumentCheck ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Document;
			title = "Check";
			description = "Checks to see if a particular Document is in the Player's possession.";
		}


		public override bool CheckCondition ()
		{
			return KickStarter.runtimeDocuments.DocumentIsInCollection (documentID);
		}


		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			documentID = InventoryManager.DocumentSelectorList (documentID, "Check carrying:");

			AfterRunningOption ();
		}


		override public string SetLabel ()
		{
			Document document = KickStarter.inventoryManager.GetDocument (documentID);
			if (document != null)
			{
				return document.Title;
			}
			return string.Empty;
		}

		#endif
		
	}

}