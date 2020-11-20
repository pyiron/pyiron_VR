using UnityEngine;
using UnityEngine.UI;

namespace Networking
{
    public static class PythonCmd
    {
        public static readonly string GetPath = "project.path[:-1]";
        public static readonly string GetFolderData = "project.list_all()";
        public static readonly string LoadNoneJob = "load_job(None)";
        public static readonly string GetData = "get_data()";
        public static readonly string FormatJobSettings = "format_job_settings()";

        public static string OpenAbsPath(string jobName)
        {
            return "project = Project('" + jobName + "')";
        }

        public static string OpenRelPath(string jobName)
        {
            return "project = " + PythonScript.unityManager + ".project['" + jobName + "']";
        }
        
        public static string ResetCurrentJob()
        {
            return "reset_job('" + SimulationMenuController.jobName + "')";
        }
        
        public static string LoadJob(string jobName)
        {
            return "load_job(" + PythonScript.unityManager + ".project['" + jobName + "'])";
        }
        
        public static string SetJobStructureToCurrentFrame()
        {
            return "job.structure = unity_manager.Executor.job.get_structure("
                   + AnimationController.frame + ")";
        }
        
        public static string SetStructureToCurrentFrame()
        {
            return "structure = " + PythonScript.executor + ".job.get_structure(" + AnimationController.frame + ")";
        }
        
        public static string CreateStructure(string element, string repeat, Toggle cubicToggle, Toggle orthorombicToggle)
        {
            return "create(" + element + ", " + repeat + ", " + Utilities.ToggleToPythonBool(cubicToggle) + ", " + 
                   Utilities.ToggleToPythonBool(orthorombicToggle) + ")";
        }
        
        public static string RepeatStructure(string repeat)
        {
            return "structure.repeat([" + repeat + ", " + repeat + ", " + repeat + "])";
        }
        
        /// <summary>
        /// Although this function does nothing, it makes it easier to identify commands send to python, as most IDEs
        /// can show which functions are calling a function.
        /// </summary>
        /// <param name="msg">The message that should be send as it is.</param>
        /// <returns></returns>
        public static string SendRaw(string msg)
        {
            return msg;
        }
    }
}
