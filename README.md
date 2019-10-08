# Code Samples
This project was made in 2017 by Alberto Gómez. It consists of a simple scene with a set of objects used to test functionalities implemented in different scripts located in the assets folder. It can be described as a custom third camera character controller.
Right now I don't have the time to redo the scripts in this project, which are 2 years old at this point and sometimes seem a bit old. However, I remember `FreeCameraMovementScript.cs` as a script I was really proud of at the time, so I would recommend checking that one out if the time to revise this project is a constrain.

## Key Features
### Input
* Mouse + Keyboard and Gamepad input support.

### Player Character
- Movement Script
    * It supports movement on ramps.
    * Makes the player slide on steep slopes.
    * It support movement on top of moving and rotating platforms, correctly preserving the platform movement.
    * It corrects the movement direction when the user tries to run against a wall.
- Visuals
    * Simple character model with basic animations.
    * Particle System that makes use of textures with normal maps to give tbe illusion of a 3D model.
        
### Camera
* Free camera rotation around the player character.
* Auto camera rotation (when playing with a controller and moving off screen).
* Reset the camera to its default position by pressing the Right Thumb Stick, the Mouse Wheel or TAB).
* Enemy z-lock by reseting the camera close to an enemy (the yellow Spheres in the scene.)(Keyboard: tab to release the lock, Q and E to move the tagert. Controller: Right Thumb Stick to release the lock, Right Stick Left and Right Stick Right to move the tagert.)
* Transition in and out to fixed camera angles (walk behind the wall in the scene).
* Camera movement with object clipping prevention.

### Game
* The different elements in the game are grouped in layers in order to improve the performance when using Physics methods such as SpehereCast or RayCast.