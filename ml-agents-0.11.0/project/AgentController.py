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

        self.robot_arm_rotation_constant = 10
        self.robot_shovel_rotation_constant = 10

        self.features = [self.feature_1,
                         self.throttling_into_wall,
                         self.reversing_into_wall,
                         self.robot_within_dropzone,
                         self.getting_closer_to_debris_1,
                         self.ready_to_pickup_debris,
                         self.debris_in_shovel,
                         self.debris_in_front_of_shovel,
                         self.velocity,
                         self.rotation,
                         self.pointed_towards_debris,
                         self.arm_rotation,
                         self.shovel_rotation]

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

        # Robot rotated towards debris
        state.append(1) if self.observations[75] else state.append(0)

        # Robot arm rotation
        state.append(int(self.observations[6]))

        # Robot shovel rotation
        state.append(int(self.observations[7]))

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

    # Getting closer to debris number 1 (2-6 following)
    def getting_closer_to_debris_1(self, state, action):
        return self.getting_closer_to_debris(state, action, 68)

    # TODO: add 2-6 to feature list (line 33)
    def getting_closer_to_debris_2(self, state, action):
        return self.getting_closer_to_debris(state, action, 69)

    def getting_closer_to_debris_3(self, state, action):
        return self.getting_closer_to_debris(state, action, 70)

    def getting_closer_to_debris_4(self, state, action):
        return self.getting_closer_to_debris(state, action, 71)

    def getting_closer_to_debris_5(self, state, action):
        return self.getting_closer_to_debris(state, action, 72)

    def getting_closer_to_debris_6(self, state, action):
        return self.getting_closer_to_debris(state, action, 73)

    def getting_closer_to_debris(self, state, action, obs_num):
        action_list = list(action)
        # Angle on each side of the robot's forward vector
        angle_range = 45  # TODO: Figure out the exact value
        direction_debris = -angle_range < self.observations[obs_num] < angle_range

        getting_closer = 0

        # steer 1 = more negative, steer -1 = more positive
        # Move directly towards debris
        if action_list[0] == 1 and direction_debris:
            getting_closer = 1
        # Turn (left) towards debris
        elif action_list[1] == 1 and self.observations[obs_num] < -angle_range:
            getting_closer = 1
        # Turn (Right) towards debris
        elif action_list[1] == -1 and self.observations[obs_num] > angle_range:
            getting_closer = 1
        elif self.velocity_z > 0 and action_list[0] == 0 and action_list[1] == 0 and direction_debris:
            getting_closer = 1

        return getting_closer

    def ready_to_pickup_debris(self, state, action):
        action_list = list(action)

        # if arm is not down
        if self.observations[6] < 360 - self.robot_arm_rotation_constant:
            # if arm is not moving down
            if action_list[2] < 1:
                return 0

        # if shovel is not down
        if self.observations[7] < 360 - 47 - self.robot_shovel_rotation_constant:
            # if shovel is not moving down
            if action_list[3] < 1:
                return 0

        # if there is debris in front of the shovel
        return self.debris_in_front_of_shovel(state, action)

    def debris_in_shovel(self, state, action):
        return 1 if self.observations[67] else 0

    def debris_in_front_of_shovel(self, state, action):
        action_list = list(action)

        if self.velocity_z > self.throttle_constant or self.velocity_z < -self.reverse_constant:
            if action_list[1] != 0:
                return 0

        return 1 if self.observations[74] else 0

    def getting_closer_to_dropzone(self, state, action):
        action_list = list(action)

        if action_list[0] == 1:
            if action_list[1] == 0:
                if 90 < self.observations[76] < 270:
                    return 1
                else:
                    return 0

        if action_list[0] == -1:
            if action_list[1] == 0:
                if (90 > self.observations[76] > 0) or (270 < self.observations[76] < 360):
                    return 1
                else:
                    return 0

        if action_list[0] == 0:
            if action_list[1] == 0:
                if 90 < self.observations[76] < 270:
                    if self.velocity_z > 0:
                        return 1
                    else:
                        return 0

    # If action is (0, 0, 0, 0), the velocity should be considered.

    # TODO - Check of (if?) this makes sense
    # TODO - Consider transforming velocity into a number between 0 and 1
    def velocity(self, state, action):
        action_list = list(action)
        # Transform velocity into a value between 0 and 1
        transform_value = 1 / 10

        if action_list[0] == 1:
            return round(self.velocity_z * self.throttle_constant, 1) * transform_value
        elif action_list[0] == -1:
            return round(self.velocity_z * self.reverse_constant, 1) * transform_value
        elif action_list[0] == 0:
            return round(self.velocity_z, 1) * transform_value

    # Rotation: 0 to 360
    def rotation(self, state, action):
        action_list = list(action)
        rotation = self.observations[2]

        # Transform rotation into a value between 0 and 1
        transform_value = 1 / 360
        
        if self.velocity_z == 0 or action_list[1] == 0:
            return int(rotation) * transform_value
        elif action_list[1] == 1:
            total_rotation = rotation + self.rotation_constant

            # Check if new rotation is above 360 and return value beyond 0 instead
            if total_rotation >= 360:
                return int(total_rotation - 360) * transform_value
            else:
                return int(total_rotation) * transform_value
        elif action_list[1] == -1:
            total_rotation = rotation - self.rotation_constant

            # Check if new rotation is below zero and return value behind 360 instead
            if total_rotation < 0:
                return int(360 - abs(total_rotation)) * transform_value
            else:
                return int(total_rotation) * transform_value

    def pointed_towards_debris (self, state, action): # TODO kig på 68
        action_list = list(action)
        pointing = 0

        if not self.observations[75]:
            if action_list[1] == 1 and self.observations[68] < 0:
                pointing = 1
            elif action_list[1] == -1 and self.observations[68] > 0:
                pointing = 1
        elif self.observations[75]:
            pointing = 1

        return pointing

    # Arm rotation: 0 to 89
    def arm_rotation(self, state, action):
        action_list = list(action)
        arm_rotation = self.observations[6]

        # TODO Should the transform value be 1/360 instead?
        # Transform rotation into a value between 0 and 1
        transform_value = 1 / 360

        if action_list[2] == 1:
            # Add rotation constant to rotation to check if rotation is beyond maximum
            total_rotation = arm_rotation + self.robot_arm_rotation_constant

            return 89 * transform_value if total_rotation > 89 else int(total_rotation) * transform_value
        elif action_list[2] == -1:
            # Subtract rotation constant from rotation to check if rotation is beyond minimum
            total_rotation = arm_rotation - self.robot_arm_rotation_constant

            return 0 * transform_value if total_rotation < 0 else int(total_rotation) * transform_value
        elif action_list[2] == 0:
            return int(arm_rotation) * transform_value

    # Shovel rotation: 0 to 70
    def shovel_rotation(self, state, action):
        action_list = list(action)
        shovel_rotation = self.observations[7]

        # TODO Should the transform value be 1/360 instead?
        # Transform rotation into a value between 0 and 1
        transform_value = 1 / 360

        if action_list[3] == 1:
            # Add rotation constant to rotation to check if rotation is beyond maximum
            total_rotation = shovel_rotation + self.robot_shovel_rotation_constant

            return 70 * transform_value if total_rotation > 70 else int(total_rotation) * transform_value
        elif action_list[3] == -1:
            # Subtract rotation constant from rotation to check if rotation is beyond minimum
            total_rotation = shovel_rotation - self.robot_shovel_rotation_constant

            return 0 * transform_value if total_rotation < 0 else int(total_rotation) * transform_value
        elif action_list[3] == 0:
            return int(shovel_rotation) * transform_value
