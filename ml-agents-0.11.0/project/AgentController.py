import numpy as np
from mlagents.envs.environment import UnityEnvironment


class Agent:
    # Environment variables
    env = None
    env_info = None
    default_brain = None

    # Observation of environment variables
    observations = None

    robot_position = None
    arm_position = None
    shovel_position = None

    dropzone_position = None
    dropzone_radius = None

    debris_visibility = None
    debris_position = None

    timeElapsed = None

    def __init__(self):
        self.setup_connection_with_unity()
        self.initial_observations()

    # Setup connection between Unity and Python
    def setup_connection_with_unity(self):
        # Connect to Unity and get environment
        self.env = UnityEnvironment(file_name=None, worker_id=0, seed=3)

        # Reset the environment
        self.env_info = self.env.reset(train_mode=True)

        # Set the default brain to work with
        self.default_brain = "Robot"

    # Initial observations about the environment
    def initial_observations(self):
        self.observations = self.env_info[self.default_brain].vector_observations[0]

        self.robot_position = [self.observations[0], self.observations[1]]
        self.arm_position = self.observations[2]
        self.shovel_position = self.observations[3]
        self.dropzone_position = [self.observations[4], self.observations[5]]
        self.dropzone_radius = self.observations[6]

        # distance sensors (7-36)

        # debris positions (37-55)
        self.debris_position = [self.observations[38], self.observations[40]]

    # Update observations variable with information about the environment without dropzone
    def update_observations(self):
        self.observations = self.env_info[self.default_brain].vector_observations[0]

        self.robot_position = [self.observations[0], self.observations[1]]
        self.debris_position = [self.observations[38], self.observations[40]]
        self.timeElapsed = self.observations[55]

    # Action functions
    def perform_action(self, throttle, angle, arm_rotation, shovel_rotation):
        action = np.array([throttle, angle, arm_rotation, shovel_rotation])
        return self.env.step({self.default_brain: action})

    def get_reward(self):
        return self.env_info[self.default_brain].rewards

    def get_brain_info(self):
        return self.env_info[self.default_brain]

    # Closes simulation
    def close_simulation(self):
        self.env.close()

    def reset_simulation(self):
        self.env.reset()
