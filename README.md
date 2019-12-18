# Feature-Based Reinforcement Learning Using Unity
------

## Overview
This is a 5th semester Computer Science project about machine intelligence.

This project was built using [ML-Agents](https://github.com/Unity-Technologies/ml-agents) and [Unity](https://unity.com/).

------

## Installation
1. Install [python 3.6.8](https://www.python.org/downloads/release/python-368/)

2. Install the mlagents package:

   ```
   pip install mlagents=0.11.0
   ```

3. Update the setuptools package:

   ```
   pip install -U setuptools
   ```

We have provided builds that work with our python implementation of SARSA.

To change environment and agent settings or make your own build:

1. Install [Unity 2019.2.11f1](https://unity3d.com/unity/whats-new/2019.2.11)
2. Use it to open the P5/ml-agents-0.11.0/**UnitySDK** folder
3. Inside Unity, use the Project window to open a scene file in Assets/P5 (preferably **1x3.scene**)
4. From here you can run the scene or make a new build

------

## Running

**SARSA**

To run the project using our python implementation of SARSA:

1. Open a terminal and navigate to P5/ml-agents-0.11.0/project/

3. ```
   python Main.py -n [path to build]
   ```

**PPO**

To run the project using ml-agents' implementation of PPO:

1. Open a terminal and navigate to P5/ml-agents-0.11.0/

2. ```
   mlagents-learn config.yaml --env=[path to build] --run-id=test --train
   ```

**Running with the Editor**

It is possible to run the training in the Unity Editor instead of a build.

If no build is specified, python will print:
*Start training by pressing the Play button in the Unity Editor.*

If you then press play and the Unity and Python ports match, they will connect and start training.

Our SARSA and Unity's PPO use different ports and amounts of observations.

**SARSA configuration**

- On the Academy object, set *Communicator Port* to **Our Python Script**
- On the Robot object
  - On the RobotAgent component, **disable** *Limited Observations*
  - On the Behavior Parameters component, set *Observations* to **92**

**PPO configuration**

- On the Academy object, set *Communicator Port* to **Default Training**
- On the Robot object
  - On the RobotAgent component, **enable** *Limited Observations*
  - On the Behavior Parameters component, set *Observations* to **46**

------

```
python Main.py -help
```

```
mlagents-learn with -help
```