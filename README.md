# Pipeline-ViewModel
Infrasctructure de build d'un VM des services au controller.

# Dataflow / Build Pipeline :

Orchestrator → orchestration
Step → transformation
Context → transport d’état

1. l’orchestrator :
- valide le graphe
- ordonne les steps
- exécute par batch (potentiellement parallélisable)

2. chaque Step :
- déclare ses dépendances
- produit un artefact typé

3. le BuildContext est un bus de données



# 📝 Architecture des orchestrators

## 1️⃣ Génériques en C#

### Qu’est-ce que c’est ?

* Un **générique** permet de créer des classes, interfaces ou méthodes qui **fonctionnent avec différents types** sans dupliquer le code.
* Exemple : `OrchestratorBase<TParameters, TViewModel>` est un **template** pour n’importe quel orchestrator qui transforme des paramètres en un ViewModel.

### Pourquoi les utiliser ?

* **Réduction du code répétitif** : plus besoin d’écrire `BuildVmX` pour chaque orchestrator.
* **Type-safe** : les erreurs de type sont détectées à la compilation, pas au runtime.
* **Extensible** : ajouter un nouvel orchestrator ne nécessite qu’une “configuration”, pas de code complexe.

### Quand les utiliser ?

* Quand **la logique est la même mais que les types changent** (ex : BuildContext → ViewModel).
* Quand tu veux imposer une **convention ou un contrat** entre classes.

### Quand ne pas les utiliser ?

* Si le comportement **change radicalement selon le type**.
* Si tu dois faire des **switch type ou reflection**, tu perds la sécurité des génériques.

---

## 2️⃣ DAG (Directed Acyclic Graph)

### Qu’est-ce que c’est ?

* Un DAG est un **graphe orienté sans cycle**.
* Ici, il sert à organiser des **étapes (`IStep`)** qui dépendent les unes des autres : certaines étapes doivent être exécutées avant d’autres.

### Pourquoi l’utiliser ?
* **Dépendances explicites** : Poour chaque étape, on explicite les dépendances. 
* **Ordre automatique** : tu n’as pas besoin de coder manuellement l’ordre des exécutions.
* **Détection de problèmes** : cycles, doublons ou dépendances manquantes peuvent être détectés.
* **Extensible** : tu peux ajouter des étapes indépendantes sans casser le pipeline.

### Quand les utiliser ?

* Quand tu as **plusieurs étapes avec dépendances**, comme un calcul complexe ou un traitement par batch.
* Quand tu veux **garantir que les dépendances sont respectées**.

### Quand ne pas les utiliser ?

* Si les étapes sont strictement séquentielles et simples → un simple foreach suffit.
* Si tu n’as **aucune dépendance** entre les étapes → un DAG est overkill.

---

## 3️⃣ BuildContext
Le builder exécute un pipeline d’étapes typées opérant sur un contexte interne.
Ce contexte stocke les résultats intermédiaires et garantit la cohérence des dépendances.
Une fois le pipeline exécuté, le contexte est projeté en ViewModel final.

### Qu’est-ce que c’est ?

* C’est un **contenant de données** pour partager les résultats des étapes (`IStep`) entre elles.
* Chaque étape peut **mettre à disposition un résultat** qui sera utilisé par d’autres étapes ou par le converter.

### Pourquoi c’est une bonne pratique ?

* **Découplage** : les étapes ne se connaissent pas entre elles, elles n’accèdent qu’au `BuildContext`.
* **Testabilité** : tu peux tester chaque étape indépendamment en simulant le context.
* **Extensible** : ajouter un nouveau résultat n’impacte pas les étapes existantes.

### Quand l’utiliser ?

* Quand des étapes partagent des **résultats intermédiaires**.
* Quand tu veux **séparer les calculs (context) de la construction finale (ViewModel)**.

### Quand ne pas l’utiliser ?

* Si tu n’as qu’une seule étape ou un simple mapping → un context serait inutile.


---

## 4️⃣ Convertisseurs VM (BuildContext → ViewModel)

### Qu’est-ce que c’est ?

* Chaque **ViewModel** a son **converter spécifique** qui transforme le BuildContext en ViewModel.
* Cela sépare le **calcul / collecte de données** (DAG et context) de la **représentation finale** (VM).

### Pourquoi c’est une bonne pratique ?

* **Séparation des responsabilités** : l’orchestrator ne construit pas le ViewModel, il se contente de fournir les données.
* **Testabilité** : tu peux tester les converters indépendamment.
* **Clarté** : chaque VM a sa logique isolée, pas de code multi-VM dans le même converter.

### Quand l’utiliser ?

* Quand les **VM ont des logiques de mapping différentes**.
* Quand tu veux un **pipeline clair** : données → context → VM.

### Quand ne pas l’utiliser ?

* Si le mapping est trivial et identique pour toutes les VM → tu pourrais le faire dans l’orchestrator directement.

