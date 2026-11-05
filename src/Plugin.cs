// ---------------------------------------------------------------
//  Kein Schmerzmittel-Filter (No Painkiller Desaturation)
//
//  Nimmt Schmerzmitteln die grauen, entsaettigten Farben. Das
//  Schmerzmittel selbst wirkt unveraendert.
//
//  WO DER EFFEKT ENTSTEHT: EffectsController haengt an der Kamera
//  und hoert nur zu, welche Gesundheits-Effekte starten und enden.
//  Fuer IPainKiller legt Init() einen eigenen Sammler an:
//
//      new DesaturateMaskAccumulator(this, cc_Sharpen_0, typeof(IPainKiller))
//
//  Solange ein Schmerzmittel laeuft, schiebt dessen UpdateAmount()
//  jedes Bild MaskDesaturate Richtung 1 (0,5 pro Sekunde) und den
//  Radius der Maske von 1 auf 0,325 (0,2 pro Sekunde). CC_Sharpen
//  bzw. DesaturateEffect reichen beides als _desatMask/_radius an
//  den Shader weiter. Niemand sonst schreibt MaskDesaturate, und
//  kein anderer Gesundheits-Effekt laeuft ueber diesen Sammler.
//
//  WAS HIER PASSIERT: Ein Prefix vor UpdateAmount() biegt nur die
//  beiden Zielwerte auf den Ruhezustand um, also auf das, was der
//  Sammler auch ohne Schmerzmittel anstrebt. Die Bewegung dorthin
//  macht das Spiel selbst, darum blendet das Einschalten mitten im
//  Raid weich aus und nichts bleibt haengen.
//
//  WARUM NICHT WEITER OBEN:
//  - Effekt nicht starten (ActiveHealthController): dann gaebe es
//    keine Schmerzfreiheit.
//  - EffectsController.OnEffectStarted abfangen: dann fehlt der
//    Effekt auch im CC_RadialBlurAccumulator, und der ist es, der
//    unter Schmerzmittel den Schmerz-Weichzeichner abschaltet.
//  - DesaturateMaskAccumulator.AddEffect abfangen: wirkt nicht live,
//    ein laufender Effekt bliebe drin.
//  - CC_Sharpen/DesaturateEffect anfassen: dort sitzen auch die
//    Wetter- und die Erschoepfungs-Entsaettigung.
//
//  NICHT BETROFFEN: HealthDesaturate (DesaturateAccumulator, leichte
//  Entsaettigung bei Erschoepfung) ist ein eigener Wert im selben
//  Shader und hat nichts mit Schmerzmitteln zu tun.
// ---------------------------------------------------------------
using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace KeinSchmerzmittelFilter
{
	[BepInPlugin("com.kaan.keinschmerzmittelfilter", "No Painkiller Desaturation", "1.0.0")]
	public class Plugin : BaseUnityPlugin
	{
		internal static ManualLogSource Log;

		internal static ConfigEntry<bool> Eingeschaltet;

		private void Awake()
		{
			Log = Logger;
			Eingeschaltet = Config.Bind("1. General", "Enabled", true,
				"Removes the grey, desaturated colours of painkillers. The painkiller itself keeps working. "
				+ "Takes effect immediately, also during a raid.");
			new Harmony("com.kaan.keinschmerzmittelfilter").PatchAll(typeof(MaskenZielPatch));
			Log.LogInfo("No Painkiller Desaturation active");
		}
	}

	[HarmonyPatch(typeof(EffectsController.DesaturateMaskAccumulator),
		nameof(EffectsController.DesaturateMaskAccumulator.UpdateAmount))]
	internal static class MaskenZielPatch
	{
		private static bool _fehlerGemeldet;

		[HarmonyPrefix]
		private static void Vorher(EffectsController.DesaturateMaskAccumulator __instance)
		{
			try
			{
				if (Plugin.Eingeschaltet.Value)
				{
					__instance.MaxEffectValue = 0f;
					__instance._desaturateRadius = __instance._maxDesaturateRadius;
					// Das Ende EINES von zwei Schmerzmitteln setzt diese Marke.
					// Bei Maske 0 wuerde UpdateAmount den Sammler dann abschalten,
					// obwohl noch eins laeuft, und das Ausschalten des Mods bliebe
					// bis zum naechsten Schmerzmittel ohne Wirkung.
					if (__instance.ActiveEffects.Count > 0)
					{
						__instance._restoreDesaturateMask = false;
					}
				}
				else if (__instance.MaxEffectValue == 0f && __instance.ActiveEffects.Count > 0)
				{
					// Gerade ausgeschaltet: Das Spiel rechnet seine Ziele neu aus.
					// Ohne diesen Mod gibt es diesen Zustand nicht, sonst laeuft
					// hier also nichts.
					__instance.UpdateMaxEffectValue();
				}
			}
			catch (Exception fehler)
			{
				if (!_fehlerGemeldet)
				{
					_fehlerGemeldet = true;
					Plugin.Log.LogError("Painkiller desaturation patch failed: " + fehler);
				}
			}
		}
	}
}
