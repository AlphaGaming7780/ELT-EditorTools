using System;
using Game.Prefabs;
using Game.Tools;
using HarmonyLib;

namespace ELT_EditorTools;

class AreaToolSystemPatch
{
	[HarmonyPatch(typeof(AreaToolSystem), nameof(AreaToolSystem.GetAvailableSnapMask),
		new Type[] { typeof(AreaGeometryData), typeof(bool), typeof(Snap), typeof(Snap) },
		new ArgumentType[] {ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out })]
	class AreaToolSystem_GetAvailableSnapMask
	{
		private static bool Prefix(AreaGeometryData prefabAreaData, bool editorMode, out Snap onMask, out Snap offMask) {

			onMask = (Snap.ExistingGeometry | Snap.StraightDirection);
			offMask = onMask;
			switch (prefabAreaData.m_Type)
			{
			case Game.Areas.AreaType.Lot:
				onMask |= Snap.NetSide | Snap.ObjectSide | Snap.LotGrid;
				offMask |= Snap.NetSide | Snap.ObjectSide | Snap.LotGrid;
				if (editorMode)
				{
					onMask |= Snap.AutoParent;
					offMask |= Snap.AutoParent;
					return false;
				}
				break;
			case Game.Areas.AreaType.District:
				onMask |= Snap.NetMiddle;
				offMask |= Snap.NetMiddle;
				return false;
			case Game.Areas.AreaType.MapTile:
				break;
			case Game.Areas.AreaType.Space:
				onMask |= Snap.NetSide | Snap.ObjectSide | Snap.ObjectSurface | Snap.LotGrid;
				offMask |= Snap.NetSide | Snap.ObjectSide | Snap.ObjectSurface | Snap.LotGrid;
				if (editorMode)
				{
					onMask |= Snap.AutoParent;
					offMask |= Snap.AutoParent;
					return false;
				}
				break;
			case Game.Areas.AreaType.Surface:
				onMask |= Snap.NetSide | Snap.ObjectSide | Snap.LotGrid;
				offMask |= Snap.NetSide | Snap.ObjectSide | Snap.LotGrid;
				if (editorMode)
				{
					onMask |= Snap.AutoParent;
					offMask |= Snap.AutoParent;
				}
				break;
			default:
				return false;
			}
			return false;
		}
	}


}