﻿using System;
using System.Collections.Generic;
using DM7_PPLUS_Integration.Daten;

namespace DM7_PPLUS_Integration.Implementierung
{
    public interface PPLUS_Backend
    {
        IObservable<IEnumerable<Guid>> Aenderungen_an_Mitarbeiterstammdaten { get; }
        IEnumerable<Guid> Alle_Mitarbeiter();
        IEnumerable<Mitarbeiterdatensatz> Mitarbeiterdatensaetze_abrufen(IEnumerable<Guid> mitarbeiter_ids);
    }
}