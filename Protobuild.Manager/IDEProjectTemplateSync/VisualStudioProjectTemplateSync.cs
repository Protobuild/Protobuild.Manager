using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using System.Xml;

namespace Protobuild.Manager
{
    public class VisualStudioProjectTemplateSync : IIDEProjectTemplateSync
    {
        private readonly ITemplateSource _templateSource;
        private readonly IBrandingEngine _brandingEngine;

        public VisualStudioProjectTemplateSync(ITemplateSource templateSource, IBrandingEngine brandingEngine)
        {
            _templateSource = templateSource;
            _brandingEngine = brandingEngine;
        }

        public void Sync()
        {
            Task.Run(async () => await SyncInternal());
        }

        private async Task SyncInternal()
        {
            var vsVersions = new[] {"2015"};

            foreach (var version in vsVersions)
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                path = Path.Combine(path, "Visual Studio " + version, "Templates", "ProjectTemplates", _brandingEngine.TemplateIDECategory, _brandingEngine.ProductStorageID.TrimStart('.'));
                
                await InstallTemplates(path);
            }

            var expVsVersions = new[] {"14.0"};

            foreach (var version in expVsVersions)
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                path = Path.Combine(path, "Microsoft", "VisualStudio", version + "Exp", "ProjectTemplates");
                if (Directory.Exists(path))
                {
                    path = Path.Combine(path, _brandingEngine.TemplateIDECategory,
                        _brandingEngine.ProductStorageID.TrimStart('.'));
                    await InstallTemplates(path);
                }
            }
        }

        private async Task InstallTemplates(string path)
        {
            Directory.CreateDirectory(path);

            foreach (var file in new DirectoryInfo(path).GetFiles("*.vstemplate").ToList())
            {
                file.Delete();
            }

            var assemblyPath = Assembly.GetEntryAssembly().Location;
            var workingDirectory = Environment.CurrentDirectory;

            // Extract branding icon for Windows.
            using (
                var writer = new FileStream(Path.Combine(path, "Template.ico"), FileMode.Create, FileAccess.Write))
            {
                _brandingEngine.WindowsIcon.Save(writer);
            }

            foreach (var template in _templateSource.GetTemplates())
            {
                var xml =
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                    "<VSTemplate Version=\"3.0.0\" Type=\"Project\" xmlns=\"http://schemas.microsoft.com/developer/vstemplate/2005\" xmlns:sdk=\"http://schemas.microsoft.com/developer/vstemplate-sdkextension/2010\">" +
                    "  <TemplateData>" +
                    "    <Name>" + SecurityElement.Escape(template.TemplateName) + "</Name>" +
                    "    <Description>" + SecurityElement.Escape(template.TemplateDescription) + "</Description>" +
                    "    <Icon>Template.ico</Icon>" +
                    "    <ProjectType>CSharp</ProjectType>" +
                    "    <TemplateID>" + SecurityElement.Escape(template.TemplateName) + "</TemplateID>" +
                    "    <SortOrder>0</SortOrder>" +
                    "    <DefaultName>" + SecurityElement.Escape(template.TemplateName) + "</DefaultName>" +
                    "    <ProvideDefaultName>true</ProvideDefaultName>" +
                    "    <CreateNewFolder>false</CreateNewFolder>" +
                    "    <LocationField>Disabled</LocationField>" +
                    "    <CreateInPlace>true</CreateInPlace>" +
                    "  </TemplateData>" +
                    "  <TemplateContent>" +
                    "    <CustomParameters>" +
                    "      <CustomParameter Name=\"ProtobuildManagerExecutablePath\" Value=\"" +
                    SecurityElement.Escape(assemblyPath) + "\" />" +
                    "      <CustomParameter Name=\"ProtobuildManagerWorkingDirectory\" Value=\"" +
                    SecurityElement.Escape(workingDirectory) + "\" />" +
                    "      <CustomParameter Name=\"ProtobuildManagerTemplateURL\" Value=\"" +
                    SecurityElement.Escape(template.TemplateURI) + "\" />" +
                    "    </CustomParameters>" +
                    "  </TemplateContent>" +
                    "  <WizardExtension>" +
                    "    <Assembly>Protobuild.IDE.VisualStudio, Version=1.0.0.0, Culture=neutral, PublicKeyToken=418e79cef58c6857</Assembly>" +
                    "    <FullClassName>Protobuild.IDE.VisualStudio.CrossPlatformProjectWizard</FullClassName>" +
                    "  </WizardExtension>" +
                    "</VSTemplate>";
                using (var writer = new StreamWriter(Path.Combine(path, template.TemplateName + ".vstemplate")))
                {
                    await writer.WriteAsync(xml);
                }
            }
        }
    }
}
