# import matplotlib.pyplot as plt
import numpy as np
import sys

from mlagents.envs.environment import UnityEnvironment

# Connect to Unity and get environment
env = UnityEnvironment(file_name=None, worker_id=0, seed=1)

# Reset the environment
env_info = env.reset(train_mode=True)

# Set the default brain to work with
default_brain = "Robot"
brain = env_info[default_brain]

# Examine the state space for the default brain
observations = brain.vector_observations[0]
print("Agent state looks like: \n{}".format(observations))

# Assign initial observation values
robot_position = [observations[0], observations[1]]
debris_position = [observations[2], observations[3]]

# Drive the robot until the debris and the robot has the same X value
while debris_position[0] - robot_position[0] > 0:
    action = np.array([1, 0, 0, 0])
    env_info = env.step({default_brain: action}, memory=None, text_action=None)

    # Make observations
    observations = env_info[default_brain].vector_observations[0]
    robot_position = [observations[0], observations[1]]
    debris_position = [observations[2], observations[3]]

# Close simulation
env.close()
