# Contexte du Projet : FortniteStatsDesktop

## Vue d'ensemble Fonctionnelle
**FortniteStatsDesktop** est une application de bureau conçue pour analyser, stocker et afficher des statistiques détaillées à partir de fichiers de rediffusion (replays) du jeu Fortnite. L'application agit comme un tableau de bord (dashboard) personnel permettant aux joueurs de suivre leurs performances, de consulter l'historique de leurs parties, d'analyser leurs statistiques globales et d'obtenir des détails précis sur chaque match (classement, éliminations, cartes, etc.). 

L'application supporte le traitement automatique des replays (via un watcher de dossier) ainsi que le drag-and-drop manuel de fichiers de replay.

## Architecture Technique

### Stack Technologique Principal
*   **Plateforme** : .NET 10 (Target: `net10.0-windows10.0.17763.0`)
*   **Type d'application** : WPF (Windows Presentation Foundation) couplé à Blazor Hybrid (via `Microsoft.AspNetCore.Components.WebView.Wpf`).
*   **Interface Utilisateur (UI)** : Composants Razor (Blazor), HTML/CSS, Bootstrap.
*   **Analyse de Replays** : Librairie tierce `FortniteReplayReader` (v3.0.0).

### Modèle Architectural
L'application suit une architecture basée sur des composants pour l'interface utilisateur (Blazor) et des services injectés (Dependency Injection) pour la logique métier et l'accès aux données. L'application WPF (`MainWindow.xaml`) sert principalement de conteneur hôte (WebView2) pour l'application Blazor.

### Services Principaux (`/Services`)
*   **`ReplayService`** : Coeur de la logique de lecture et de parsing des fichiers `.replay` utilisant `FortniteReplayReader`.
*   **`ReplayWatcherService`** : Service d'arrière-plan surveillant le dossier de replays de Fortnite pour détecter de nouvelles parties automatiquement.
*   **`MatchDataService`** : Gère la persistance, le chargement et la sauvegarde des données de matchs analysés.
*   **`SettingsService`** : Gère les préférences de l'utilisateur (ex: auto-navigation après l'analyse d'une partie).
*   **`ReplayEventService`** : Bus d'événements interne pour la communication entre les services d'arrière-plan et l'UI (ex: notification de début et fin d'analyse).
*   **`DragDropService`** : Facilite la communication des événements de glisser-déposer de la fenêtre WPF native vers l'environnement Blazor.

### Interface Utilisateur (`/Pages` & `/Shared`)
*   **Structure** : Layout principal (`MainLayout.razor`) avec barre de navigation latérale et un système d'overlay pour les notifications d'analyse en temps réel.
*   **Vues clés** :
    *   `Home.razor` : Tableau de bord principal (résumés, statistiques rapides).
    *   `Games.razor` : Historique / Liste des parties enregistrées.
    *   `GameDetails.razor` : Vue détaillée d'une partie spécifique (leaderboard, événements, carte).
    *   `Stats.razor` : Statistiques globales agrégées.
    *   `Settings.razor` : Configuration de l'application.

## Fonctionnalités Clés
1.  **Parsing de Replay** : Extraction des métadonnées, du classement, des éliminations et potentiellement des mouvements depuis les fichiers bruts.
2.  **Surveillance en temps réel** : Détection de la fin d'une partie et analyse automatique en arrière-plan.
3.  **Notifications UI** : Overlay interactif in-app informant l'utilisateur du statut de l'analyse et offrant un accès rapide aux résultats.
4.  **Drag & Drop** : Possibilité d'analyser d'anciens replays en les glissant simplement dans l'application.
5.  **Visualisation de Données** : Affichage des statistiques sous forme de tableaux, listes et résumés.

## Points d'Attention Actuels
*   L'intégration WPF/Blazor nécessite une gestion fine des événements natifs (comme le Drag&Drop ou les problèmes de rendu liés à WebView2, contournés via des arguments sRGB spécifiques).
*   L'application est fortement dépendante du format des fichiers `.replay` d'Epic Games, qui peut changer à chaque mise à jour du jeu. Le maintien de `FortniteReplayReader` est crucial.
