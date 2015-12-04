// ====================================================================== //
// This source code is licensed in accordance with the licensing outlined //
// on the main Tychaia website (www.tychaia.com).  Changes to the         //
// license on the website apply retroactively.                            //
// ====================================================================== //
namespace Unearth
{
    public static class VersionConfig
    {
        /// <summary>
        /// The compatibility version number.  Change this value when the launcher requires
        /// breaking changes in conjunction with the website.  This version is sent to the
        /// server, which will tell the user to update the launcher if this number is too old.
        /// </summary>
        public const int VersionNumber = 2;
    }
}