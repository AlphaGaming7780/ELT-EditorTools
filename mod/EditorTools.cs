using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Colossal.Json;
using ExtraLandscapingTools;
using Game.Net;
using Game.Prefabs;
using static ExtraLandscapingTools.Extensions;
namespace ELT_EditorTools
{
	public class EditorTools : Extension
	{
		public override ExtensionType Type => ExtensionType.Other;
        public override string ExtensionID => MyPluginInfo.PLUGIN_NAME;
        // internal NetworkSettings ExtensionSettings;
        // public override SettingsUI UISettings => new("ELT Network", [
		// 	new SettingsCheckBox("Show untested object", "elt_netTool.showuntestedobject")
		// ]);

		internal static EditorTools editorTools;

        protected override void OnCreate()
        {	
			editorTools = this;
			// ExtensionSettings = LoadSettings( new NetworkSettings() );
            base.OnCreate();
        }

        internal static Stream GetEmbedded(string embeddedPath) {
			return Assembly.GetExecutingAssembly().GetManifestResourceStream($"ELT_EditorTools.embedded.{embeddedPath}");	
			// return Assembly.GetExecutingAssembly().GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name}.embedded.{embeddedPath}");	
		}

        public override Dictionary<string, Dictionary<string, string>> OnLoadLocalization()
        {
            return Decoder.Decode(new StreamReader(GetEmbedded("Localization.Localization.jsonc")).ReadToEnd()).Make<LocalizationJS>().Localization;
        }

        public override bool OnAddPrefab(PrefabBase prefab)
		{
			try {

				if (
					prefab is not NetLanePrefab ||
					prefab.GetComponent<Game.Prefabs.TrackLane>() != null ||
					prefab.GetComponent<Game.Prefabs.UtilityLane>() != null
				) {	
					return true;
				}

				var prefabUI = prefab.GetComponent<UIObject>();
				if (prefabUI == null)
				{
					prefabUI = prefab.AddComponent<UIObject>();
					prefabUI.active = true;
					prefabUI.m_IsDebugObject = false;
					prefabUI.m_Icon = ELT.GetIcon(prefab);
					prefabUI.m_Priority = 1;
				}

				if(prefab is NetLanePrefab) prefabUI.m_Group ??= Prefab.GetOrCreateNewToolCategory(prefab, "Landscaping", "NetLanePrefab");
				else prefabUI.m_Group ??= Prefab.GetOrCreateNewToolCategory(prefab, "Landscaping", "[ELT - NetTool]Failed Prefab, IF you see this tab, repport it, it's a bug.");
				
				if(prefabUI.m_Group == null) {
					return false;
				}

			} catch (Exception e) {Plugin.Logger.LogError(e);}

			return base.OnAddPrefab(prefab);
		}

		public override string OnGetIcon(PrefabBase prefab)
		{

			if(File.Exists($"{GameManager_Awake.resourcesIcons}/{prefab.GetType().Name}/{prefab.name}.svg")) return $"{GameManager_InitializeThumbnails.COUIBaseLocation}/resources/Icons/{prefab.GetType().Name}/{prefab.name}.svg";

			return null;
		}
	}
}