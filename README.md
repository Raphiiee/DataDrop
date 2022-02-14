# DataDrop
Dieses Projekt beinhaltet eine Android sowie eine Windows App. <br>
Diese Apps sollen Dateien wie PDFs, Foto oder andere gespeicherte Daten übertragen. <br>
Dabei soll es möglich sein Dateien zwischen Android und Windows oder innerhalb der Platformen Dateien auszutauschen

## Features 
* Dateiübertragung 
* Fortschrittsanzeige in Windows und Android (Benachrichtigungssystem)
* Dateien für die Übertragung Zippen (Ordner Auswahl möglich)
* Bei Verbindungsabbrüchen müssen nicht alle Daten nocheinmal Heruntergeladen werden (Blöcke einteilen & Checksum bilden) 
* WIFI-Direct (wenn die Geräte nicht im selben WLAN-Netzwerk sind wird ein neues erstellt)
* QR-Code für die einfach Verbindungskonfiguration der Geräte
* Veröffentlichung der Android-App im Playstore und der Windows-App im Windows-Store
<br>

## Schritte 
1. KW: 7 Windows-App: Rest Server mit Rohbau-GUI (Windows kann nur senden im selben Netzwerk; Statisches Textfile;)
2. KW: 7 Android-App: Kann nur empfangen; Statische IP (ohne PC Findung)
3. KW: 8 Windows-App: Fortschrittsanzeige, Dateiauswahl
4. KW: 8 Android-App: Fortschrittsanzeige (Benachrichtigungssystem), Dateiauswahl, Datei senden
5. KW: 9 Windows-App: Dateien Empfangen, Ent-/Zippen
6. KW: 9 Android-App: Ent-/Zippen
7. KW: 10 Windows-App: Senden: Die zu Übertragende Datei in Blöcke einteilen und Checksum bilden
8. KW: 11 Android-App: Empfangen: Checksum Überprüfen / Fehlerhaften Block erneut anfordern
9. KW: 12 Windows-App: Empfangen: Checksum Überprüfen / Fehlerhaften Block erneut anfordern
10. KW: 13 Android-App: Senden: Die zu Übertragende Datei in Blöcke einteilen und Checksum bilden
11. KW: 14 Windows-App: WIFI-Direct Logik
12. KW: 15 Android-App: WIFI-Direct Logik
13. KW: 16 Windows-App: QR-Code generieren für einfaches Verbinden
14. KW: 17 Android-App: QR-Code generieren für einfaches Verbinden
17. KW: 18 Veröffentlichung der Apps

## Aktuelle Umsetzung
KW: 6 Projektstart | Schritt 1-2


## Wichtige Termine
* 26.04.2022 Vorläufige Endversion 
* 23.05.2022 Abgabe Bachelorarbeit
* 07.06.2022 Berurteilung/Feedback Berufspraktikum
* 08.06.2022 Beurteilung Bachelorarbeit