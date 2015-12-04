using System;

namespace Unearth
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

