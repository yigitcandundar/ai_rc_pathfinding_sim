# INF 3910-6-6 Artificial Intelligence Term Project

## Setup & Installation
+ Download and Install Unity 2018.2.1f1 (Updated versions may or may not work)
+ Navigate to Assets/Scenes/ folder
+ Double click (or open) "MainScene.unity" file
+ In the Unity editor hit the play button to run the AI


## Source Code
All of the scripts that are used in the simulation are located inside the Assets/Scripts/ folder.
+ *CarAI* Script manages the AI's relation with its sensors and wheel motor. It also keeps track of the progress and handles every UI feature related to the Car AI
+ *MotorBehavior* Script provides methods to control the wheels of the car
+ *AreaProximitySensorBehaviour* Script detects nearby objects and returns a normalized direction with respect to the goal position. This is the script where the actual Potential Field inspired logic lies in.
+ *GoalManager* Keeps track of the current number of goal positions
+ *CameraSensorBehaviour* This script analyzes the camera feed to find the objectives and feed that information to the Car AI
+ *SwitchCameras* This script is a utility script that manages the simulation camera angles (Dashboard Camera & 3rd Person Camera)
