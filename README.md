# SEMPT3X SMTP Checker

> Dieses Programm wurde von [sempt3x](https://github.com/sempt3x) mit Unterstützung von KI erstellt.

Ein Windows-Desktop-Tool (WPF, .NET 10) zum schnellen Testen von Mailserver-Konfigurationen. Die Anwendung prüft Schritt für Schritt DNS-Auflösung, Port-Erreichbarkeit, Verbindung, Authentifizierung und Übermittlung und sendet dabei eine echte Test-E-Mail – wahlweise über einen **klassischen SMTP-Server** oder über **Microsoft 365 (Microsoft Graph)**.

## Funktionen

- **Zwei Verbindungsarten**
  - **Standard SMTP**: Versand über einen beliebigen SMTP-Server mit Benutzername/Passwort und TLS/STARTTLS
  - **Microsoft 365 / Microsoft Online**: Versand über die Microsoft Graph API (`sendMail`) per Client-Credentials-Flow einer Entra-App-Registrierung
- **Schrittweise Diagnose** mit farbigem Status pro Prüfschritt:
  1. Serverauflösung (DNS)
  2. TCP-Port
  3. Verbindung
  4. Authentifizierung
  5. E-Mail-Übermittlung
- **Test-E-Mail** mit frei wählbarem Absender, Empfänger, Betreff, Nachrichtentext und optionalen Anhängen
- **Live-Protokoll** mit Zeitstempeln (Info / Erfolg / Fehler)
- **Katalog bekannter Fehlercodes** (SMTP-Statuscodes, AADSTS-Codes, Graph-Fehler) mit möglicher Ursache und Lösung
- **Info-Fenster zu SMTP-Ports** (25, 465, 587, 2525) und Begriffen wie STARTTLS oder implizites TLS
- **Abbrechen** laufender Tests jederzeit möglich

## Sicherheit

- Passwörter und Client Secrets werden ausschließlich in `PasswordBox`-Feldern gehalten und erst beim Start eines Tests ausgelesen – sie werden nicht zwischengespeichert.
- Zugangsdaten, Tokens und Dateiinhalte werden **nicht** protokolliert.
- SMTP-Anmeldedaten werden niemals über eine unverschlüsselte Verbindung gesendet – ohne aktiviertes TLS/STARTTLS wird der Test verweigert.
- E-Mail-Adressen werden validiert; nur reine Adressen (ohne Anzeigenamen) werden akzeptiert.

## Voraussetzungen

- Windows 10/11
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (zum Bauen) bzw. .NET 10 Desktop Runtime (zum Ausführen)

## Installation & Start

```bash
git clone https://github.com/sempt3x/SBH-SMTP-Checker.git
cd SBH-SMTP-Checker
dotnet run --project SEMPT3X-SMTP-CHECKER
```

Eigenständige Version erstellen:

```bash
dotnet publish SEMPT3X-SMTP-CHECKER -c Release -r win-x64 --self-contained
```

Alternativ kann die Solution `SEMPT3X-SMTP-CHECKER.slnx` in Visual Studio oder JetBrains Rider geöffnet werden.

## Verwendung

### Standard SMTP

1. Verbindungsart **Standard SMTP Server** wählen
2. SMTP-Server, Port (z. B. `587`), Benutzername und Passwort eintragen
3. **TLS / STARTTLS verwenden** aktiviert lassen
4. Absender- und Empfängeradresse eintragen, optional Betreff, Text und Anhänge
5. **Test starten & E-Mail senden** klicken

### Microsoft 365 (Microsoft Graph)

Voraussetzung ist eine App-Registrierung in **Microsoft Entra ID**:

1. App registrieren und einen **Clientschlüssel** erstellen (den *Wert* verwenden, nicht die Schlüssel-ID)
2. Unter *API-Berechtigungen* die **Anwendungsberechtigung** `Mail.Send` für Microsoft Graph hinzufügen
3. **Administratorzustimmung** erteilen

In der Anwendung:

1. Verbindungsart **Microsoft 365 / Microsoft Online** wählen
2. Mandanten-ID (GUID oder z. B. `contoso.onmicrosoft.com`), Client-ID und Clientschlüssel eintragen
3. Absender (ein Postfach im Mandanten) und Empfänger eintragen
4. **Test starten & E-Mail senden** klicken

> **Hinweis:** Eine erfolgreiche Übermittlung bedeutet, dass der Server bzw. die API die Nachricht angenommen hat. Die endgültige Zustellung ins Postfach kann weiterhin durch nachgelagerte Richtlinien (Spamfilter, Transportregeln usw.) beeinflusst werden.

## Limits

| Verbindungsart | Max. Gesamtgröße der Anhänge |
|---|---|
| Standard SMTP | 25 MB |
| Microsoft 365 (Graph) | 3 MB |

## Bekannte Einschränkungen

- Der SMTP-Versand nutzt `System.Net.Mail.SmtpClient`, der **STARTTLS** unterstützt, aber kein **implizites TLS**. Server, die auf Port 465 ausschließlich implizites TLS (SMTPS) anbieten, schlagen daher in der Regel mit einem Timeout fehl – in diesem Fall Port 587 verwenden.

## Projektstruktur

```
SEMPT3X-SMTP-CHECKER/
├── Models/        # Datenmodelle (Status, Log-Einträge, Fehlerdefinitionen)
├── Services/      # DNS/TCP-Tests, SMTP-Versand, Microsoft Graph, Fehlerkatalog, Logger
├── ViewModels/    # MVVM-ViewModels und Commands
├── MainWindow.xaml          # Hauptfenster
├── ErrorCodesWindow.xaml    # Übersicht bekannter Fehlercodes
└── SmtpPortInfoWindow.xaml  # Infos zu SMTP-Ports
```

## Technologie

- C# / .NET 10 (`net10.0-windows`)
- WPF mit MVVM-Muster
- Keine externen NuGet-Abhängigkeiten
