# P-PLUS_DM7_Integration
Entwurf der Integration von DM7 und P-PLUS via NetMQ Nachrichten.
DM7 bekommt einen Adapter der die DM7_PPLUS_API implementiert. Darüber kann DM7 Abfragen stellen und wird über Änderungen informiert. 
Ziel ist es, die Integration beidseitig Versionsflexibel zu entwickeln, damit mehrere Stände von P-PLUS und mehrere Stände von DM7 miteinander arbeiten können.
  siehe [Offene Fragen]

## Benutzung
Erstelle eine Instanz der DM7_PPLUS_API via DM7_PPLUS_API_Factory.Connect
Zur Verbindung braucht die API eine Netzwerkadresse (IP & Port).
Mit der Konfiguration können wir die Identifikation der DM7 Instanz und evtl. weitere Einstellungen vornehmen.
Der Log Adapter dient als Rückkanal, um Support- und Betriebsnachrichten von P-PLUS zu DM7 zu senden.

## DM7_PPLUS_API
Die API ermöglicht ein asynchrones Abrufen aller Mitarbeiterdaten, und ein eingeschränktes Abrufen der Mitarbeiter, bei denen sich Änderungen ergeben haben:
```
 Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen(Stand von, Stand bis);
 Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen();
```
Über Änderungen an den Mitarbeiterdaten informiert der Stream des Mitarbeiterstands:
```
 IObservable<Stand> Stand_Mitarbeiterdaten { get; } 
```

In den Mitarbeiterdatensätzen finden sich Auswahllisten, deren Elemente als Guid übertragen werden. Welchen Stand der Auswahllisten die P-PLUS DM7 Integration unterstützt, liefert die Abfrage:
``` 
int Auswahllisten_Version { get; } 
```

## Auswahllisten
Die konkrete Bedeutung der Guids wird über ein Wörterbuch in der Auswahllisten_{Auswahllisten_Version}.cs aufgelöst. Bei Veränderungen an diesen Listen wird immer eine neue Auswahllisten_{Auswahllisten_Version+1}.cs ausgeliefert. Diese Datei ist nebenläufig zur Integration gepflegt und ermöglicht eine dynamische Anpassung der Inhalte, ohne eine Neuauslieferung der Software zu erfordern.

## Offene Fragen
Gesamtversionierung muss noch geklärt werden.
Wie behandeln wir mehrere (P-PLUS)Mandanten in der Integration.

## Kollaboration
1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request :D

## License
MIT License

Copyright (c) 2017 ASD Personalinformationssysteme GmbH

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
