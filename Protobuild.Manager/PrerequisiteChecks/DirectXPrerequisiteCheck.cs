#if PLATFORM_WINDOWS
namespace Protobuild.Manager
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Xml;

    public class DirectXPrerequisiteCheck : AbstractPrerequisiteCheck
    {
        public DirectXPrerequisiteCheck()
        {
            this.Status = PrerequisiteCheckStatus.Pending;
        }

        public override string ID
        {
            get
            {
                return "directx-11";
            }
        }

        public override string Name
        {
            get
            {
                return "DirectX 11 or greater";
            }
        }

        public override void Check()
        {
            this.Status = PrerequisiteCheckStatus.Checking;

            var path = Path.GetTempFileName();

            try
            {
                var p = Process.Start("dxdiag", "/x " + path);
                p.WaitForExit();
            }
            catch (Exception)
            {
            }

            if (!File.Exists(path))
            {
                this.Status = PrerequisiteCheckStatus.Warning;
                this.Message = "Unable to check DirectX version.  Ensure dxdiag.exe is available.";
                return;
            }

            var doc = new XmlDocument();
            doc.Load(path);

            try
            {
                var dxd = doc.SelectSingleNode("//DxDiag");
                var dxv = dxd.SelectSingleNode("//DirectXVersion");
                var version = Convert.ToInt32(dxv.InnerText.Split(' ')[1]);

                if (version >= 11)
                {
                    this.Status = PrerequisiteCheckStatus.Passed;
                }
                else
                {
                    this.Status = PrerequisiteCheckStatus.Failed;
                    this.Message = "You need to have at least DirectX 11 installed to run this game.";
                }
            }
            catch
            {
                this.Status = PrerequisiteCheckStatus.Warning;
                this.Message = "Unable to check DirectX version.  dxdiag.exe did not return a usable result.";
                return;
            }
        }
    }
}
#endif