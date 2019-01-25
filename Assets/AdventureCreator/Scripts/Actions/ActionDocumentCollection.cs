/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionDocumentCollection.cs"
 * 
 *	This action adds or removes a Document active from the player's collection.
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
	public class ActionDocumentCollection : Action
	{

		public int documentID;

		[SerializeField] private DocumentCollectionMethod documentCollectionMethod = DocumentCollectionMethod.Add;
		private enum DocumentCollectionMethod { Add, Remove, Clear };

		
		public ActionDocumentCollection ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Document;
			title = "Add or remove";
			description = "Adds or removes a document from the player's collection, or removes all of them.";
		}


		override public float Run ()
		{
			Document document = KickStarter.inventoryManager.GetDocument (documentID);

			if (document != null)
			{
				if (documentCollectionMethod == DocumentCollectionMethod.Add)
				{
					KickStarter.runtimeDocuments.AddToCollection (document);
				}
				else if (documentCollectionMethod == DocumentCollectionMethod.Remove)
				{
					KickStarter.runtimeDocuments.RemoveFromCollection (document);
				}
				else if (documentCollectionMethod == DocumentCollectionMethod.Clear)
				{
					KickStarter.runtimeDocuments.ClearCollection ();
				}
			}

			return 0f;
		}
		

		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			documentCollectionMethod = (DocumentCollectionMethod) EditorGUILayout.EnumPopup ("Method:", documentCollectionMethod);
			documentID = InventoryManager.DocumentSelectorList (documentID);

			AfterRunningOption ();
		}


		override public string SetLabel ()
		{
			return documentCollectionMethod.ToString ();
		}

		#endif
		
	}

}