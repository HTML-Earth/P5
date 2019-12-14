import numpy as np
import RobotObservations as observation
from mlagents.envs.environment import UnityEnvironment


class Agent:

    def __init__(self):
        self.env = None
        self.env_info = None
        self.default_brain = None

        # Algorithm
        self.actions = [(i, j)
                        for i in range(-1, 2)
                        for j in range(-1, 2)]

        # Robot constants
        self.robot_length_forward = 2.4
        self.robot_length_backwards = 1.1

        # Action constants
        self.thrust_constant = 0.1 * 5
        self.rotation_constant = 2 * 5

        self.features = [self.feature_1,
                         #self.throttling_into_wall,
                         #self.reversing_into_wall,
                         self.getting_closer_to_debris_1,
                         #self.getting_closer_to_dropzone,
                         #self.rotation,
                         self.pointed_towards_debris,
                         #self.angle_to_debris_1,
                         # self.distance_to_debris_1,
                         # self.debris_to_dropzone_1
                         ]

        # Observations
        self.observations = [0] * 86
        self.sensors_front = []
        self.sensors_behind = []
        # Observation list class(IntEnum)
        self.obs = observation.RobotObservations

    # Setup connection between Unity and Python
    def setup_connection_with_unity(self, build_scene):
        # Connect to Unity and get environment
        self.env = UnityEnvironment(file_name=build_scene, worker_id=0, seed=1)

        # Reset the environment
        self.env_info = self.env.reset(train_mode=True)

        # Set the default brain to work with
        self.default_brain = "Robot"

    # Update observations variable with information about the environment without dropzone
    def update_observations(self):
        self.observations = self.env_info[self.default_brain].vector_observations[0]

        self.sensors_front = [self.get_obs(self.obs.sensor_measurement_1),
                              self.get_obs(self.obs.sensor_measurement_2),
                              self.get_obs(self.obs.sensor_measurement_30)]
        self.sensors_behind = [-self.get_obs(self.obs.sensor_measurement_16),
                               -self.get_obs(self.obs.sensor_measurement_15),
                               -self.get_obs(self.obs.sensor_measurement_17)]

    def get_obs(self, index):
        return self.observations[index]

    # Action functions
    def perform_action(self, thrust, rotate):
        action = np.array([thrust, rotate])
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
        #state.append(1) if self.sensors_front < [self.robot_length_forward] else state.append(0)

        # Reversing into wall
        #state.append(1) if self.sensors_behind > [-self.robot_length_backwards] else state.append(0)

        # Getting closer to debris 1
        getting_closer = 0
        angle_range = 30

        if -angle_range < self.get_obs(self.obs.angle_robot_debris_1) < angle_range and self.get_obs(self.obs.robot_velocity_z) > 0:
            getting_closer = 1
        #elif (self.get_obs(self.obs.getting_closer_to_debris_1) + self.rotation_constant) < -angle_range:
        #    getting_closer = 1
        #elif -angle_range < self.get_obs(self.obs.getting_closer_to_debris_1) < angle_range:
        #    getting_closer = 1
        #print(getting_closer)
        state.append(getting_closer)
        #state.append(1) if self.get_obs(self.obs.getting_closer_to_debris_1) else state.append(0)

        # Getting closer to dropzone
        #state.append(1) if self.get_obs(self.obs.robot_facing_debris) else state.append(0)

        # Rotation
        #state.append(int(self.get_obs(self.obs.robot_rotation)))

        # Pointed towards debris
        state.append(1) if self.get_obs(self.obs.robot_facing_debris) else state.append(0)

        # Angle to debris
        #state.append(int(self.get_obs(self.obs.angle_robot_debris_1)))

        # Distance to debris
        # state.append(int(self.get_obs(self.obs.getting_closer_to_debris_1)))

        # Debris distance to dropzone
        # state.append(int(self.get_obs(self.obs.debris_to_dropzone_1)))

        return state

    # Features
    def feature_1(self, state, action):
        return 1

    def throttling_into_wall(self, state, action):
        # Convert to list because tuples returns an error when indexing
        action_list = list(action)

        constant = 1

        if action_list[0] == 1:
            constant = self.thrust_constant
        elif action_list[0] == -1:
            constant = -self.thrust_constant

        return 1 if self.sensors_front < [self.robot_length_forward + constant] else 0


    def reversing_into_wall(self, state, action):
        action_list = list(action)

        constant = 1

        if action_list[0] == -1:
            constant = -self.thrust_constant
        elif action_list[0] == 1:
            constant = self.thrust_constant

        return 1 if self.sensors_behind > [self.robot_length_backwards + constant] else 0

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

    def distance_to_debris(self, state, action, observation_index):
        action_list = list(action)
        distance = 0

        transform_value = 1 / 20

        return distance * transform_value

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

    # TODO Check if it reverses towards debris
    def getting_closer_to_debris(self, state, action, obs_num):
        action_list = list(action)
        # Angle on each side of the robot's forward vector
        angle_range = 30  # TODO: Figure out the exact value
        angle_to_debris = self.get_obs(obs_num)

        getting_closer = 0

        # steer 1 = more negative, steer -1 = more positive
        # Move directly towards debris
        if action_list[0] == 1:
            if action_list[1] == 1:
                if (angle_to_debris - self.rotation_constant) > angle_range:
                    getting_closer = 1
            elif action_list[1] == -1:
                if (angle_to_debris + self.rotation_constant) < -angle_range:
                    getting_closer = 1
            elif action_list[1] == 0:
                if -angle_range < angle_to_debris < angle_range:
                    getting_closer = 1

        return getting_closer

    # TODO Check if it reverses towards debris
    def getting_closer_to_dropzone(self, state, action):
        action_list = list(action)
        angle_to_dropzone = self.get_obs(self.obs.angle_to_dropzone)
        getting_closer_to_dropzone = 0

        if action_list[0] == 1:
            if action_list[1] == 0:
                if -45 < angle_to_dropzone < 45:
                    getting_closer_to_dropzone = 1
            elif action_list[1] == 1:
                if -45 < angle_to_dropzone + self.rotation_constant < 45:
                    getting_closer_to_dropzone = 1
            elif action_list[1] == -1:
                if -45 < angle_to_dropzone - self.rotation_constant < 45:
                    getting_closer_to_dropzone = 1

        return getting_closer_to_dropzone

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
        closer_to_dropzone = 0  # the domain is either 1 or 0 (boolean-alike)

        if action_list[0] == 1:
            if action_list[1] == 1:
                if self.get_obs(observation_index) is 1:
                    closer_to_dropzone = self.get_obs(observation_index)
            elif action_list[1] == 0:
                if self.get_obs(observation_index) is 1:
                    closer_to_dropzone = self.get_obs(observation_index)
            elif action_list[1] == -1:
                if self.get_obs(observation_index) is 1:
                    closer_to_dropzone = self.get_obs(observation_index)

        if action_list[0] == 0:
            if action_list[1] == 1:
                if self.get_obs(observation_index) is 1:
                    closer_to_dropzone = self.get_obs(observation_index)
            elif action_list[1] == 0:
                if self.get_obs(observation_index) is 1:
                    closer_to_dropzone = self.get_obs(observation_index)
            elif action_list[1] == -1:
                if self.get_obs(observation_index) is 1:
                    closer_to_dropzone = self.get_obs(observation_index)

        if action_list[0] == -1:
            if action_list[1] == 1:
                if self.get_obs(observation_index) is 1:
                    closer_to_dropzone = self.get_obs(observation_index)
            elif action_list[1] == 0:
                if self.get_obs(observation_index) is 1:
                    closer_to_dropzone = self.get_obs(observation_index)
            elif action_list[1] == -1:
                if self.get_obs(observation_index) is 1:
                    closer_to_dropzone = self.get_obs(observation_index)

        return closer_to_dropzone

    def angle_to_debris_1(self, state, action):
        return self.angle_to_debris(state, action, self.obs.angle_to_debris_1)

    def angle_to_debris_2(self, state, action):
        return self.angle_to_debris(state, action, self.obs.angle_to_debris_2)

    def angle_to_debris_3(self, state, action):
        return self.angle_to_debris(state, action, self.obs.angle_to_debris_3)

    def angle_to_debris_4(self, state, action):
        return self.angle_to_debris(state, action, self.obs.angle_to_debris_4)

    def angle_to_debris_5(self, state, action):
        return self.angle_to_debris(state, action, self.obs.angle_to_debris_5)

    def angle_to_debris_6(self, state, action):
        return self.angle_to_debris(state, action, self.obs.angle_to_debris_6)

    def angle_to_debris(self, state, action, observation_index):
        action_list = list(action)

        angle_to_debris = 0
        transform_value = 1 / 360

        if action_list[1] == 1:
            angle_to_debris = self.get_obs(observation_index) + self.rotation_constant
        elif action_list[1] == -1:
            angle_to_debris = self.get_obs(observation_index) - self.rotation_constant
        elif action_list[1] == 0:
            angle_to_debris = self.get_obs(observation_index)

        return (angle_to_debris + 180) * transform_value

    # Rotation: 0 to 360
    def rotation(self, state, action):
        action_list = list(action)
        rotation = self.get_obs(self.obs.robot_rotation)
        new_rotation = 0

        # Transform rotation into a value between 0 and 1
        transform_value = 1 / 360
        
        if action_list[1] == 0:
            return int(rotation) * transform_value
        elif action_list[1] == 1:
            total_rotation = rotation + self.rotation_constant

            # Check if new rotation is above 360 and return value beyond 0 instead
            if total_rotation >= 360:
                new_rotation = int(total_rotation - 360)
            else:
                new_rotation = int(total_rotation)
        elif action_list[1] == -1:
            total_rotation = rotation - self.rotation_constant

            # Check if new rotation is below zero and return value behind 360 instead
            if total_rotation < 0:
                new_rotation = int(360 - abs(total_rotation))
            else:
                new_rotation = int(total_rotation)

        return new_rotation * transform_value

    # Pointing / Turning towards debris base on angle
    def pointed_towards_debris(self, state, action):
        action_list = list(action)
        pointing = 0

        if action_list[1] == 1:
            if self.get_obs(self.obs.angle_robot_debris_1) + self.rotation_constant < 0:
                pointing = 1
        elif action_list[1] == -1:
            if self.get_obs(self.obs.angle_robot_debris_1) - self.rotation_constant > 0:
                pointing = 1
        elif action_list[1] == 0:
            if self.get_obs(self.obs.robot_facing_debris):
                pointing = 1

        return pointing
