# P-PLUS_DM7_Integration
Schnittstelle zur Integration von DM7 und P-PLUS.
DM7 bekommt einen Adapter, der die DM7_PPLUS_API implementiert. Darüber kann DM7 Abfragen stellen und wird über Änderungen informiert. 
Ziel ist es, die Integration beidseitig versionsflexibel zu entwickeln, damit mehrere Stände von P-PLUS und mehrere Stände von DM7 miteinander arbeiten können.
  siehe [Offene Fragen]

## Benutzung
Das aktuelle Release findet sich hier:
https://github.com/ASD-GmbH/P-PLUS_DM7_Integration/releases .
Die .nupkg Datei kann mittels NuGet oder Paket in eine Visual Studio Solution eingebunden werden. Die .zip Datei enthält zusätzlich einen Beispiel Client und -server, die miteinander oder auch mit einer zu erstellenden Client Implementierung kommunizieren können. Außerdem ist der Quellcode der für den API Kontrakt relevanten Dateien aus diesem Repository noch einmal in der Zip Datei abgelegt.

Zunächst muss das NuGet Paket in die Solution eingebunden werden.
Danach kann eine Instanz der DM7_PPLUS_API via 
``` 
var api = PPLUS.Connect(url,log,cancellationToken_Verbindung).Result;
``` 
erstellt werden.

Zur Verbindung braucht die API eine Netzwerkadresse (IP & Port). 
Der Log Adapter dient als Rückkanal, um Support- und Betriebsnachrichten von P-PLUS zu DM7 zu senden.
Mittels CancellationToken kann ein laufender Verbindungsaufbau abgebrochen werden, beispielsweise durch extern festgestellten Timeout oder Benutzeranforderung. Alternativ kann CancellationToken.None übergeben werden, dann bricht der Verbindungsaufbau nie ab, dann kann mittels .Result ewig auf den Aufbau einer Verbindung gewartet werden oder mittels .ContinueWith asynchron auf den erfolgreichen Verbindungsaufbau reagiert werden.
Im Falle eines Verbindungsabbruchs mittels CancellationTokenSource gibt der Task dann null als Result zurück!

Die Netzwerkadresse kann zur Zeit zwei verschiedene Protokolle nutzen. 
Für den Entwicklungsbetrieb kann die Adresse "demo://nnn" verwendet werden. Hierdurch wird in-Process ein simuliertes Backend gestartet, dass im nnn-Sekunden Takt Änderungen an Mitarbeitern generiert. Zur Zeit startet das Demosystem mit 10 Mitarbeitern mit zufälligen Namen und variiert in 3 Takten jeweils einen Mitarbeiter und im 4. wird ein neuer Mitarbeiter hinzugefügt. Falls kein Wert für die Intervalllänge angegeben wird, beträgt sie 60 Sekunden.
Für den Echtbetrieb kann die Adresse "tcp://host:port" angegeben werden, unter der ein P-PLUS Server (echt oder extern simuliert) verbunden werden kann. "host" kann dabei eine IPv4 Adresse oder ein über DNS auflösbarer Hostname sein.
Die Reihenfolge des Startens ist nicht relevant. Die Verbindung bleibt auch über einen Neustart des P-PLUS Serversystems bestehen. Natürlich werden in der Zwischenzeit keine Daten aktualisiert. Es kann davon ausgegangen werden, dass nach einem P-PLUS Serverneustart, auf jeden Fall aber nach einem P-PLUS Update die DM7/P-PLUS Schnittstelle die abonnierten Daten als vollständigen Datensatz erneut übermittelt.

Das Projekt "Demo_Implementierung" bzw. die darin enthaltene Datei "DemoClientImplementierung.cs" zeigt die Verwendung der DM7/P-PLUS Schnittstelle exemplarisch. Es kann mit beiden o.a. Adressen als Kommandozeilenparameter betrieben werden (im Falle von "demo://60" startet also ein interner Demo Server).

Das Projekt "PPLUS_Demo_Server" kann verwendet werden, um Netzwerkkommunikation zu testen. Es enthält o.a. Demo Server und muss mit einem Kommandozeilenparameter der Form tcp://host:port gestartet werden. Natürlich kann auch die Demo_Implementierung mit diesem simulierten Server kommunizieren.

Das Projekt "DM7_PPLUS_Integration" enthält die eigentliche DLL in der aktuellen Version und darf ignoriert werden. Stattdessen sollte das nuget Paket und die ZIP Dateien aus dem github Projekt direkt verwendet werden.

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

CAVEAT: Zur Zeit sind die Mitarbeiter-Demodaten noch unvollständig! Vor- und Nachname und Personalnummer können erwartet werden.

## Auswahllisten
Die konkrete Bedeutung der Guids wird über ein Wörterbuch in der Auswahllisten_{Auswahllisten_Version}.cs aufgelöst. Bei Veränderungen an diesen Listen wird immer eine neue Auswahllisten_{Auswahllisten_Version+1}.cs ausgeliefert. Diese Datei ist nebenläufig zur Integration gepflegt und ermöglicht eine dynamische Anpassung der Inhalte, ohne eine Neuauslieferung der Software zu erfordern.
Wir empfehlen, diese Datei nicht statisch zu kompilieren sondern die darin enthaltenen Daten austauschbar zu verwenden.

## Offene Fragen
Gesamtversionierung muss noch geklärt werden.
Wie behandeln wir mehrere (P-PLUS) Mandanten in der Integration.

## Kollaboration
1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request :D

## Änderungen
- 0.20 (04.10.2017): **Breaking Change**
Die Connect Methode erwartet als dritten Parameter ein CancellationToken.
Damit kann ein laufender Verbindungsaufbau abgebrochen werden, beispielsweise durch extern festgestellten Timeout oder Benutzeranforderung. Alternativ kann CancellationToken.None übergeben werden, dann bricht der Verbindungsaufbau nie ab. Mittels .Result kann dann ewig auf den Aufbau einer Verbindung gewartet werden oder mittels .ContinueWith asynchron auf den erfolgreichen Verbindungsaufbau reagiert werden.
**Achtung**: Im Falle eines Verbindungsabbruchs mittels CancellationTokenSource gibt der Task dann null als Result zurück!
Die DemoClientImplementierung ist um den Fall eines Abbruchs nach Benutzeraufforderung erweitert.

- 0.10 (27.07.2017): Änderung des Aufrufs zum Starten der Client-Seite der Schnittstelle, neu: PPLUS.Connect(network_address:string, log:Log):Task<DM7_PPLUS_API>. Die Konfiguration ist weggefallen. Im Task können nach wie vor zwei Exceptions auftreten, ConnectionErrorException sowie UnsupportedVersionException. Hierbei ist zu beachten, dass diese in einer AggregateException verpackt auftreten können. Außerdem ist der PPLUS_Demo_Server hinzugekommen.

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
