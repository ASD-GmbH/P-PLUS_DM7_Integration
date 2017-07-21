using System;

namespace DM7_PPLUS_Integration.Implementierung.Shared
{
    internal struct VersionsStand : Stand
    {
        public readonly Guid Session;
        public readonly long Version;

        public static Stand AbInitio(Guid session)
        {
            return new VersionsStand(session, -1);
        }

        public static Stand AbInitio()
        {
            return new VersionsStand(Guid.Empty, -1);
        }

        public VersionsStand(Guid session, long version)
        {
            Session = session;
            Version = version;
        }


        public override string ToString()
        {
            return $"{Version}";
        }
    }
}