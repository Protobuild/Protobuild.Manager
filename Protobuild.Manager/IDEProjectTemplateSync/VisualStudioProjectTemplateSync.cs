using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Protobuild.Manager
{
    public class VisualStudioProjectTemplateSync : IIDEProjectTemplateSync
    {
        private readonly ITemplateSource _templateSource;
        private readonly IBrandingEngine _brandingEngine;
        private readonly IProcessLog _processLog;

        public VisualStudioProjectTemplateSync(ITemplateSource templateSource, IBrandingEngine brandingEngine, IProcessLog processLog)
        {
            _templateSource = templateSource;
            _brandingEngine = brandingEngine;
            _processLog = processLog;
        }

        public async Task Sync()
        {
            var vsVersions = new[] {"2010","2012","2013","2015"};

            foreach (var version in vsVersions)
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                path = Path.Combine(path, "Visual Studio " + version, "Templates", "ProjectTemplates", _brandingEngine.TemplateIDECategory);
             
                _processLog.WriteInfo("Writing out project templates for Visual Studio " + version + "...");
                   
                await InstallTemplates(path);
            }

            var expVsVersions = new[] {"14.0"};

            foreach (var version in expVsVersions)
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                path = Path.Combine(path, "Microsoft", "VisualStudio", version + "Exp", "ProjectTemplates");
                if (Directory.Exists(path))
                {
                    path = Path.Combine(path, _brandingEngine.TemplateIDECategory);
                    await InstallTemplates(path);
                }
            }

            _processLog.WriteInfo("Project templates synchronised.");
        }

        private async Task InstallTemplates(string path)
        {
            Directory.CreateDirectory(path);

            foreach (var file in new DirectoryInfo(path).GetFiles(_brandingEngine.ProductStorageID.TrimStart('.') + "-*.zip").ToList())
            {
                file.Delete();
            }

            var assemblyPath = Assembly.GetEntryAssembly().Location;
            var workingDirectory = Environment.CurrentDirectory;

            var i = 1;

            foreach (var template in _templateSource.GetTemplates())
            {
                using (var zip =
                    ZipFile.Open(
                        Path.Combine(path,
                            _brandingEngine.ProductStorageID.TrimStart('.') + "-" + (i++) + ".zip"),
                        ZipArchiveMode.Create))
                {

#if PLATFORM_WINDOWS
                    // Extract branding icon for Windows.
                    var templateIcon = zip.CreateEntry("Template.ico");
                    using (var stream = templateIcon.Open())
                    {
                        _brandingEngine.WindowsIcon.Save(stream);
                    }
#endif

                    var xml =
                        "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                        "<VSTemplate Version=\"3.0.0\" Type=\"Project\" xmlns=\"http://schemas.microsoft.com/developer/vstemplate/2005\" xmlns:sdk=\"http://schemas.microsoft.com/developer/vstemplate-sdkextension/2010\">" +
                        "  <TemplateData>" +
                        "    <Name>" + SecurityElement.Escape(template.TemplateName) + "</Name>" +
                        "    <Description>" + SecurityElement.Escape(template.TemplateDescription) + "</Description>" +
                        "    <NumberOfParentCategoriesToRollUp>1</NumberOfParentCategoriesToRollUp>" +
                        "    <Icon>Template.ico</Icon>" +
                        "    <ProjectType>CSharp</ProjectType>" +
                        "    <SortOrder>0</SortOrder>" +
                        "    <DefaultName>" + SecurityElement.Escape(template.TemplateName) + "</DefaultName>" +
                        "    <ProvideDefaultName>true</ProvideDefaultName>" +
                        "    <CreateNewFolder>false</CreateNewFolder>" +
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
                        "    <Assembly>Protobuild.IDE.VisualStudio.Wizard, Version=1.0.0.0, Culture=neutral, PublicKeyToken=418e79cef58c6857</Assembly>" +
                        "    <FullClassName>Protobuild.IDE.VisualStudio.CrossPlatformProjectWizard</FullClassName>" +
                        "  </WizardExtension>" +
                        "</VSTemplate>";
                    using (var memory = new MemoryStream())
                    {
                        using (var writer = new StreamWriter(memory, Encoding.UTF8, 4096, true))
                        {
                            await writer.WriteAsync(xml);
                        }

                        memory.Seek(0, SeekOrigin.Begin);

                        var document = new XmlDocument();
                        document.Load(memory);

                        using (var memory2 = new MemoryStream())
                        {
                            using (
                                var writer = XmlWriter.Create(memory2,
                                    new XmlWriterSettings
                                    {
                                        Indent = true,
                                        IndentChars = "  "
                                    }))
                            {
                                document.Save(writer);
                            }

                            memory2.Seek(0, SeekOrigin.Begin);

                            var templateXml = zip.CreateEntry("Template.vstemplate");
                            using (var stream = templateXml.Open())
                            {
                                await memory2.CopyToAsync(stream);
                            }
                        }
                    }
                }
            }
        }
    }
}
