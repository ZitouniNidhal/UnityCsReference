// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Runtime.InteropServices;
using UnityEngine.Bindings;
using Unity.Collections;
using Unity.Jobs;

namespace Unity.Jobs.LowLevel.Unsafe
{
    /// <summary>
    /// Represents a batch query job that processes a set of commands and produces results.
    /// </summary>
    /// <typeparam name="CommandT">The type of the commands.</typeparam>
    /// <typeparam name="ResultT">The type of the results.</typeparam>
    public struct BatchQueryJob<CommandT, ResultT> 
        where CommandT : struct
        where ResultT : struct
    {
        [ReadOnly]
        private readonly NativeArray<CommandT> _commands;
        private readonly NativeArray<ResultT> _results;

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchQueryJob{CommandT, ResultT}"/> struct.
        /// </summary>
        /// <param name="commands">The commands to process.</param>
        /// <param name="results">The results of the processing.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="commands"/> or <paramref name="results"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="commands"/> or <paramref name="results"/> is empty.</exception>
        public BatchQueryJob(NativeArray<CommandT> commands, NativeArray<ResultT> results)
        {
            if (commands == null)
                throw new ArgumentNullException(nameof(commands));
            if (results == null)
                throw new ArgumentNullException(nameof(results));
            if (commands.Length == 0)
                throw new ArgumentException("Commands array cannot be empty.", nameof(commands));
            if (results.Length == 0)
                throw new ArgumentException("Results array cannot be empty.", nameof(results));

            _commands = commands;
            _results = results;
        }

        /// <summary>
        /// Gets the commands to process.
        /// </summary>
        public NativeArray<CommandT> Commands => _commands;

        /// <summary>
        /// Gets the results of the processing.
        /// </summary>
        public NativeArray<ResultT> Results => _results;
    }

    /// <summary>
    /// Provides reflection data for batch query jobs.
    /// </summary>
    /// <typeparam name="T">The type of the job struct.</typeparam>
    public struct BatchQueryJobStruct<T> where T : struct
    {
        private static IntPtr _jobReflectionData;

        /// <summary>
        /// Initializes the job reflection data.
        /// </summary>
        /// <returns>The pointer to the job reflection data.</returns>
        public static IntPtr Initialize()
        {
            if (_jobReflectionData == IntPtr.Zero)
            {
                _jobReflectionData = JobsUtility.CreateJobReflectionData(typeof(T), null);
            }
            return _jobReflectionData;
        }
    }
}