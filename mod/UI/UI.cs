using System;
using System.Collections.Generic;
using Colossal.UI.Binding;
using ExtraLandscapingTools;
using Game.Prefabs;
using Game.UI;
using Unity.Collections;
using Unity.Entities;
namespace ELT_Network
{
	
	public class UI : UISystemBase
	{	
		private readonly List<string> unTestedPrefabs = ["Seagull Spawner"];
		private static EntityQuery UnTestedPrefabEntityQuery;
        private static GetterValueBinding<bool> showUnTestedObject;
		protected override void OnCreate() {

			base.OnCreate();
			
			AddBinding(showUnTestedObject = new GetterValueBinding<bool>("elt_networks", "showuntestedobject", () => Network.network.ExtensionSettings.ShowUnTestedObject));
			AddBinding(new TriggerBinding<bool>("elt_networks", "showuntestedobject", new Action<bool>(ShowUnTestedObject)));

			UnTestedPrefabEntityQuery = GetEntityQuery(new EntityQueryDesc
			{
				All =
			   [
					ComponentType.ReadOnly<ObjectData>(),
					ComponentType.ReadOnly<PrefabData>(),
					ComponentType.ReadOnly<UIObjectData>()
			   ],
			});

		}

		private void ShowUnTestedObject(bool b) {
			Network.network.ExtensionSettings.ShowUnTestedObject = b;
			Network.network.SaveSettings(Network.network.ExtensionSettings);

			NativeArray<Entity> entities =  UnTestedPrefabEntityQuery.ToEntityArray(AllocatorManager.Temp);

			foreach(Entity entity in entities) {
				if(ELT.m_PrefabSystem.TryGetPrefab(entity, out MarkerObjectPrefab markerObjectPrefab) && markerObjectPrefab is not null) {
					// Plugin.Logger.LogMessage(markerObjectPrefab.name);
					// we can check the name of the prefab we don't want and use the following code on them.
					if(!Network.network.ExtensionSettings.ShowUnTestedObject)
					{
						if (unTestedPrefabs.Contains(markerObjectPrefab.name))
						{
							// Plugin.Logger.LogMessage(markerObjectPrefab.name + " has been removed from UI");
							ELT_UI.RemoveEntityObjectFromCategoryUI(entity);
							continue;
						}
					}
					ELT_UI.AddEntityObjectToCategoryUI(entity);
				}
			}

			ExtraLandscapingTools.Patches.ToolbarUISystemPatch.UpdateMenuUI();

			showUnTestedObject.Update();
		}
	}
}