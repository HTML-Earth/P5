import numpy as np
import RobotObservations as observation
from mlagents.envs.environment import UnityEnvironment


class Agent:

    def __init__(self):
        self.env = None
        self.env_info = None
        self.default_brain = None

        # Algorithm
        self.actions = [(i, j, k, 0)
                        for i in range(-1, 2)
                        for j in range(-1, 2)
                        for k in range(-1, 2)]

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
                         self.getting_closer_to_dropzone,
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
        # Observation list class(IntEnum)
        self.obs = observation.RobotObservations

    # Setup connection between Unity and Python
    def setup_connection_with_unity(self, build_scene):
        # Connect to Unity and get environment
        self.env = UnityEnvironment(file_name=build_scene, worker_id=0, seed=3)

        # Reset the environment
        self.env_info = self.env.reset(train_mode=True)

        # Set the default brain to work with
        self.default_brain = "Robot"

    # Update observations variable with information about the environment without dropzone
    def update_observations(self):
        self.observations = self.env_info[self.default_brain].vector_observations[0]

        self.velocity_z = self.get_obs(self.obs.robot_velocity_z)
        self.sensors_front = [self.get_obs(self.obs.sensor_measurement_1), self.get_obs(self.obs.sensor_measurement_2), self.get_obs(self.obs.sensor_measurement_30)]
        self.sensors_behind = [-self.get_obs(self.obs.sensor_measurement_16), -self.get_obs(self.obs.sensor_measurement_15), -self.get_obs(self.obs.sensor_measurement_17)]

    def get_obs(self, index):
        return self.observations[index]

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

        # Throttling into wall
        state.append(1) if self.sensors_front < [self.velocity_z] else state.append(0)

        # Reversing into wall
        state.append(1) if self.sensors_behind > [self.velocity_z] else state.append(0)

        # Robot within dropZone
        state.append(1) if self.get_obs(self.obs.robot_in_dropzone) else state.append(0)

        # Getting closer to debris 1
        state.append(1) if self.get_obs(self.obs.getting_closer_to_debris_1) else state.append(0)

        # Ready to pickup debris
        state.append(1) if self.get_obs(self.obs.arm_position) == 330 and self.get_obs(self.obs.shovel_position) == 360 - 47 else state.append(0)

        # Debris is in shovel
        state.append(1) if self.get_obs(self.obs.debris_in_shovel) else state.append(0)

        # Debris in front of shovel
        state.append(1) if self.get_obs(self.obs.debris_in_front) else state.append(0)

        # Getting closer to dropzone
        state.append(1) if 90 < self.get_obs(self.obs.getting_closer_dropzone) < 270 and self.velocity_z > 0 else state.append(0)

        # Velocity
        state.append(round(self.velocity_z, 1))

        # Rotation
        state.append(int(self.get_obs(self.obs.robot_rotation)))

        # Pointed towards debris
        state.append(1) if self.get_obs(self.obs.robot_direction_debris) else state.append(0)

        # Arm rotation
        state.append(int(self.get_obs(self.obs.arm_position)))

        # Shovel rotation
        state.append(int(self.get_obs(self.obs.shovel_position)))

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
        return 1 if self.get_obs(self.obs.robot_in_dropzone) else 0

    # distance between robot and each debris (a total of 6)
    def distance_to_debris_1(self, state, action):
        return self.distance_to_debris(state, action, self.obs.getting_closer_to_debris_1)

    def distance_to_debris_2(self, state, action):
        return self.distance_to_debris(state, action, self.obs.getting_closer_to_debris_2)

    def distance_to_debris_3(self, state, action):
        return self.distance_to_debris(state, action, self.obs.getting_closer_to_debris_3)

    def distance_to_debris_4(self, state, action):
        return self.distance_to_debris(state, action, self.obs.getting_closer_to_debris_4)

    def distance_to_debris_5(self, state, action):
        return self.distance_to_debris(state, action, self.obs.getting_closer_to_debris_5)

    def distance_to_debris_6(self, state, action):
        return self.distance_to_debris(state, action, self.obs.getting_closer_to_debris_6)

    # function called by previous 6 functions
    def distance_to_debris(self, state, action, observation_index):
        action_list = list(action)
        distance = 0

        # each relevant action is used to predict
        if action_list[0] == 1:
            if action_list[1] == 1:
                if self.observations[observation_index] > distance:
                    distance = self.observations[observation_index]
            elif action_list[1] == 0:
                if self.observations[observation_index] > distance:
                    distance = self.observations[observation_index]
            elif action_list[1] == -1:
                if self.observations[observation_index] > distance:
                    distance = self.observations[observation_index]

        if action_list[0] == -1:
            if action_list[1] == 1:
                if self.observations[observation_index] > distance:
                    distance = self.observations[observation_index]
            elif action_list[1] == 0:
                if self.observations[observation_index] > distance:
                    distance = self.observations[observation_index]
            elif action_list[1] == -1:
                if self.observations[observation_index] > distance:
                    distance = self.observations[observation_index]

        if action_list[0] == 0:
            if action_list[1] == 0:
                if self.velocity_z > 0:
                    if self.observations[observation_index] > distance:
                        distance = self.observations[observation_index]

        return distance

    # Getting closer to debris number 1 (2-6 following)
    def getting_closer_to_debris_1(self, state, action):
        return self.getting_closer_to_debris(state, action, self.obs.angle_robot_debris_1)

    # TODO: add 2-6 to feature list (line 33)
    def getting_closer_to_debris_2(self, state, action):
        return self.getting_closer_to_debris(state, action, self.obs.angle_robot_debris_2)

    def getting_closer_to_debris_3(self, state, action):
        return self.getting_closer_to_debris(state, action, self.obs.angle_robot_debris_3)

    def getting_closer_to_debris_4(self, state, action):
        return self.getting_closer_to_debris(state, action, self.obs.angle_robot_debris_4)

    def getting_closer_to_debris_5(self, state, action):
        return self.getting_closer_to_debris(state, action, self.obs.angle_robot_debris_5)

    def getting_closer_to_debris_6(self, state, action):
        return self.getting_closer_to_debris(state, action, self.obs.angle_robot_debris_6)

    def getting_closer_to_debris(self, state, action, obs_num):
        action_list = list(action)
        # Angle on each side of the robot's forward vector
        angle_range = 45  # TODO: Figure out the exact value
        direction_debris = -angle_range < self.get_obs(obs_num) < angle_range

        getting_closer = 0

        # steer 1 = more negative, steer -1 = more positive
        # Move directly towards debris
        if action_list[0] == 1 and direction_debris:
            getting_closer = 1
        # Turn (left) towards debris
        elif action_list[1] == 1 and self.get_obs(obs_num) < -angle_range:
            getting_closer = 1
        # Turn (Right) towards debris
        elif action_list[1] == -1 and self.get_obs(obs_num) > angle_range:
            getting_closer = 1
        elif self.velocity_z > 0 and action_list[0] == 0 and action_list[1] == 0 and direction_debris:
            getting_closer = 1

        return getting_closer

    def ready_to_pickup_debris(self, state, action):
        action_list = list(action)

        # if arm is not down
        if self.get_obs(self.obs.arm_position) < 360 - self.robot_arm_rotation_constant:
            # if arm is not moving down
            if action_list[2] < 1:
                return 0

        # if shovel is not down
        if self.get_obs(self.obs.shovel_position) < 360 - 47 - self.robot_shovel_rotation_constant:
            # if shovel is not moving down
            if action_list[3] < 1:
                return 0

        # if there is debris in front of the shovel
        return self.debris_in_front_of_shovel(state, action)

    def debris_in_shovel(self, state, action):
        return 1 if self.get_obs(self.obs.debris_in_shovel) else 0

    def debris_in_front_of_shovel(self, state, action):
        action_list = list(action)

        if self.velocity_z > self.throttle_constant or self.velocity_z < -self.reverse_constant:
            if action_list[1] != 0:
                return 0

        return 1 if self.get_obs(self.obs.debris_in_front) else 0

    def getting_closer_to_dropzone(self, state, action):
        action_list = list(action)

        if action_list[0] == 1:
            if action_list[1] == 0:
                if 90 < self.get_obs(self.obs.getting_closer_dropzone) < 270:
                    return 1
                else:
                    return 0

        if action_list[0] == -1:
            if action_list[1] == 0:
                if (90 > self.get_obs(self.obs.getting_closer_dropzone) > 0) or (270 < self.get_obs(self.obs.getting_closer_dropzone) < 360):
                    return 1
                else:
                    return 0

        if action_list[0] == 0:
            if action_list[1] == 0:
                if 90 < self.get_obs(self.obs.getting_closer_dropzone) < 270:
                    if self.velocity_z > 0:
                        return 1
                    else:
                        return 0

        return 0

    def debris_to_dropzone_1(self, state, action):
        return self.debris_to_dropzone(state, action, self.obs.debris_to_dropzone_1)

    def debris_to_dropzone_2(self, state, action):
        return self.debris_to_dropzone(state, action, self.obs.debris_to_dropzone_2)

    def debris_to_dropzone_3(self, state, action):
        return self.debris_to_dropzone(state, action, self.obs.debris_to_dropzone_3)

    def debris_to_dropzone_4(self, state, action):
        return self.debris_to_dropzone(state, action, self.obs.debris_to_dropzone_4)

    def debris_to_dropzone_5(self, state, action):
        return self.debris_to_dropzone(state, action, self.obs.debris_to_dropzone_5)

    def debris_to_dropzone_6(self, state, action):
        return self.debris_to_dropzone(state, action, self.obs.debris_to_dropzone_6)

    def debris_to_dropzone(self, state, action, observation_index):
        action_list = list(action)
        closer_to_dropzone = 0 # the domain is either 1 or 0 (boolean-alike)

        if action_list[0] == 1:
            if action_list[1] == 1:
                if self.observations[observation_index] is 1:
                    closer_to_dropzone = self.observations[observation_index]
            elif action_list[1] == 0:
                if self.observations[observation_index] is 1:
                    closer_to_dropzone = self.observations[observation_index]
            elif action_list[1] == -1:
                if self.observations[observation_index] is 1:
                    closer_to_dropzone = self.observations[observation_index]

        if action_list[0] == 0:
            if action_list[1] == 1:
                if self.observations[observation_index] is 1:
                    closer_to_dropzone = self.observations[observation_index]
            elif action_list[1] == 0:
                if self.observations[observation_index] is 1:
                    closer_to_dropzone = self.observations[observation_index]
            elif action_list[1] == -1:
                if self.observations[observation_index] is 1:
                    closer_to_dropzone = self.observations[observation_index]

        if action_list[0] == -1:
            if action_list[1] == 1:
                if self.observations[observation_index] is 1:
                    closer_to_dropzone = self.observations[observation_index]
            elif action_list[1] == 0:
                if self.observations[observation_index] is 1:
                    closer_to_dropzone = self.observations[observation_index]
            elif action_list[1] == -1:
                if self.observations[observation_index] is 1:
                    closer_to_dropzone = self.observations[observation_index]

        return closer_to_dropzone


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
        rotation = self.get_obs(self.obs.robot_rotation)

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

    # Pointing / Turning towards debris base on angle
    def pointed_towards_debris (self, state, action):
        action_list = list(action)
        pointing = 0

        if not self.get_obs(self.obs.robot_direction_debris):
            if action_list[1] == 1 and self.get_obs(self.obs.angle_robot_debris_1) < 0:
                pointing = 1
            elif action_list[1] == -1 and self.get_obs(self.obs.angle_robot_debris_1) > 0:
                pointing = 1
        elif self.get_obs(self.obs.robot_direction_debris):
            pointing = 1

        return pointing

    # Arm rotation: 0 to 89
    def arm_rotation(self, state, action):
        action_list = list(action)
        arm_rotation = self.get_obs(self.obs.arm_position)

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
        shovel_rotation = self.get_obs(self.obs.shovel_position)

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
