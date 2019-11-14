import matplotlib.pyplot as plt
import numpy as np
import sys

from mlagents.envs.environment import UnityEnvironment

# Connect to Unity and get environment
env = UnityEnvironment(file_name=None, worker_id=0, seed=1)

# Reset the environment
env_info = env.reset(train_mode=True)

# Set the default brain to work with
default_brain = "3DBall"
brain = env_info[default_brain]

# Examine the state space for the default brain
print("Agent state looks like: \n{}".format(brain.vector_observations[0]))

# Examine the observation space for the default brain
for observation in brain.visual_observations:
    print("Agent observations look like:")
    if observation.shape[3] == 3:
        plt.imshow(observation[0,:,:,:])
    else:
        plt.imshow(observation[0,:,:,0])

#action = 1
#env.step(action, memory=None, text_action=None)

#env.reset(train_mode=True, config=None)

env.close()