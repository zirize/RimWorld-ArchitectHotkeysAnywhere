using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace ArchitectHotkeysAnywhere;

// The Hotkeys mod reads its "<category> tab" bindings inside MainTabWindow_Architect.ExtraOnGUI,
// which only runs while the Architect window is open. This component handles the same bindings
// while that window is closed: it opens the window on the requested category and consumes the
// key, so Hotkeys' own handler never sees it. While the window is open nothing changes.
public class ArchitectHotkeysGameComp : GameComponent
{
    private const string Prefix = "Hotkeys_ArchitectHotkey";

    private static readonly FieldInfo DesPanelsField =
        typeof(MainTabWindow_Architect).GetField("desPanelsCached", BindingFlags.Instance | BindingFlags.NonPublic);

    private static List<KeyValuePair<KeyBindingDef, string>> bindings;

    public ArchitectHotkeysGameComp(Game game)
    {
    }

    public override void GameComponentOnGUI()
    {
        if (Event.current.type != EventType.KeyDown || Event.current.keyCode == KeyCode.None)
            return;
        if (Find.CurrentMap == null || WorldRendererUtility.WorldSelected)
            return;
        if (Find.MainTabsRoot.OpenTab == MainButtonDefOf.Architect)
            return;
        if (GUIUtility.keyboardControl != 0 || AnyBlockingWindow())
            return;

        bindings ??= CollectBindings();
        foreach (var pair in bindings)
        {
            if (!pair.Key.JustPressed)
                continue;
            if (!(MainButtonDefOf.Architect.TabWindow is MainTabWindow_Architect architect))
                return;
            var panel = FindPanel(architect, pair.Value);
            if (panel == null)
                return;
            Event.current.Use();
            Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Architect);
            architect.selectedDesPanel = panel;
            Find.DesignatorManager.Deselect();
            return;
        }
    }

    private static List<KeyValuePair<KeyBindingDef, string>> CollectBindings()
    {
        var list = new List<KeyValuePair<KeyBindingDef, string>>();
        foreach (var def in DefDatabase<KeyBindingDef>.AllDefsListForReading)
        {
            if (def.defName.StartsWith(Prefix))
                list.Add(new KeyValuePair<KeyBindingDef, string>(def, def.defName.Substring(Prefix.Length)));
        }
        if (list.Count == 0)
            Log.Message("[ArchitectHotkeysAnywhere] no Hotkeys architect bindings found; is Hotkeys loaded with 'Architect Hotkeys' enabled?");
        return list;
    }

    private static ArchitectCategoryTab FindPanel(MainTabWindow_Architect architect, string categoryDefName)
    {
        if (!(DesPanelsField?.GetValue(architect) is List<ArchitectCategoryTab> panels))
            return null;
        foreach (var panel in panels)
        {
            if (panel.def.defName == categoryDefName)
                return panel;
        }
        return null;
    }

    // A modal dialog (absorbs input around itself) must keep its keys; vanilla tab hotkeys don't fire there either.
    private static bool AnyBlockingWindow()
    {
        var windows = Find.WindowStack.Windows;
        for (int i = 0; i < windows.Count; i++)
        {
            if (windows[i].absorbInputAroundWindow)
                return true;
        }
        return false;
    }
}
