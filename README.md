# FortniteStatsDesktop

**FortniteStatsDesktop** est une application Windows performante et moderne conçue pour l'analyse locale et automatique des fichiers de rediffusion (.replay) de Fortnite. 

Elle permet aux joueurs de suivre leurs performances, d'analyser leurs statistiques de combat et de visualiser l'historique de leurs parties avec une précision chirurgicale, sans dépendre de services tiers lents ou payants.

---

## 🚀 Fonctionnalités Clés

- **Surveillance en Temps Réel** : L'application surveille automatiquement votre dossier de replays Fortnite et analyse chaque nouvelle partie dès qu'elle est terminée.
- **Parsing Avancé** : Extraction détaillée du classement (Top), du nombre d'éliminations, de la précision, des dégâts infligés/subis et des matériaux utilisés.
- **Tableau de Bord Global** : Visualisez vos statistiques agrégées (K/D moyen, placement moyen, total de kills) au fil du temps.
- **Détails du Match** : Consultez le Leaderboard complet de vos parties, le Kill Feed détaillé et les causes de mort de chaque joueur.
- **Drag & Drop** : Analysez instantanément n'importe quel fichier de replay en le glissant simplement dans l'application.
- **Interface Hybride** : Profitez de la puissance de .NET 10 combinée à la fluidité d'une interface web moderne via Blazor Hybrid.

---

## 🛠️ Stack Technique

- **Framework** : .NET 10.0 (Windows Desktop)
- **Interface** : WPF (Windows Presentation Foundation) avec **Blazor Hybrid** (WebView2)
- **Langages** : C#, Razor, HTML5, CSS3 (Bootstrap 5)
- **Analyse de Replays** : [FortniteReplayReader](https://www.nuget.org/packages/FortniteReplayReader)
- **Persistance** : Fichiers JSON (Migration vers SQLite prévue)

---

## ⚙️ Installation et Développement

### Prérequis
- [SDK .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0) ou supérieur.
- Windows 10/11 (Architecture x64).

### Compilation
1. Clonez le dépôt.
2. Restaurez les packages :
   ```bash
   dotnet restore
   ```
3. Lancez l'application en mode développement :
   ```bash
   dotnet run
   ```

---

## ⚠️ Statut du Projet

> [!IMPORTANT]
> Cette application est actuellement en **phase de développement actif**. Certaines fonctionnalités peuvent être incomplètes et le format des données peut changer entre les versions.

---

## 🛡️ Sécurité

L'application analyse des fichiers locaux uniquement et n'envoie aucune donnée personnelle vers des serveurs externes.

**Analyse VirusTotal (Build de référence) :**  
[Consulter le rapport VirusTotal](https://www.virustotal.com/gui/file/f75d945673266ad6856a9ac8cf6089ff2994cf39e77998db23658131ff8696b6?nocache=1)

---

## 📄 Licence
Ce projet est destiné à un usage personnel et éducatif. Tous les droits sur les noms et contenus liés à Fortnite appartiennent à Epic Games, Inc.
