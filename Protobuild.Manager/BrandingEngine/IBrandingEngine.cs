namespace Protobuild.Manager
{
    public interface IBrandingEngine
    {
        /// <summary>
        /// The name of the product, as displayed to the user.
        /// </summary>
        string ProductName { get; }

        /// <summary>
        /// The name of the storage folder to use when storing information
        /// inside the Application Data (or Linux / Mac equivalent).
        /// </summary>
        string ProductStorageID { get; }

        /// <summary>
        /// The online RSS feed URL that should be used for displaying news.
        /// </summary>
        string RSSFeedURL { get; }

#if PLATFORM_WINDOWS
        System.Drawing.Icon WindowsIcon { get; }
#elif PLATFORM_LINUX
        Gdk.Pixbuf LinuxIcon { get; }
#endif

        /// <summary>
        /// The template source to draw from.
        /// </summary>
        string TemplateSource { get; }

        string TemplateIDECategory { get; }

        string VisualStudioAddinPackage { get; }

        string MonoDevelopAddinPackage { get; }

        ProtobuildUpdatePolicy ProtobuildUpdatePolicy { get; }
    }
}