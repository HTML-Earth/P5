import numpy as np
from mlagents.envs.environment import UnityEnvironment


class Agent:

    def __init__(self):
        self.env = None
        self.env_info = None
        self.default_brain = None

        # Setup connection
        self.setup_connection_with_unity()

        self.observations = None

        self.actions = [(i, j, k, l)
                        for i in range(-1, 2)
                        for j in range(-1, 2)
                        for k in range(-1, 2)
                        for l in range(-1, 2)]

        self.feature_values = [0] * 12

    # Setup connection between Unity and Python
    def setup_connection_with_unity(self):
        # Connect to Unity and get environment
        self.env = UnityEnvironment(file_name=None, worker_id=0, seed=3)

        # Reset the environment
        self.env_info = self.env.reset(train_mode=True)

        # Set the default brain to work with
        self.default_brain = "Robot"

    # Update observations variable with information about the environment without dropzone
    def update_observations(self):
        self.observations = self.env_info[self.default_brain].vector_observations[0]

    # Action functions
    def perform_action(self, throttle, angle, arm_rotation, shovel_rotation):
        action = np.array([throttle, angle, arm_rotation, shovel_rotation])
        self.env_info = self.env.step({self.default_brain: action})
        self.update_observations()

    # Update Feature values array
    def update_feature_values(self):
        self.update_observations()

        # z means the velocity when moving forward and backward.
        velocity_z = self.observations[5]
        sensors_front = [self.observations[11], self.observations[12], self.observations[40]]
        sensors_behind = [-self.observations[26], -self.observations[25], -self.observations[27]]

        self.feature_values[0] = 1

        # Check if robot is throttling into wall
        if sensors_front < [velocity_z]:
            self.feature_values[1] = 1
        else:
            self.feature_values[1] = 0

        # Check if robot is reversing into wall
        if sensors_behind > [velocity_z]:
            self.feature_values[2] = 1
        else:
            self.feature_values[2] = 0

        # Check if robot is within the dropZone
        if self.observations[60]:
            self.feature_values[3] = 1
        else:
            self.feature_values[3] = 0

        # Check if robot is getting closer to debris
        index = 4
        for i in range(61, 67):
            if self.observations[i]:
                self.feature_values[index] = 1
            else:
                self.feature_values[index] = 0
            index += 1

        # Check if ready to pickup debris
        if self.observations[6] == 330 and self.observations[7] == 360 - 47:
            self.feature_values[index] = 1
        else:
            self.feature_values[index] = 0

        # Check if debris is in shovel
        if self.observations[67]:
            self.feature_values[11] = 1
        else:
            self.feature_values[11] = 0

    def get_state(self):
        self.update_observations()
        self.update_feature_values()

        return self.feature_values

    def get_feature_values(self, state, action):
        return self.feature_values

    def get_features(self):
        return self.feature_values

    def get_reward(self):
        return self.env_info[self.default_brain].rewards[0]

    def is_done(self):
        return self.env_info[self.default_brain].local_done[0]

    # Closes simulation
    def close_simulation(self):
        self.env.close()

    def reset_simulation(self):
        self.env.reset()
