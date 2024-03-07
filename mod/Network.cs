using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Colossal.Json;
using ExtraLandscapingTools;
using ExtraLandscapingTools.UI;
using Game.Net;
using Game.Prefabs;
using static ExtraLandscapingTools.Extensions;
namespace ELT_Network
{
	public class Network : Extension
	{
		public override ExtensionType Type => ExtensionType.Assets;
        public override string ExtensionID => MyPluginInfo.PLUGIN_NAME;
        internal NetworkSettings ExtensionSettings;
        public override SettingsUI UISettings => new("ELT Network", [
			new SettingsCheckBox("Show untested object", "elt_networks.showuntestedobject")
		]);

		internal static Network network;

        protected override void OnCreate()
        {	
			network = this;
			ExtensionSettings = LoadSettings( new NetworkSettings() );
            base.OnCreate();
        }

        internal static Stream GetEmbedded(string embeddedPath) {
			return Assembly.GetExecutingAssembly().GetManifestResourceStream($"ELT_Network.embedded.{embeddedPath}");	
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
					prefab is not PathwayPrefab && 
					prefab is not TrackPrefab && 
					prefab is not RoadPrefab &&
					prefab is not MarkerObjectPrefab &&
					prefab is not SpacePrefab
				) {	
					return true;
				}

				if(prefab is MarkerObjectPrefab && prefab.name.ToLower().Contains("invisible")) {
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

				if(prefab is PathwayPrefab) prefabUI.m_Group ??= Prefab.GetExistingToolCategory(prefab, "Pathways");
				else if(prefab is TrackPrefab trainTrackPrefab && trainTrackPrefab.m_TrackType == TrackTypes.Train) prefabUI.m_Group ??= Prefab.GetExistingToolCategory(prefab, "TransportationTrain");
				else if(prefab is TrackPrefab SubwayTrackPrefab && SubwayTrackPrefab.m_TrackType == TrackTypes.Subway) prefabUI.m_Group ??= Prefab.GetExistingToolCategory(prefab, "TransportationSubway");
				else if(prefab is TrackPrefab TramTrackPrefab && TramTrackPrefab.m_TrackType == TrackTypes.Tram) prefabUI.m_Group ??= Prefab.GetExistingToolCategory(prefab, "TransportationTram"); 
				else if(prefab is RoadPrefab roadPrefab) prefabUI.m_Group ??= GetCatUIForRoad(roadPrefab);
				else if(prefab is SpacePrefab) prefabUI.m_Group ??= Prefab.GetOrCreateNewToolCategory(prefab, "Landscaping", "Spaces", "Pathways");
				else if(prefab is MarkerObjectPrefab) prefabUI.m_Group ??= Prefab.GetOrCreateNewToolCategory(prefab, "Landscaping", "Marker Object Prefabs", "Spaces");
				else prefabUI.m_Group ??= Prefab.GetOrCreateNewToolCategory(prefab, "Landscaping", "[ELT - Network]Failed Prefab, IF you see this tab, repport it, it's a bug.");
				
				if(prefabUI.m_Group == null) {
					return false;
				}

			} catch (Exception e) {Plugin.Logger.LogError(e);}

			return base.OnAddPrefab(prefab);
		}

		public override string OnGetIcon(PrefabBase prefab)
		{

			if(File.Exists($"{GameManager_Awake.resourcesIcons}/{prefab.GetType().Name}/{prefab.name}.svg")) return $"{GameManager_InitializeThumbnails.COUIBaseLocation}/resources/Icons/{prefab.GetType().Name}/{prefab.name}.svg";

			if(prefab is PathwayPrefab) {
				// return prefab.name switch
				// {   
				// 	_ => "Media/Game/Icons/Pathways.svg",
				// };
				return null; //$"{GameManager_InitializeThumbnails.COUIBaseLocation}/resources/Icons/Misc/placeholder.svg";
				//return "Media/Game/Icons/Pathways.svg";
			} else if (prefab is TrackPrefab trackPrefab) {
				if(trackPrefab.m_TrackType == Game.Net.TrackTypes.Train) {
					// return prefab.name switch
					// {   
					// 	"" => 
					// 	_ => "Media/Game/Icons/DoubleTrainTrack.svg",
					// };
					return "Media/Game/Icons/DoubleTrainTrack.svg";
				}
				else if(trackPrefab.m_TrackType == Game.Net.TrackTypes.Subway) {
					return prefab.name switch
					{
						"Twoway Subway Track" => "Media/Game/Icons/TwoWayTrainTrack.svg",
						_ => "Media/Game/Icons/DoubleTrainTrack.svg",
					};
					// return "Media/Game/Icons/DoubleTrainTrack.svg";
				}
				else if(trackPrefab.m_TrackType == Game.Net.TrackTypes.Tram) {
					// return prefab.name switch
					// {   
					// 	_ => "Media/Game/Icons/OnewayTramTrack.svg",
					// };
					return "Media/Game/Icons/OnewayTramTrack.svg";
				}
			} else if(prefab is RoadPrefab roadPrefab) {
				return roadPrefab.name switch
				{   
					"Golden Gate Road" => "Media/Game/Icons/LargeRoad.svg",
					_ => null //$"{GameManager_InitializeThumbnails.COUIBaseLocation}/resources/Icons/Misc/placeholder.svg",
				};
			}

			return null;
		}

		internal static UIAssetCategoryPrefab GetCatUIForRoad(RoadPrefab roadPrefab) {
			return roadPrefab.name switch
			{   
				"Golden Gate Road" => Prefab.GetExistingToolCategory(roadPrefab , "RoadsLargeRoads"),
				_ => Prefab.GetOrCreateNewToolCategory(roadPrefab , "Roads", "Hidden Roads")
			};
		}
	}
}