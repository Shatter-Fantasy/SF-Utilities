using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

#nullable disable

namespace SF.Utilities.Raymarching
{
    public struct RayMarchingCommand : IRayMarchingCommand
    {
        /// <summary>
        /// The amount of ray marching steps to take.
        /// </summary>
        public int MaxSteps;
        /// <summary>
        /// The distance to check for each ray marching step.
        /// </summary>
        public float StepsAmount;
        /// <summary>
        /// The position the ray marching command starts to calculate steps from.
        /// Most of the time this is the camera position to allow stepping from the camera to the scene.
        /// </summary>
        public Vector3 OriginPos;
        /// <summary>
        /// The view direction the ray marching is moving forward in.
        /// If this value is set in the constructor the steps amount will be precalculated to save on proccesing after it is created. 
        /// </summary>
        public Vector3 ViewDirection;
        /// <summary>
        /// This is an array of the positions made from calculating the ray marching per step.
        /// This will have a length of the MaxSteps plus 1 to also store the starting point.
        /// </summary>
        private Vector3[] StepPositions;

        /// <summary>
        /// Static cached default value for RayMarchingCommands. This allows for quickly setting up a simple raymarching command
        /// </summary>
        public static RayMarchingCommand DefaultCommand = new RayMarchingCommand(
            64,
            0.02f,
            Vector3.zero,
            Vector3.forward,
            false); 
        
        public RayMarchingCommand(int maxSteps, float stepAmount)
        {
            // Make sure we never try to pass 
            if (maxSteps <= 0)
                maxSteps = 1;
            
            MaxSteps = maxSteps;
            StepsAmount = stepAmount;
            // Step positions length is MaxSteps plus one to also keep track of the starting position.
            StepPositions = new Vector3[MaxSteps + 1];

            ViewDirection = Vector3.forward;
            OriginPos = Vector3.zero;
        }
        public RayMarchingCommand(int maxSteps, float stepAmount, Vector3 originPos, Vector3 viewDirection, bool preCalculateSteps = false)
        {
            // Make sure we never try to pass 
            if (maxSteps <= 0)
                maxSteps = 1;
            
            MaxSteps = maxSteps;
            StepsAmount = stepAmount;
            // Step positions length is MaxSteps plus one to also keep track of the starting position.
            StepPositions = new Vector3[MaxSteps + 1];

            OriginPos = originPos;
            ViewDirection = viewDirection;

            if (preCalculateSteps)
            {
                CalculateStepPositions();
            }
        }
        
        public void CalculateStepPositions()
        {
            var stepPositions = new NativeArray<Vector3>((MaxSteps + 1),Allocator.Persistent);

            var rayStepJob = new RayMarchingStepJob()
            {
                
                StepPositionsResults = stepPositions,
                StepsAmount = StepsAmount,
                OriginPos = OriginPos,
                ViewDirectionNormalized = ViewDirection.normalized,
            };

            JobHandle jobHandle = rayStepJob.Schedule(stepPositions.Length, 32);
            
            jobHandle.Complete();

            rayStepJob.StepPositionsResults.CopyTo(StepPositions);
            stepPositions.Dispose();
        }
                
        public void DrawRayMarching(Vector3 startingPos)
        {
            
            CalculateStepPositions();

            ReadOnlySpan<Vector3> pointsSpan = StepPositions;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLineStrip(pointsSpan, false);
        }

        /* TODO: Implement this by finsihing the JobHandle return after setting the parameters.
        public static unsafe JobHandle ScheduleCalculateStepPositionsBatch(
            NativeArray<RayMarchingCommand> commands,
            NativeArray<RayMarchingStepResult> resultSteps, 
            int minCommandsPerJob,
            JobHandle dependsOn = default(JobHandle))
        {
            if (resultSteps.Length < commands.Length)
            {
                Debug.LogWarning("The supplied result positions buffer is to small compared to the amount of RayMarching commands.");
                return new JobHandle();
            }

            // Create the batch query for the commands and set the NativeArray that the results will be put into.
            BatchQueryJob<RayMarchingCommand, RayMarchingStepResult> output =
                new BatchQueryJob<RayMarchingCommand, RayMarchingStepResult>(commands, resultSteps);

            JobsUtility.JobScheduleParameters parameters = new JobsUtility.JobScheduleParameters(
                    UnsafeUtility.AddressOf<BatchQueryJob<RayMarchingCommand, RayMarchingStepResult>>(ref output),
                    BatchQueryJobStruct<BatchQueryJob<RayMarchingCommand,RayMarchingStepResult>>.Initialize(),
                    dependsOn,
                    ScheduleMode.Parallel);
            
        }
        */
    }

    public struct RayMarchingStepJob : IJobParallelFor
    {
        public NativeArray<Vector3> StepPositionsResults;
        /// <summary>
        /// The distance to check for each ray marching step.
        /// </summary>
        public float StepsAmount;
        /// <summary>
        /// The position the ray marching command starts to calculate steps from.
        /// Most of the time this is the camera position to allow stepping from the camera to the scene.
        /// </summary>
        public Vector3 OriginPos;
        /// <summary>
        /// The normalized view direction the ray marching is moving forward in.
        /// If this value is set in the constructor the steps amount will be precalculated to save on proccesing after it is created. 
        /// </summary>
        public Vector3 ViewDirectionNormalized;
        public void Execute(int index)
        {
            StepPositionsResults[index] = OriginPos + (StepsAmount * index * ViewDirectionNormalized);
        }
    }
}
