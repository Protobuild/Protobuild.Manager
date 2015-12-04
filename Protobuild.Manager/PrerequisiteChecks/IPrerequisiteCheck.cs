namespace Unearth
{
    using System;

    public interface IPrerequisiteCheck
    {
        event EventHandler StatusChanged;

        string ID { get; }

        string Name { get; }

        string Message { get; }

        PrerequisiteCheckStatus Status { get; }

        void Check();
    }
}