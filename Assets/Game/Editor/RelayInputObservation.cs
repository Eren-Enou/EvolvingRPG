using System;
using System.IO;
using System.Reflection;
using System.Text;
using EvolvingRPG.Expeditions;
using EvolvingRPG.Progression;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace EvolvingRPG.Editor
{
    // Explicit, bounded, read-only observation. No input injection, device changes, saves or scene edits.
    [InitializeOnLoad]
    public static class RelayInputObservation
    {
        const string Report = "Logs/RelayInputObservation.txt";
        static readonly StringBuilder trace = new StringBuilder();
        static OwnedCharacter owner;
        static ExpeditionLoop loop;
        static InputAction inspect, interact;
        static double until;
        static string previous;
        static int keyboardEvents, tabEvents, fEvents, tabPerformed, fPerformed;
        static bool active;
        static RelayInputObservation()
        {
            AssemblyReloadEvents.beforeAssemblyReload += Stop;
            EditorApplication.playModeStateChanged += state => { if(state == PlayModeStateChange.ExitingPlayMode) Stop(); };
        }

        [MenuItem("Tools/EvolvingRPG/Observe Relay Input For 30 Seconds")]
        public static void Run()
        {
            if(!EditorApplication.isPlaying) throw new InvalidOperationException("Enter RelayOutpost Play mode first.");
            Stop();
            loop = UnityEngine.Object.FindAnyObjectByType<ExpeditionLoop>();
            if(loop == null) throw new InvalidOperationException("No active relay loop.");
            owner = loop.Character;
            inspect = Action(owner,"inspect"); interact = Action(loop,"interact");
            trace.Clear(); previous=null; keyboardEvents=tabEvents=fEvents=tabPerformed=fPerformed=0;
            trace.AppendLine("Unity "+Application.unityVersion+" / read-only relay input observation / "+DateTime.UtcNow.ToString("O"));
            trace.AppendLine("Only Tab/F presence and action/focus/menu state are observed; no other key values recorded.");
            until=EditorApplication.timeSinceStartup+30; active=true;
            inspect.performed += InspectPerformed; interact.performed += InteractPerformed;
            InputSystem.onEvent += Event;
            EditorApplication.update += Tick;
            EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView")).Focus();
        }
        static InputAction Action(object target,string name) => (InputAction)target.GetType()
            .GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(target);
        static void InspectPerformed(InputAction.CallbackContext _) => tabPerformed++;
        static void InteractPerformed(InputAction.CallbackContext _) => fPerformed++;
        static void Event(InputEventPtr value,InputDevice device)
        {
            if(!(device is Keyboard keyboard) || !value.IsA<StateEvent>() && !value.IsA<DeltaStateEvent>()) return;
            keyboardEvents++;
            if(keyboard.tabKey.ReadValueFromEvent(value,out float tab) && tab>.5f) tabEvents++;
            if(keyboard.fKey.ReadValueFromEvent(value,out float f) && f>.5f) fEvents++;
        }
        static void Tick()
        {
            if(!active) return;
            if(owner == null || !EditorApplication.isPlaying || EditorApplication.timeSinceStartup>=until) { Stop(); return; }
            var keyboard=Keyboard.current;
            string state="focus="+Application.isFocused+" cursor="+Cursor.lockState+" safe="+owner.CanDevelop
                +" menu="+owner.MenuOpen+" board="+loop.BoardOpen+" mission="+loop.InMission
                +" input="+owner.GetComponent<PlayerInput>().inputIsActive
                +" inspect="+inspect.enabled+"/"+inspect.controls.Count+" interact="+interact.enabled+"/"+interact.controls.Count
                +" keyboard="+(keyboard == null ? "none" : keyboard.deviceId+"/"+keyboard.enabled+"/"+keyboard.description.interfaceName)
                +" keyEvents="+keyboardEvents+" Tab="+tabEvents+"/"+tabPerformed+" F="+fEvents+"/"+fPerformed;
            if(state==previous) return;
            previous=state; trace.AppendLine(state);
        }
        static void Stop()
        {
            if(!active) return;
            active=false;
            inspect.performed-=InspectPerformed; interact.performed-=InteractPerformed;
            InputSystem.onEvent-=Event; EditorApplication.update-=Tick;
            trace.AppendLine("END keyEvents="+keyboardEvents+" Tab-present/perform="+tabEvents+"/"+tabPerformed+" F-present/perform="+fEvents+"/"+fPerformed);
            Directory.CreateDirectory("Logs"); File.WriteAllText(Report,trace.ToString());
            owner=null; loop=null; inspect=interact=null;
        }
    }
}
