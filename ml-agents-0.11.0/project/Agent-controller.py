"""
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
"""

# import matplotlib.pyplot as plt
import numpy as np
import time

from mlagents.envs.environment import UnityEnvironment

# Global environment variables
env = None
env_info = None
default_brain = None
brain = None

# Global variables for observation of environment
observations = None
robot_position = None
debris_position = None


# Setup connection between Unity and Python
def setup_connection_with_unity():
    global env
    global env_info
    global default_brain
    global brain

    # Connect to Unity and get environment
    env = UnityEnvironment(file_name=None, worker_id=0, seed=1)

    # Reset the environment
    env_info = env.reset(train_mode=True)

    # Set the default brain to work with
    default_brain = "Robot"
    brain = env_info[default_brain]


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
# Throttle wheels forward or backwards (1 forwards and -1 backwards)
# Turn wheels right or left (1 right and -1 left)
# Rotate arm up or down (1 down and -1 up)
# Rotate shovel up or down (1 down and -1 up)
def perform_action(throttle, angle, arm_rotation, shovel_rotation):
    # Assets/P5/Scripts/RobotAgent.cs - AgentAction
    action = np.array([throttle, angle, arm_rotation, shovel_rotation])
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
    # Setup connection
    setup_connection_with_unity()

    # Start time
    start_time = time.time()

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
        env_info = perform_action(1, 0, 0, 0)

        # Update observations
        update_observations()

    # Print ended positions
    print("\nEnded: Positions of observations:")
    print_positions()

    # Close simulation
    env.close()

    # End time
    end_time = time.time()
    print("\nTime to reach goal start: " + str(end_time - start_time))
