using System;
using System.Collections.ObjectModel;

namespace DM7_PPLUS_Integration.Daten
{
    /// <summary>
    /// Das Ergebnis der Verarbeitung des Soll/Ist Abgleichs im Dienstplan.
    /// Mögliche Ergebnisse:
    /// - Verarbeitet
    /// - Dienstplanabschluss_verhindert_Verarbeitung
    /// </summary>
    public interface Soll_Ist_Abgleich_Verarbeitungsergebnis {}

    public readonly struct Verarbeitet : Soll_Ist_Abgleich_Verarbeitungsergebnis { }

    public readonly struct Dienstplanabschluss_verhindert_Verarbeitung : Soll_Ist_Abgleich_Verarbeitungsergebnis
    {
        public readonly ReadOnlyCollection<Dienstplanabschluss> Dienstplanabschlüsse;

        public Dienstplanabschluss_verhindert_Verarbeitung(ReadOnlyCollection<Dienstplanabschluss> dienstplanabschlüsse)
        {
            Dienstplanabschlüsse = dienstplanabschlüsse;
        }
    }

    public readonly struct Dienstplanabschluss
    {
        public readonly Guid MitarbeiterId;
        public readonly Guid MandantId;
        public readonly Datum Datum;

        public Dienstplanabschluss(Guid mitarbeiterId, Guid mandantId, Datum datum)
        {
            MitarbeiterId = mitarbeiterId;
            MandantId = mandantId;
            Datum = datum;
        }
    }
}