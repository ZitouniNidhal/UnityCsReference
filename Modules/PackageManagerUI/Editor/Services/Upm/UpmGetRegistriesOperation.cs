// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using UnityEditor.PackageManager.Requests;

namespace UnityEditor.PackageManager.UI
{
    [Serializable]
    internal class UpmGetRegistriesOperation : UpmBaseOperation<GetRegistriesRequest>
    {
        public void GetRegistries()
        {
            Start();
        }

        protected override GetRegistriesRequest CreateRequest()
        {
            return Client.GetRegistries();
        }

        public void Cancel()
        {
            CancelInternal();
        }
    }
}