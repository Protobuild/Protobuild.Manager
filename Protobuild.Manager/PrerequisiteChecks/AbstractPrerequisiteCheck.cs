namespace Unearth
{
    using System;

    public abstract class AbstractPrerequisiteCheck : IPrerequisiteCheck
    {
        private PrerequisiteCheckStatus m_Status;

        private string m_Message;

        public event EventHandler StatusChanged;

        public abstract string ID { get; }

        public abstract string Name { get; }

        public PrerequisiteCheckStatus Status
        {
            get
            {
                return this.m_Status;
            }
            protected set
            {
                this.m_Status = value;

                if (this.StatusChanged != null)
                {
                    this.StatusChanged(this, new EventArgs());
                }
            }
        }

        public string Message
        {
            get
            {
                return this.m_Message;
            }
            protected set
            {
                this.m_Message = value;

                if (this.StatusChanged != null)
                {
                    this.StatusChanged(this, new EventArgs());
                }
            }
        }

        public abstract void Check();
    }
}