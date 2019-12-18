# P5 (Project name TBD)
## Overview
This is a project

## Installation
1. Install [python 3.6.8](https://www.python.org/downloads/release/python-368/)

2. Install the mlagents package:

   ```python
   pip install mlagents=0.11.0
   ```

3. Update the setuptools package:

   ```python
   pip install -U setuptools
   ```

We have provided builds that work with our python implementation of SARSA.

To change environment and agent settings or make your own build:

1. Install [Unity 2019.2.11f1](https://unity3d.com/unity/whats-new/2019.2.11)
2. Use it to open the P5/ml-agents-0.11.0/**UnitySDK** folder
3. Inside Unity, use the Project window to open a scene file in Assets/P5 (preferably **1x3.scene**)
4. From here you can run the scene or make a new build

## Running

It is possible to run the training in the Unity Editor or in a build.

If no build is specified, python will print:
*Start training by pressing the Play button in the Unity Editor.*

If you then press play and the Unity and Python ports match, they will connect and start training.

**SARSA**

To run the project using our python implementation of SARSA:

1. Open a terminal

2. Navigate to P5/ml-agents-0.11.0/project/

3. ```python
   python Main.py -n 
   ```
   
4. sss

**PPO**

To run the project using ml-agents' implementation of PPO:

1. Open a terminal

2. Navigate to P5/ml-agents-0.11.0/

3. ```python
   mlagents-learn config.yaml --run-id=test --train
   ```

4. sss

## Documentation
This project was built using [ML-Agents](https://github.com/Unity-Technologies/ml-agents) and [Unity](https://unity.com/).

