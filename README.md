# Mazeman

<img src="https://user-images.githubusercontent.com/64329402/166218722-adec154e-989a-47ae-bdd3-e73d715d9cfe.jpg" width="500">

## Description
Mazeman is a one- or two-player maze-chase game, in which the size of the maze can be chosen by the player. 
The objective of the game is to collect as many points as possible, while avoiding the enemy "ghosts" which follow the players.
The number of enemies is directly related to the size of the maze, while the speed is dependent on the chosen difficulty.

During a game, collectible powerups are regularly generated in the maze. 
These powerups can be collected by any player (or enemy!) and their effect is applied universally, for a randomly-determined amount of time.

<img src="https://user-images.githubusercontent.com/64329402/166217967-6907d049-0ed0-4c4c-bb8f-31719bd13c17.jpg" width="400">

## Screenshots

### Game on 25x15
<img src="https://user-images.githubusercontent.com/64329402/166218724-00916fe9-0997-4c73-88b4-d96c20d6ec4a.jpg" width="400">

### Game on 25x25
<img src="https://user-images.githubusercontent.com/64329402/166221298-36df5136-a373-4b00-b906-d683a3e37197.jpg" width="400">

### Menu/Settings Window

<img src="https://user-images.githubusercontent.com/64329402/165782792-fc7e649e-24ae-4653-9226-20d5ed23a4b7.jpg" width="400">


### Viewing Previous Scores (Game History)
Users can choose to save their score under a pseudonym and then view the history of game scores under that name.

Bar colour indicates game difficulty (green = easy, yellow = medium, red = hard).

<img src="https://user-images.githubusercontent.com/64329402/166218725-0443dbb1-c4c9-48b6-b198-b79518b9e8f2.jpg" width="400">

Clicking a bar shows detailed information about that game.

<img src="https://user-images.githubusercontent.com/64329402/166218726-541ce49c-de29-401f-8936-ed8088c51fd5.jpg" width="400">

## Algorithms
The maze is generated using a Recursive Backtracker, while the enemy path-finding is handled by the A* Search algorithm.

## Libraries/References
- [Newtonsoft.Json](https://www.newtonsoft.com/json) - JSON (de)serialisation for file reading/writing, by James Newton-King. Used under MIT licence.
- [OptimizedPriorityQueue-4.2.0](https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp/) - by BlueRaja
