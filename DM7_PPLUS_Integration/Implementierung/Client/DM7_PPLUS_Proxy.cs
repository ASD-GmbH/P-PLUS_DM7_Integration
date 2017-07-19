namespace DM7_PPLUS_Integration.Implementierung.Client
{
    /*internal class DM7_PPLUS_Proxy : DM7_PPLUS_API
    {
        private readonly DM7_Netzwerkprotokoll _netzwerkprotokoll;
        private VersionsStand _stand = new VersionsStand(Guid.Empty, 0);

        public DM7_PPLUS_Proxy(DM7_Netzwerkprotokoll netzwerkprotokoll, Log log, int auswahllistenVersion)
        {
            _netzwerkprotokoll = netzwerkprotokoll;
            Auswahllisten_Version = auswahllistenVersion;
            Stand_Mitarbeiterdaten = new Subject<Stand>();
            _netzwerkprotokoll.Notifications.Subscribe(
                new Observer<byte[]>(
                    b =>
                    {
                        var datenquelle = new Guid(b);

                        ((Subject<Stand>)Stand_Mitarbeiterdaten).Next(_stand);
                    },
                    ex => log.Debug(ex.Message)));
        }

        public void Dispose() { }

        public int Auswahllisten_Version { get; }

        public IObservable<Stand> Stand_Mitarbeiterdaten { get; }

        public Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen()
        {
            return Mitarbeiterdaten_abrufen(new VersionsStand(_stand.Session,-1), _stand);
        }

        public Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen(Stand von, Stand bis)
        {
            //return _netzwerkprotokoll.Mitarbeiterdaten_abrufen((VersionsStand)von, (VersionsStand)bis);
            throw new NotImplementedException();
        }
    }*/
}
