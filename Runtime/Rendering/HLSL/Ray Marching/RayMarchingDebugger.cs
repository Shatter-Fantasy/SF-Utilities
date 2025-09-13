using Unity.Jobs;
using Unity.Collections;
using UnityEngine;

namespace SF.Utilities.Raymarching
{
    public class RayMarchingDebugger : MonoBehaviour
    {
        public Camera MainCamera;
        public RayMarchingCommand MarchingCommand = RayMarchingCommand.DefaultCommand;
        private void OnDrawGizmos()
        {
            if(MainCamera == null)
                MainCamera = Camera.main;
            if (MainCamera == null)  return;
            
            MarchingCommand.ViewDirection = MainCamera.transform.forward;
            MarchingCommand.DrawRayMarching(MainCamera.transform.position);
        }
    }
    
    
    /// <summary>
    /// Interface to implement a drawing command for visualizing RayMarching Commands using Gizmos drawing.
    /// </summary>
    public interface IRayMarchingCommand
    {
        public abstract void DrawRayMarching(Vector3 startingPos);
    }
    
    public static class SFRayMarchingUtils
    {
        public static int MaxStepsPropertyID = Shader.PropertyToID("_MaxSteps");
        public static int StepsAmountPropertyID = Shader.PropertyToID("_StepsAmount");
        
        public static void SetMaterialProperties(this RayMarchingCommand command, Material material)
        {
            if (material == null)
            {
#if UNITY_EDITOR
                Debug.Log("There is no valid material to set properties on.");
#endif
                return;
            }

            if (material.HasProperty(MaxStepsPropertyID))
            {
                material.SetInteger(MaxStepsPropertyID, (int)command.MaxSteps);
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log($"There was no shader property called _MaxSteps for the material: {material}. Skipping the setting of this one property");
#endif
            }

            if (material.HasProperty(StepsAmountPropertyID))
            {
                material.SetFloat(StepsAmountPropertyID, command.StepsAmount);
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log($"There was no shader property called _StepsAmount for the material: {material}. Skipping the setting of this one property");
#endif
            }
        }
    }
}
