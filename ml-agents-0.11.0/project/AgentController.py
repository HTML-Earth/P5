import numpy as np
from mlagents.envs.environment import UnityEnvironment


class Agent:

    def __init__(self):
        self.env = None
        self.env_info = None
        self.default_brain = None

        # Setup connection
        self.setup_connection_with_unity()

        # Algorithm
        self.actions = [(i, j, k, l)
                        for i in range(-1, 2)
                        for j in range(-1, 2)
                        for k in range(-1, 2)
                        for l in range(-1, 2)]

        self.throttle_constant = 1.1
        self.reverse_constant = 0.9

        self.robot_length_forward = 2.4
        self.robot_length_backwards = 1.1

        self.rotation_constant = 0.83 * 5

        self.features = [self.feature_1,
                         self.throttling_into_wall,
                         self.reversing_into_wall,
                         self.robot_within_dropzone,
                         self.getting_closer_to_debris_1,
                         self.ready_to_pickup_debris,
                         self.debris_in_shovel,
                         self.debris_in_front_of_shovel,
                         self.velocity,
                         self.rotation]

        # Observations
        self.observations = None
        self.velocity_z = None
        self.sensors_front = None
        self.sensors_behind = None

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

        self.velocity_z = self.observations[5]
        self.sensors_front = [self.observations[11], self.observations[12], self.observations[40]]
        self.sensors_behind = [-self.observations[26], -self.observations[25], -self.observations[27]]

    # Action functions
    def perform_action(self, throttle, angle, arm_rotation, shovel_rotation):
        action = np.array([throttle, angle, arm_rotation, shovel_rotation])
        self.env_info = self.env.step({self.default_brain: action})

    def get_reward(self):
        return self.env_info[self.default_brain].rewards[0]

    def is_done(self):
        return self.env_info[self.default_brain].local_done[0]

    def reset_simulation(self):
        self.env.reset()

    # States
    def get_state(self):
        state = [1]

        self.update_observations()

        # Check if robot is throttling into wall
        state.append(1) if self.sensors_front < [self.velocity_z] else state.append(0)

        # Check if robot is reversing into wall
        state.append(1) if self.sensors_behind > [self.velocity_z] else state.append(0)

        # Check if robot is within the dropZone
        state.append(1) if self.observations[60] else state.append(0)

        # Check if robot is getting closer to debris 1
        state.append(1) if self.observations[61] else state.append(0)

        # Check if ready to pickup debris
        state.append(1) if self.observations[6] == 330 and self.observations[7] == 360 - 47 else state.append(0)

        # Check if debris is in shovel
        state.append(1) if self.observations[67] else state.append(0)

        # Check if debris in front of shovel
        state.append(1) if self.observations[68] else state.append(0)

        # Robot velocity
        state.append(round(self.velocity_z, 1))

        # Robot rotation
        state.append(int(self.observations[2]))

        return state

    # Features
    def feature_1(self, state, action):
        return 1

    def throttling_into_wall(self, state, action):
        # Convert to list because tuples returns an error when indexing
        action_list = list(action)

        constant = 1

        if action_list[0] == 1:
            constant = self.throttle_constant
        elif action_list[0] == -1:
            constant = self.reverse_constant

        return 1 if self.sensors_front < [(self.velocity_z + self.robot_length_forward) * constant] else 0

    def reversing_into_wall(self, state, action):
        action_list = list(action)

        constant = 1

        if action_list[0] == -1:
            constant = self.reverse_constant
        elif action_list[0] == 1:
            constant = self.throttle_constant

        return 1 if self.sensors_behind > [(self.velocity_z + self.robot_length_backwards) * constant] else 0

    def robot_within_dropzone(self, state, action):
        return 1 if self.observations[60] else 0

    def getting_closer_to_debris_1(self, state, action):
        return 1 if self.observations[61] else 0

    def ready_to_pickup_debris(self, state, action):
        return 1 if self.observations[6] == 330 and self.observations[7] == 360 - 47 else 0

    def debris_in_shovel(self, state, action):
        return 1 if self.observations[67] else 0

    def debris_in_front_of_shovel(self, state, action):
        return 1 if self.observations[68] else 0

    # TODO - Check of this makes sense
    # TODO - Consider transforming velocity into a number between 0 and 1
    def velocity(self, state, action):
        action_list = list(action)

        if action_list[0] == 1:
            return round(self.velocity_z * self.throttle_constant, 1)
        elif action_list[0] == -1:
            return round(self.velocity_z * self.reverse_constant, 1)
        elif action_list[0] == 0:
            return round(self.velocity_z, 1)

    def rotation(self, state, action):
        action_list = list(action)
        rotation = self.observations[2]

        # Transform rotation into a value between 0 and 1
        transform_value = 1 / 360
        
        if self.velocity_z == 0 or action_list[1] == 0:
            return int(rotation) * transform_value
        elif action_list[1] == 1:
            total_rotation = rotation + self.rotation_constant

            if total_rotation >= 360:
                return int(total_rotation - 360) * transform_value
            else:
                return int(total_rotation) * transform_value
        elif action_list[1] == -1:
            total_rotation = rotation - self.rotation_constant

            if total_rotation < 0:
                return int(360 - abs(total_rotation)) * transform_value
            else:
                return int(total_rotation) * transform_value
