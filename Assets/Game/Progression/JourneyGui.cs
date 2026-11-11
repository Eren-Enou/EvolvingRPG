using UnityEngine;

namespace EvolvingRPG.Progression
{
    // The Editor can observe layout for native input checks; button clicks still go through IMGUI.
    public static class JourneyGui
    {
        #if UNITY_EDITOR
        public static event System.Action<string, Vector2> ButtonRepainted;
        static Vector2 gameViewOrigin;
        #endif

        public static void Begin()
        {
            #if UNITY_EDITOR
            if (ButtonRepainted != null && Event.current.type == EventType.Repaint)
                gameViewOrigin = GUIUtility.GUIToScreenPoint(Vector2.zero);
            #endif
        }

        public static bool Button(string label, string observationKey = null)
        {
            bool clicked = GUILayout.Button(label);
            #if UNITY_EDITOR
            if (ButtonRepainted != null && Event.current.type == EventType.Repaint && GUI.enabled)
                ButtonRepainted(observationKey ?? label,
                    GUIUtility.GUIToScreenPoint(GUILayoutUtility.GetLastRect().center) - gameViewOrigin);
            #endif
            return clicked;
        }
    }
}
