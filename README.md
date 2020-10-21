# Flocking_Unity2D
## Project Description
The project implements Craig Reynolds' flocking algorithm in 2D using Unity. The flocking behaviour emerges based on three simple rules of Cohesion, Separation and Alignment. To achieve high numbers of boid/agents the simulation is implemented using the Unity Jobs System and Burst compiler. This allows for simulation of up to 5000 agents at a fast frame rate.

## Usage Help
Feel free to fork this repo if you want to play around with the project. The project requires a version of Unity that supports the Urp, since the project utilises subtle postprocessing. Preferably version 2019.4 and up.

## Build of Project
A Windows build of the project is available here: https://drive.google.com/drive/folders/1KrUVjF-6JzGsgztfXrJIKfSkMdGKRFr1?usp=sharing

Feel free to download it and give it a try. The UI allows for easy manipulation of simulation and agent settings.



## Example
![Alt Text](Gifs/Flocking5000Boids.gif)
