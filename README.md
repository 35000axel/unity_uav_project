# AeroTrack - Projet étudiant

AeroTrack est un projet créé pour surveiller l'état des piste d'un aéroport à l'aide de 1 ou plusieur drone.
L'objectif de ce projet est de déterminer si une piste est en état d'être utilisée pour l'atterrissage ou le décollage d'un avion en détectant les éventuels trou ou autres dégradations. Pour ce faire, les drones parcourent la piste de chaque extrémité jusqu'au milieu, puis retournent à leur point de départ.

<p align="center"><img src="scene.png"/></p>

## Requirements

Pour commencer à exécuter ce projet, vous devez suivre quelques étapes. Tout d'abord, vous devez avoir Unity installé (version 20.3.4f), et deuxièmement, vous devriez télécharger/cloner le projet. Les assets visuel sont déjà présent dans le projet.

### Présentation

Pour lancer la simulation il faudra simplement sélectionner la seul scène présente et lancer la simulation.

Les drones ne sont pas présent sur la scène et sont instancié par la station.
La station a besoin des prefabs des drones pour les instancier et des setPoints (Vector3) correspondant au deux extrémité de la piste.
L'ajout et la supression d'obstacles se fait dans le gameObject Obstacles.
Les images des dégradations capturées par les drones sont enregistré dans Assets/Images/.
