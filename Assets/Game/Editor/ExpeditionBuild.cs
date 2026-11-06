using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EvolvingRPG.Editor
{
    public static class ExpeditionBuild
    {
        [MenuItem("Tools/EvolvingRPG/Build Relay Windows Development Player")]
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play first.");
            for(int i=0;i<SceneManager.sceneCount;i++)
                if(SceneManager.GetSceneAt(i).isDirty || string.IsNullOrEmpty(SceneManager.GetSceneAt(i).path))
                    throw new InvalidOperationException("Save scene edits first. Build does not save or replace user scene edits.");
            if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone,BuildTarget.StandaloneWindows64))
                throw new InvalidOperationException("Configured Unity installation lacks Windows build support.");
            ExpeditionSceneBuilder.Create();
            string output=Path.GetFullPath(Path.Combine("Builds","Relay-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"),"EvolvingRPG.exe"));
            Directory.CreateDirectory(Path.GetDirectoryName(output)); Directory.CreateDirectory("Logs");
            string log="Logs/ExpeditionBuild.txt";
            File.WriteAllText(log,"Unity "+Application.unityVersion+" / Windows development build / "+DateTime.UtcNow.ToString("O")+"\nOutput: "+output+"\n");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[] { ExpeditionSceneBuilder.ScenePath }, locationPathName=output,
                target=BuildTarget.StandaloneWindows64, options=BuildOptions.Development | BuildOptions.StrictMode
            });
            var summary=report.summary;
            File.AppendAllText(log,summary.result+" / errors "+summary.totalErrors+" / warnings "+summary.totalWarnings
                +" / bytes "+summary.totalSize+" / duration "+summary.totalTime+"\n");
            foreach(var step in report.steps) foreach(var message in step.messages)
                if(message.type==LogType.Error || message.type==LogType.Exception) File.AppendAllText(log,message.content+"\n");
            if(summary.result!=BuildResult.Succeeded || summary.totalErrors!=0) throw new InvalidOperationException("Relay build did not succeed; inspect "+log);
            Debug.Log("Relay Windows development build succeeded: "+output+". Player has not been launched or playtested.");
        }
    }
}
