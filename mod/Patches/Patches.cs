﻿using HarmonyLib;
using Game.SceneFlow;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Game.UI;
using ExtraLandscapingTools;
using System.IO.Compression;
using Game.Common;
using Game;
using Game.Prefabs;
using System;
using Game.Net;
using System.Diagnostics;
using Colossal.OdinSerializer.Utilities;
using MonoMod.RuntimeDetour;
using Game.Tools;

namespace ELT_EditorTools
{

	[HarmonyPatch(typeof(GameManager), "Awake")]
	internal class GameManager_Awake
	{

		static readonly string pathToZip = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)+"\\resources.zip";
		static internal readonly string resources = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "resources");
		static internal readonly string resourcesIcons = Path.Combine(resources, "Icons");

		static void Prefix(GameManager __instance)
		{	
			Extensions.RegisterELTExtension(new EditorTools());

			if(File.Exists(pathToZip)) {
				if(Directory.Exists(resources)) Directory.Delete(resources, true);
				ZipFile.ExtractToDirectory(pathToZip, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
				File.Delete(pathToZip);
			}

		}
	}

	[HarmonyPatch(typeof(GameManager), "InitializeThumbnails")]
	internal class GameManager_InitializeThumbnails
	{	
		static readonly string IconsResourceKey = $"{MyPluginInfo.PLUGIN_NAME.ToLower()}";

		public static readonly string COUIBaseLocation = $"coui://{IconsResourceKey}";

		static void Prefix(GameManager __instance)
		{
			List<string> pathToIconToLoad = [Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)];

			var gameUIResourceHandler = (GameUIResourceHandler)GameManager.instance.userInterface.view.uiSystem.resourceHandler;
			
			if (gameUIResourceHandler == null)
			{
				UnityEngine.Debug.LogError("Failed retrieving GameManager's GameUIResourceHandler instance, exiting.");
				return;
			}
			
			gameUIResourceHandler.HostLocationsMap.Add(
				IconsResourceKey, pathToIconToLoad
			);
		}
	}

	[HarmonyPatch(typeof(GameModeExtensions), "IsEditor")]
	public class GameModeExtensions_IsEditor
	{
		public static void Postfix(ref bool __result) {

			MethodBase caller = new StackFrame(2, false).GetMethod();
			if((caller.DeclaringType == typeof(NetToolSystem) && caller.Name == "GetNetPrefab")) {
				__result = true;
			}

		}
	}

	// [HarmonyPatch(typeof(SystemOrder), "Initialize")]
	// public static class SystemOrderPatch {
	// 	public static void Postfix(UpdateSystem updateSystem) {
	// 		updateSystem.UpdateAt<UI>(SystemUpdatePhase.UIUpdate);
	// 	}
	// }
}
