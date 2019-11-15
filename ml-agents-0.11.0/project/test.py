'''
Notes for interaction with Robot Agent:

Observations collected in CollectObservations are as follows:
- Robot position (x)
- Robot position (z)
- Debris position (x)
- Debris position (z)
RESPECTIVELY

Actions performed in AgentAction are as follows:
- SetTorque (Accelerate)
- SetAngle (Wheels)
- RotateArm
- RotateShovel
RESPECTIVELY

This note will be updated as the project goes on.
'''

# import matplotlib.pyplot as plt
import numpy as np

from mlagents.envs.environment import UnityEnvironment


'''Setup for connection with Python and Unity'''
# Connect to Unity and get environment
env = UnityEnvironment(file_name=None, worker_id=0, seed=1)

# Reset the environment
env_info = env.reset(train_mode=True)

# Set the default brain to work with
default_brain = "Robot"
brain = env_info[default_brain]

# Global variables for observation of environment
observations = None
robot_position = None
debris_position = None


# Update observations variable with information about the environment
def update_observations():
    global observations
    global robot_position
    global debris_position
    observations = env_info[default_brain].vector_observations[0]

    # Assets/P5/Scripts/RobotAgent.cs - CollectObservations
    robot_position = [observations[0], observations[1]]
    debris_position = [observations[2], observations[3]]


# Action functions
# Throttle wheels forward or backwards (1 forwards and -1 backwards
def move_wheels(throttle):
    # Assets/P5/Scripts/RobotAgent.cs - AgentAction
    action = np.array([throttle, 0, 0, 0])
    return env.step({default_brain: action}, memory=None, text_action=None)


# Prints position of robot and debris
def print_positions():
    print("Robot position:"
          "\nx: " + str(observations[0]) +
          "\nz: " + str(observations[1]))
    print("Debris position:"
          "\nx: " + str(observations[2]) +
          "\nz: " + str(observations[3]))


# Main function which will be run after the above code has finished (Setup of connection and definition of functions)
if __name__ == '__main__':
    # Update observations, robot_position and debris position for the default brain
    update_observations()

    # Examine the state space for the default brain
    print("Agent state looks like: \n{}".format(observations))

    # Print initial positions
    print("\nInitial: Positions of observations:")
    print_positions()

    # Drive the robot until the debris and the robot has the same X value
    while debris_position[0] - robot_position[0] > 0:
        # Update information about the environment after action/step is performed
        env_info = move_wheels(1)

        # Update observations
        update_observations()

    # Print ended positions
    print("\nEnded: Positions of observations:")
    print_positions()

    # Close simulation
    env.close()