---

## 5️⃣ Orchestrator générique

### Qu’est-ce que c’est ?

* Une classe qui prend un **BuildContext** et un **converter** et produit un **ViewModel**.
* La base générique réduit le boilerplate et impose la convention de nommage.

### Pourquoi c’est une bonne pratique ?

* **Réutilisable** pour tous les pipelines qui suivent le même pattern.
* **Contrat clair** : BuildContext → ViewModel.
* **Minimise les erreurs humaines** : impossible de créer un orchestrator sans respecter la convention.

### Quand l’utiliser ?

* Quand tu as **plusieurs pipelines avec la même structure** (DAG + Context + Converter).

### Quand ne pas l’utiliser ?

* Si tu as un pipeline complètement différent pour lequel la structure générique ne s’applique pas.

---

## 6️⃣ DI (Dependency Injection)

### Ce qu’on a fait

* Autofac est utilisé pour injecter **steps**, **services**, **converter** et **orchestrators**.
* Grâce aux **interfaces génériques**, tout est type-safe.

### Pourquoi c’est une bonne pratique ?

* **Découplage complet** : aucune classe ne connaît la création des dépendances.
* **Extensible** : ajouter une étape ou un orchestrator ne nécessite pas de modifier les autres.
* **Testable** : tu peux remplacer n’importe quelle dépendance par un mock.

---

## 7️⃣ Points additionnels pertinents

* **Type-safe DI** : avec des interfaces génériques, Autofac sait exactement quel orchestrator et converter injecter.
* **Convention sur les types** : BuildParametersX → XViewModel → ConverterX → OrchestratorX
* **Testabilité à tous les niveaux** :
  * Steps isolés avec BuildContext
  * Converters indépendants
  * Orchestrators avec DAG simulé
* **Extensibilité** : ajouter un pipeline ne touche pas les existants.

---

## 💡 Synthèse / Pourquoi cette architecture est meilleure

1. **Séparation des responsabilités** :

   * DAG = orchestration des étapes
   * BuildContext = stockage intermédiaire
   * Converter = mapping vers VM
   * Orchestrator = orchestration + binding

2. **Réutilisation et génériques** :

   * Pas de duplication du pattern “BuildVmX”
   * Type-safe, compilation sécurisée

3. **Testabilité** :

   * Chaque composant peut être testé isolément

4. **Extensibilité** :

   * Ajouter un pipeline = créer DAG + converter + orchestrator minimal

5. **Clarté pour les développeurs** :

   * Convention imposée par types et DI
   * Lecture du pipeline directe via le DAG et le converter

---

         ┌──────────────────────┐
         │  BuildParametersX    │
         │  (input)             │
         └─────────┬────────────┘
                   │
                   ▼
         ┌──────────────────────┐
         │     OrchestratorX    │
         │ (OrchestratorBase)   │
         └─────────┬────────────┘
                   │
                   ▼
         ┌──────────────────────┐
         │        DAG           │
         │ (IStep[], topological│
         │  sort batches)       │
         └─────────┬────────────┘
                   │
                   ▼
         ┌──────────────────────┐
         │     Steps (IStep)    │
         │ - CriteriaStep       │
         │ - ProductsStep       │
         │ - FiltersStep        │
         │ - ...                │
         └─────────┬────────────┘
                   │
                   ▼
         ┌──────────────────────┐
         │   BuildContext       │
         │  (intermediate store │
         │   for step outputs)  │
         └─────────┬────────────┘
                   │
                   ▼
         ┌──────────────────────┐
         │ ConverterX            │
         │ (BuildContext → VM)   │
         │  - ListeResultatsVM   │
         │  - BViewModel         │
         └─────────┬────────────┘
                   │
                   ▼
         ┌──────────────────────┐
         │  ViewModel (VM)      │
         │  - ListeResultatsVM  │
         │  - BViewModel        │
         └─────────┬────────────┘
                   │
                   ▼
         ┌──────────────────────┐
         │      Controller      │
         │  return View(VM)     │
         └──────────────────────┘

# Points à mettre en avant avec ce schéma

1. Contrôleur = léger

Plus besoin de gérer toutes les étapes, il reçoit directement le ViewModel prêt.

2. Orchestrator = pipeline

Compose toutes les étapes, gère l’ordre via DAG, stocke les résultats intermédiaires dans BuildContext.

3. Steps = unités de travail indépendantes

Chacune fait un calcul spécifique.
Dépendances déclaratives via DAG → pas d’ordre codé en dur.

4. BuildContext = médiateur

Partage les résultats intermédiaires entre Steps.
Découplage fort entre les étapes.

5. Converter = mapping final

Transforme le BuildContext en ViewModel spécifique.
Chaque VM a son converter → logique claire et isolée.

6. DI & génériques

Tout est type-safe et injecté : impossible d’oublier une dépendance.
Orchestrator et Converter sont liés par types → convention imposée.
