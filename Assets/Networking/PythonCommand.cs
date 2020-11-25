using UnityEngine;
using UnityEngine.UI;

namespace Networking
{
    public static class PythonCmd
    {
        public static readonly string GetPath = PythonScript.unityManager + ".project.path[:-1]";
        public static readonly string GetFolderData = PythonScript.unityManager + ".project.list_all()";
        public static readonly string LoadNoneJob = PythonScript.executor + ".load_job(None)";
        public static readonly string GetStructureData = PythonScript.structure + ".get_data()";
        public static readonly string FormatJobSettings = PythonScript.executor + ".format_job_settings()";

        public static string OpenAbsPath(string jobName)
        {
            return PythonScript.unityManager + ".project = Project('" + jobName + "')";
        }

        public static string OpenRelPath(string jobName)
        {
            return PythonScript.unityManager + ".project = " + PythonScript.unityManager + ".project['" + jobName + "']";
        }
        
        public static string ResetCurrentJob()
        {
            return PythonScript.executor + ".reset_job('" + SimulationMenuController.jobName + "')";
        }
        
        public static string LoadJob(string jobName)
        {
            return PythonScript.executor + ".load_job(" + PythonScript.unityManager + ".project['" + jobName + "'])";
        }
        
        public static string SetJobStructureToCurrentFrame()
        {
            return PythonScript.executor + ".job.structure = unity_manager.Executor.job.get_structure("
                   + AnimationController.frame + ")";
            //return "job.structure = job.get_structure(" // this should work as well
            //       + AnimationController.frame + ")";
        }
        
        public static string SetStructureToCurrentFrame()
        {
            return PythonScript.structure + ".structure = " + PythonScript.executor + ".job.get_structure(" + AnimationController.frame + ")";
        }
        
        public static string CreateStructure(string element, string repeat, Toggle cubicToggle, Toggle orthorombicToggle)
        {
            return PythonScript.structure + ".create(" + element + ", " + repeat + ", " + 
                   Utilities.ToggleToPythonBool(cubicToggle) + ", " + Utilities.ToggleToPythonBool(orthorombicToggle) + ")";
        }
        
        public static string RepeatStructure(string repeat)
        {
            return PythonScript.structure + ".structure.repeat([" + repeat + ", " + repeat + ", " + repeat + "])";
        }
        
        public static string CalculateJob(JobData jobData)
        {
            return PythonScript.executor + ".calculate(" + JsonUtility.ToJson(jobData) + ")";
        }
        
        // public static string Calculate_MD(string calculation, JobData data, JobData jobData)
        // {
        //     return PythonScript.executor + ".calculate_" + calculation + "(" +
        //            data.temperature + ", " +
        //            data.n_ionic_steps + ", " +
        //            data.n_print + ", " +
        //            jobData.job_type + ", " +
        //            jobData.job_name + ", " +
        //            jobData.currentPotential + ")";
        // }
        //
        // public static string Calculate_Minimize(string calculation, JobData data, JobData jobData)
        // {
        //     return PythonScript.executor + ".calculate_" + calculation + "(" +
        //            data.f_eps + ", " +
        //            data.max_iterations + ", " +
        //            data.n_print + ", " +
        //            jobData.job_type + ", " +
        //            jobData.job_name + ", " +
        //            jobData.currentPotential + ")";
        // }
        
        
        
        // /// <summary>
        // /// Although this function does nothing, it makes it easier to identify commands send to python, as most IDEs
        // /// can show which functions are calling a function.
        // /// </summary>
        // /// <param name="msg">The message that should be send as it is.</param>
        // /// <returns></returns>
        // public static string SendRaw(string msg)
        // {
        //     return msg;
        // }
    }
}
