/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"Serializer.cs"
 * 
 *	This script serializes saved game data and performs the file handling.
 * 
 * 	It is partially based on Zumwalt's code here:
 * 	http://wiki.unity3d.com/index.php?title=Save_and_Load_from_XML
 *  and uses functions by Nitin Pande:
 *  http://www.eggheadcafe.com/articles/system.xml.xmlserialization.asp 
 * 
 */

#if !UNITY_WEBPLAYER
#define CAN_DELETE
#endif

using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace AC
{

	/**
	 * All of AC's actual file handling, serialising and deserialising is performed within this script.
	 * Its public functions are static, so it does not need to be placed on any scene object.
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_serializer.html")]
	#endif
	public class Serializer : MonoBehaviour
	{

		/**
		 * <summary>Gets a component on a GameObject in the scene that also has a ConstantID component on it.</summary>
		 * <param name = "constantID">The Constant ID number generated by the ConstantID component</param>
		 * <param name = "sceneObject">An object that, if set, must also be in the same scene as the returned component</param>
		 * <returns>The component on the GameObject with a Constant ID number that matches the request. If none is found, returns null.</returns>
		 */
		public static T returnComponent <T> (int constantID, GameObject sceneObject = null) where T : Component
		{
			if (constantID != 0)
			{
				T[] objects = UnityVersionHandler.GetOwnSceneComponents <T> (sceneObject);

				foreach (T _object in objects)
				{
					ConstantID[] idScripts = _object.GetComponents <ConstantID>();
					if (idScripts != null)
					{
						foreach (ConstantID idScript in idScripts)
						{
							if (idScript.constantID == constantID)
							{
								// Found it
								return _object;
							}
						}
					}
				}
			}
			
			return null;
		}


		/**
		 * <summary>Gets a component on a GameObject, or a child of that GameObject, with a given ConstantID.</summary>
		 * <param name = "constantID">The Constant ID number generated by the ConstantID component</param>
		 * <param name = "_gameObject">The GameObject to search</param>
		 * <returns>The component on the GameObject, or a child of that GameObject, with a given ConstantID</returns>
		 */
		public static T GetGameObjectComponent <T> (int constantID, GameObject gameObject) where T : Component
		{
			if (constantID != 0 && gameObject != null)
			{
				T[] objects = gameObject.GetComponentsInChildren <T>();

				foreach (T _object in objects)
				{
					ConstantID[] idScripts = _object.GetComponents <ConstantID>();
					if (idScripts != null)
					{
						foreach (ConstantID idScript in idScripts)
						{
							if (idScript.constantID == constantID)
							{
								// Found it
								return _object;
							}
						}
					}
				}
			}
			return null;
		}


		/**
		 * <summary>Gets all given components on GameObjects that have a given Constant ID.</summary>
		 * <param name = "constantID">The Constant ID number generated by the ConstantID component</param>
		 * <returns>All components on the GameObject with a Constant ID number that matches the request. If none is found, returns null.</returns>
		 */
		public static T[] returnComponents <T> (int constantID, GameObject gameObject = null) where T : Component
		{
			if (constantID != 0)
			{
				List<T> returnObs = new List<T>();
				T[] objects = UnityVersionHandler.GetOwnSceneComponents <T> (gameObject);

				foreach (T _object in objects)
				{
					if (_object.GetComponent <ConstantID>())
					{
						ConstantID[] idScripts = _object.GetComponents <ConstantID>();
						foreach (ConstantID idScript in idScripts)
						{
							if (idScript.constantID == constantID)
							{
								// Found it
								if (!returnObs.Contains (_object))
								{
									returnObs.Add (_object);
								}
							}
						}
					}
				}

				return returnObs.ToArray ();
			}
			
			return null;
		}


		/**
		 * <summary>Gets the Constant ID number of a GameObject, if it has one.</summary>
		 * <param name = "_gameObject">The GameObject to check for</param>
		 * <returns>The Constant ID number (generated by a ConstantID script), if it has one.  Returns 0 otherwise.</returns>
		 */
		public static int GetConstantID (GameObject _gameObject)
		{
			if (_gameObject != null)
			{
				if (_gameObject.GetComponent <ConstantID>())
				{
					if (_gameObject.GetComponent <ConstantID>().constantID != 0)
					{
						return (_gameObject.GetComponent <ConstantID>().constantID);
					}
					else
					{	
						ACDebug.LogWarning ("GameObject " + _gameObject.name + " was not saved because it does not have a Constant ID number.", _gameObject);
					}
				}
				else
				{
					ACDebug.LogWarning ("GameObject " + _gameObject.name + " was not saved because it does not have a 'Constant ID' script - please exit Play mode and attach one to it.", _gameObject);
				}
			}
			return 0;
		}


		/**
		 * <summary>Gets the Constant ID number of a Transform, if it has one.</summary>
		 * <param name = "_transform">The Transform to check for</param>
		 * <returns>The Constant ID number (generated by a ConstantID script), if it has one.  Returns 0 otherwise.</returns>
		 */
		public static int GetConstantID (Transform _transform)
		{
			if (_transform != null)
			{
				if (_transform.GetComponent <ConstantID>())
				{
					if (_transform.GetComponent <ConstantID>().constantID != 0)
					{
						return (_transform.GetComponent <ConstantID>().constantID);
					}
					else
					{	
						ACDebug.LogWarning ("GameObject " + _transform.gameObject.name + " was not saved because it does not have a Constant ID number.", _transform);
					}
				}
				else
				{
					ACDebug.LogWarning ("GameObject " + _transform.gameObject.name + " was not saved because it does not have a 'Constant ID' script - please exit Play mode and attach one to it.", _transform);
				}
			}
			return 0;
		}


		/**
		 * <summary>Serializes an object, either by XML, Binary or Json, depending on the game's iFileFormatHandler.</summary>
		 * <param name = "dataObject">The object to serialize</param>
		 * <param name = "addMethodName">If True, the name of the serialization method (XML, Binary or Json) will be appended to the start of the serialized string. This is useful when the same file is read later by a different serialization method.</param>
		 * <returns>The object, serialized to a string</returns>
		 */
		public static string SerializeObject <T> (object dataObject, bool addMethodName = false)
		{
			iFileFormatHandler fileFormatHandler = SaveSystem.FileFormatHandler;
			string serializedString = SaveSystem.FileFormatHandler.SerializeObject <T> (dataObject);

			if (serializedString != "" && addMethodName)
			{
				serializedString = fileFormatHandler.GetSaveMethod () + serializedString;
			}

			return serializedString;
		}


		/**
		 * <summary>De-serializes an object from a string, according to the game's iFileFormatHandler.</summary>
		 * <param name = "dataString">The object, serialized into a string</param>
		 * <returns>The object, deserialized</returns>
		 */
		public static T DeserializeObject <T> (string dataString)
		{
			iFileFormatHandler fileFormatHandler = SaveSystem.FileFormatHandler;

			if (string.IsNullOrEmpty (dataString))
			{
				return default (T);
			}
			else if (dataString.Contains ("<?xml") || dataString.Contains ("xml version"))
			{
				fileFormatHandler = new FileFormatHandler_Xml ();
			}

			if (dataString.StartsWith (fileFormatHandler.GetSaveMethod ()))
			{
				dataString = dataString.Remove (0, fileFormatHandler.GetSaveMethod ().ToCharArray().Length);
			}

			T result = (T) fileFormatHandler.DeserializeObject <T> (dataString);

			if (result != null && result is T)
			{
				return (T) result;
			}
			return default (T);
		}


		/**
		 * <summary>Converts a compressed string into an array of Paths object's nodes.</summary>
		 * <param name = "path">The Paths object to send the results to</param>
		 * <param name = "pathData">The compressed string</param>
		 * <returns>The Paths object, with the recreated nodes</returns>
		 */
		public static Paths RestorePathData (Paths path, string pathData)
		{
			if (pathData == null)
			{
				return null;
			}
			
			path.affectY = true;
			path.pathType = AC_PathType.ForwardOnly;
			path.nodePause = 0;
			path.nodes = new List<Vector3>();
			
			if (pathData.Length > 0)
			{
				string[] nodesArray = pathData.Split (SaveSystem.pipe[0]);
				
				foreach (string chunk in nodesArray)
				{
					string[] chunkData = chunk.Split (SaveSystem.colon[0]);
					
					float _x = 0;
					float.TryParse (chunkData[0], out _x);
					
					float _y = 0;
					float.TryParse (chunkData[1], out _y);
					
					float _z = 0;
					float.TryParse (chunkData[2], out _z);
					
					path.nodes.Add (new Vector3 (_x, _y, _z));
				}
			}
			
			return path;
		}
		

		/**
		 * <summary>Compresses a Paths object's nodes into a single string, that can be stored in a save file.</summary>
		 * <param name = "path">The Paths object to compress</param>
		 * <returns>The compressed string</returns>
		 */
		public static string CreatePathData (Paths path)
		{
			System.Text.StringBuilder pathString = new System.Text.StringBuilder ();
			
			foreach (Vector3 node in path.nodes)
			{
				pathString.Append (node.x.ToString ());
				pathString.Append (SaveSystem.colon);
				pathString.Append (node.y.ToString ());
				pathString.Append (SaveSystem.colon);
				pathString.Append (node.z.ToString ());
				pathString.Append (SaveSystem.pipe);
			}
			
			if (path.nodes.Count > 0)
			{
				pathString.Remove (pathString.Length-1, 1);
			}
			
			return pathString.ToString ();
		}


		#if UNITY_EDITOR

		public static bool SaveFile (string fullFileName, string _data)
		{
			try
			{
				StreamWriter writer; // = new 
				FileInfo t = new FileInfo (fullFileName);
				
				if (!t.Exists)
				{
					writer = t.CreateText ();
				}
				
				else
				{
					#if CAN_DELETE
					t.Delete ();
					#endif
					writer = t.CreateText ();
				}
				
				writer.Write (_data);
				writer.Close ();

				ACDebug.Log ("File written: " + fullFileName);
			}
			catch (System.Exception e)
 			{
				ACDebug.LogWarning ("Unable to save file '" + fullFileName + "'. Exception: " + e);
				return false;
 			}
			return true;
		}

		#endif


		public static string LoadFile (string fullFilename, bool doLog = true)
		{
			string _data = "";
			
			if (File.Exists (fullFilename))
			{
				StreamReader r = File.OpenText (fullFilename);

				string _info = r.ReadToEnd ();
				r.Close ();
				_data = _info;
			}
			
			if (_data != "" && doLog)
			{
				ACDebug.Log ("File Read: " + fullFilename);
			}
			return (_data);
		}


		/**
		 * <summary>Serializes a Remember script object, either XML, Binary or Json, depending on the platform.</summary>
		 * <param name = "dataObject">The object to serialize</param>
		 * <returns>The object, serialized to a string</returns>
		 */
		public static string SaveScriptData <T> (object dataObject) where T : RememberData
		{
			return SerializeObject <T> (dataObject);
		}


		/**
		 * <summary>De-serializes a RememberData class.</summary>
		 * <param name = "dataString">The RememberData class, serialized as a string</param>
		 * <returns>The de-serialized RememberData class</return>
		 */
		public static T LoadScriptData <T> (string dataString) where T : RememberData
		{
			iFileFormatHandler fileFormatHandler = SaveSystem.FileFormatHandler;
			if (dataString.StartsWith (fileFormatHandler.GetSaveMethod ()))
			{
				dataString = dataString.Remove (0, fileFormatHandler.GetSaveMethod ().ToCharArray().Length);
			}

			return fileFormatHandler.LoadScriptData <T> (dataString);
		}


		/**
		 * <summary>Deserializes a string into an OptionsData class.</summary>
		 * <param name = "dataString">The OptionsData, serialized as a string</param>
		 * <returns>The de-serialized OptionsData class</returns>
		 */
		public static OptionsData DeserializeOptionsData (string dataString)
		{
			iFileFormatHandler fileFormatHandler = SaveSystem.FileFormatHandler;

			if (dataString.StartsWith (fileFormatHandler.GetSaveMethod ()))
			{
				dataString = dataString.Remove (0, fileFormatHandler.GetSaveMethod ().ToCharArray ().Length);
			}
			else
			{
				if (dataString.StartsWith ("XML") || dataString.StartsWith ("Json") || dataString.StartsWith ("Binary"))
				{
					// Switched method, so make new
					return new OptionsData ();
				}
			}
			return (OptionsData) DeserializeObject <OptionsData> (dataString);
		}

	}

}