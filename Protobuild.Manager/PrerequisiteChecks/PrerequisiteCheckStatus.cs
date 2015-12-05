using System;

namespace Protobuild.Manager
{
    public enum PrerequisiteCheckStatus
    {
        Pending,
        Checking,
        Passed,
        Waiting,
        Installing,
        Warning,
        Failed,
    }
}

