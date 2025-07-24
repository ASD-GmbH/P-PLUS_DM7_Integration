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
var api = PPLUS.Connect(url,credentials,log,cancellationToken_Verbindung).Result;
``` 
erstellt werden.

Format für `url` (weitere Erklärung siehe unten): `tcp://ip:port|publickey`

Format für `credentials`: `key|secret`

## Beispiel - weitere Erklärung siehe unten 
(key = ping, secret = pong)

```c#
var log = // eigener Logger
var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken_Verbindung = cancellationTokenSource.Token;
PPLUS.Connect("tcp://127.0.0.1:21000|PFJTQUtleVZhbHVlP[...]V5VmFsdWU+", "ping|pong", log, cancellationToken_Verbindung); 
```


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
- 0.16.4.1 (24.07.2025) Erweiterung der DemoClientImplementierung um die Abrage des DienstbuchungsUeberwachungszeitraums

- 0.16.4 (02.07.2025) Event für StaP-Relevante Dienstbuchungsänderungen in P-PLUS / Query für DienstbuchungsUeberwachungszeitraum

- 0.16.3.1 (24.10.2024) [Fix] Die nach einem Bug in 0.16.3 verworfene Verbindung wird nun wieder aufrecht erhalten, was die Events bei Änderungen an Personen-Stammdaten und Diensten wieder berücksichtigt.

- 0.16.3 (30.09.2024) PPLUS-Authenticate gibt Port wieder frei.

- 0.16.2 Mitarbeiterinfos um Personen-UUID erweitert.

- 0.15 (08.11.2018): Rückgabewerte statt Exceptions, so dass auf Verbindungsfehler und Authentifizierungsfehler serverseitig besser reagiert werden kann.

- 0.14 (05.11.2018): Verschlüsselung und Authentifizierung
Die Datenübertragung erfolgt jetzt voll verschlüsselt. Bei der Anmeldung angegebene Credentials werden an P-PLUS weitergeleitet, um dort überprüft zu werden. 
Der Connectionstring für den Client muss jetzt den Public Key des Servers enthalten, Format: tcp://ip:port|publickey
Der Demo Server gibt den public key beim Start aus. Ausserdem kann der Demo Server nach dem gleichen Muster mit einem Schlüsselpaar initialisiert werden. Wird dieses nicht angegeben, wird ein Schlüsselpaar erzeugt und in der Konsole ausgegeben, so dass ein Neustart mit vorherigem Schlüssel möglich ist.

### Breaking Changes

 * Das Netzwerkprotokoll ist aufgrund der Verschlüsselung nicht mehr kompatibel zu Version 0.13
 * Die Connect Methode erwartet jetzt als zweiten Parameter einen String mit Credentials. Dieser wird an P-PLUS übertragen. Der Inhalt ist nicht Teil dieser Schnittstelle.


- 0.13 (30.08.2018): Grundlegende Änderungen

Überleitung der Stammdaten:
Da eine Person mehrere Vertragsverhältnisse mit unterschiedlichen Personalnummern oder Strukturzuordnungen haben kann übergeben wir zukünftig je Person mehrere Datensätze mit 
1. einer PersonId, die bei mehreren Datensätzen gleich sein könnte,
2. DatensatzId des Datensatzes mit den Informationen und 
3. einer gültigen Strukturexportnummer
Aufgrund der Tatsache, dass ein MA innerhalb einer Datenbank von einem Mandanten in den nächsten Mandanten wechseln könnte und somit nur eine Befristung auf einer Struktur (Strukturnummer in Kombination mit Mandantennummer) hätte und kein Austrittsdatum, werden wir in jedem Datensatz ein Gültig von und ein Gültig bis übergeben und nicht Eintrit und Austritt, was ggfs. für DM7 gleichbedeutend ist. Auch wenn der MA innerhalb eines Mandanten einen Strukturwechsel durchläuft (Bereichswechsel) würde es entsprechend viele Datensätze für einen Personenid geben)

### Breaking Changes

#### API Version
Das API Version wird von 2 auf 3 angehoben.
Die Rückwärtskompatibilität zu P-PLUS Servern mit dem Protokoll API Version 2 besteht nicht (pre-release).

#### Mitarbeiterdatensatz.DatensatzId
Ein neues Feld DatensatzId: string wird eingeführt. Dieses übernimmt die Funktion des bisherigen Feldes Id in Bezug auf Aktualisierung des Datensatzes. Entsprechend ändert dich der Datentyp für Referenzlisten von Guid auf string.

#### Mitarbeiterdatensatz.Id / Mitarbeiterdatensatz.PersonId
Das bisherige Feld Id wird in PersonId umbenannt und zum Typ string. Es korreliert verschiedene Datensätze zu einer Person und ist damit nicht mehr verlässlich eindeutig je Datensatz.
Mehrere Datensätze entstehen bspw. bei auf mehrere Strukturen verteilten Mitarbeitern (zeitlich parallel) sowie bei Strukturwechsel, und Elternzeit in Verbindung mit zwischenzeitlicher geringfügiger Beschäftigung (zeitlich seriell).

#### Mitarbeiterdatensatz.ArbeitsverhaeltnisId
Id des Arbeitsverhältnisses des Mitarbeiters (P-PLUS Konzept Mitarbeiter)

#### Mitarbeiterdatensatz.Titel
Neues Feld Titel: string, dass Titel der Person als Text enthält.

#### Mitarbeiterdatensatz.Mandanten
Der Typ ändert sich von ReadonlyCollection<int> zu string. Es enthält in der Mitarbeiter Exportkonfiguration einstellbare Daten (DM Mandantennummer). Wird ein Mitarbeiter für mehrere Strukturen und/oder Mandanten exportiert, entstehen mehrere Datensätze, die über das Feld PersonIs korrelierbar sind.

#### Mitarbeiterdatensatz.Struktur
Neues Feld Struktur: string, dass die P-PLUS Strukturexportnummer des Mitarbeiters enthält.

#### Mitarbeiterdatensatz.Eintritt/Mitarbeiterdatensatz.Austritt / Mitarbeiterdatensatz.GueltigAb/Mitarbeiterdatensatz.GueltigBis
Die Felder Eintritt und Austritt werden jeweils in GueltigAb und GueltigBis umbenannt. Ihr Inhalt spiegelt die Anwendbarkeit der im Datensatz übertragenen Daten wieder. Dies kann, muss aber nicht auf Eintritt und Austritt beschränkt sein. So führt bspw. ein Strukturwechsel dazu, dass der bisherige Datensatz einen GueltigBis Wert der Vortags des Strukturwechsels bekommt und für die neue Struktur ein Datensatz mit GueltigAb Wert ab dem Tag des Strukturwechsels angelegt wird.



- 0.12 (27.10.2017): Vollständige Serialisierung
Die vollständige Serialisierung ist als API Version 2 implementiert.
Das externe Interface ist unverändert.


- 0.11 (04.10.2017): **Breaking Change**
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
