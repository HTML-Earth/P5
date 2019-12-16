import math
import numpy as np
import RobotObservations as observation
from mlagents.envs.environment import UnityEnvironment


class Agent:

    def __init__(self):
        # Unity variables
        self.env = None
        self.env_info = None
        self.default_brain = None

        # Actions (Nothing, forward, turn left, reverse, turn right)
        self.actions = [0, 1, 2, 3, 4]

        # Robot constants
        self.robot_length_forward = 2.4
        self.robot_length_backwards = 1.1

        # Action constants
        self.thrust_constant = 0.1 * 5
        self.rotation_constant = 2 * 5

        self.features = [self.feature_1,
                         self.throttling_into_wall,
                         self.reversing_into_wall,
                         self.getting_closer_to_debris_1,
                         self.getting_closer_to_dropzone,
                         self.rotation,
                         self.pointed_towards_debris,
                         self.angle_to_debris_1,
                         self.distance_to_debris_1,
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
        self.velocity_z = self.get_obs(self.obs.robot_velocity_z)
        #self.sensors_front = [self.get_obs(self.obs.sensor_measurement_1), self.get_obs(self.obs.sensor_measurement_2), self.get_obs(self.obs.sensor_measurement_30)]
        #self.sensors_behind = [-self.get_obs(self.obs.sensor_measurement_16), -self.get_obs(self.obs.sensor_measurement_15), -self.get_obs(self.obs.sensor_measurement_17)]

        self.sensors_front = [self.get_obs(self.obs.sensor_measurement_1),
                              self.get_obs(self.obs.sensor_measurement_2),
                              self.get_obs(self.obs.sensor_measurement_30)]
        self.sensors_behind = [-self.get_obs(self.obs.sensor_measurement_16),
                               -self.get_obs(self.obs.sensor_measurement_15),
                               -self.get_obs(self.obs.sensor_measurement_17)]

    def get_obs(self, index):
        return self.observations[index]

    def perform_action(self, action):
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
        state.append(1) if self.sensors_front < [self.robot_length_forward] else state.append(0)

        # Reversing into wall
        state.append(1) if self.sensors_behind > [-self.robot_length_backwards] else state.append(0)

        # Robot within dropZone
        state.append(1) if self.get_obs(self.obs.robot_in_dropzone) else state.append(0)

        # Getting closer to debris 1
        #state.append(1) if self.get_obs(self.obs.getting_closer_to_debris_1) else state.append(0)

        # Ready to pickup debris
        state.append(1) if self.get_obs(self.obs.shovel_position) == 330 else state.append(0)

        # Debris is in shovel
        state.append(1) if self.get_obs(self.obs.debris_in_shovel) else state.append(0)

        # Debris in front of shovel
        state.append(1) if self.get_obs(self.obs.debris_in_front) else state.append(0)

        # Getting closer to dropzone
        state.append(1) if self.get_obs(self.obs.robot_facing_debris) else state.append(0)

        # Rotation
        #state.append(int(self.get_obs(self.obs.robot_rotation)))

        # Pointed towards debris
        state.append(1) if self.get_obs(self.obs.robot_facing_debris) else state.append(0)

        # Angle to debris
        state.append(int(self.get_obs(self.obs.angle_robot_debris_1)))

        # Distance to debris
        state.append(int(self.get_obs(self.obs.getting_closer_to_debris_1)))

        # Debris distance to dropzone
        # state.append(int(self.get_obs(self.obs.debris_to_dropzone_1)))

        return state

    # Features
    def feature_1(self, state, action):
        return 1

    def throttling_into_wall(self, state, action):
        constant = 1

        if action == 1:
            constant = self.thrust_constant
        elif action == -1:
            constant = -self.thrust_constant

        return 1 if self.sensors_front < [self.robot_length_forward + constant] else 0

    def reversing_into_wall(self, state, action):
        constant = 1

        if action == -1:
            constant = -self.thrust_constant
        elif action == 1:
            constant = self.thrust_constant

        return 1 if self.sensors_behind > [self.robot_length_backwards + constant] else 0

    def distance_to_debris_1(self, state, action):
        debrisPosition_1 = [self.obs.debris_1_position_x, self.obs.debris_1_position_z]
        return self.distance_to_debris(state, action, debrisPosition_1)

    def distance_to_debris_2(self, state, action):
        debrisPosition_2 = [self.obs.debris_2_position_x, self.obs.debris_2_position_y]
        return self.distance_to_debris(state, action, debrisPosition_2)

    def distance_to_debris_3(self, state, action):
        debrisPosition_3 = [self.obs.debris_3_position_x, self.obs.debris_3_position_z]
        return self.distance_to_debris(state, action, debrisPosition_3)

    def distance_to_debris_4(self, state, action):
        debrisPosition_4 = [self.obs.debris_4_position_x, self.obs.debris_4_position_z]
        return self.distance_to_debris(state, action, debrisPosition_4)

    def distance_to_debris_5(self, state, action):
        debrisPosition_5 = [self.obs.debris_5_position_x, self.obs.debris_5_position_z]
        return self.distance_to_debris(state, action, debrisPosition_5)

    def distance_to_debris_6(self, state, action):
        debrisPosition_6 = [self.obs.debris_6_position_x, self.obs.debris_6_position_z]
        return self.distance_to_debris(state, action, debrisPosition_6)

    def distance_to_debris(self, state, action, debrisNumber):
        robotPosition = [self.obs.robot_position_x, self.obs.robot_position_z]
        distance = 0

        if action == 1:
            if action == 1:  # forward + right
                Angle = -self.rotation_constant
                distance = self.distance_solver(robotPosition, debrisNumber, 1, Angle)

            elif action == 0:  # forward + straight
                Angle = 0
                distance = self.distance_solver(robotPosition, debrisNumber, 1, Angle)

            elif action == -1:  # forward + left
                Angle = self.rotation_constant
                distance = self.distance_solver(robotPosition, debrisNumber, 1, Angle)

        if action == -1:
            if action == 1:  # backward + right
                Angle = self.rotation_constant
                distance = self.distance_solver(robotPosition, debrisNumber, -1, Angle)

            elif action == 0:  # backward + straight
                Angle = 0
                distance = self.distance_solver(robotPosition, debrisNumber, -1, Angle)

            elif action == -1:  # backward + left
                Angle = -self.rotation_constant
                distance = self.distance_solver(robotPosition, debrisNumber, -1, Angle)

        if action == 0:
            if action == 0:
                Angle = 0
                distance = self.distance_solver(robotPosition, debrisNumber, 0, Angle)

        transform_value = 1 / 20

        return distance * transform_value


    def distance_solver(self, robotPosition, debrisNumber, throttle, angle):
        robotPosition_new = []
        # find robot's position based on current rotation
        for i in range(len(robotPosition)):
            if throttle == 1:  # if moving forwards, add constant
                robotPosition[i] += self.thrust_constant
                robotPosition_new.append(robotPosition[i])
            elif throttle == -1:  # if moving backwards, subtract constant
                robotPosition[i] -= self.thrust_constant
                robotPosition_new.append(robotPosition[i])
            elif throttle == 0:  # if not moving save robot's current position as new position
                robotPosition_new.append(robotPosition[i])

        # find robot's new position based on its' previous position
        if angle != 0:
            robotPosition_new = self.robotPostion_new(robotPosition_new, angle)

        distance_vector = []
        # find vector from robot to debris
        for i in range(len(robotPosition_new)):
            result = debrisNumber[i] - robotPosition_new[i]
            distance_vector.append(result)

        result = 0
        # find distance between robot and debris
        for i in distance_vector:
            result += i * i
        distance = math.sqrt(result)

        return distance

    def robotPostion_new(self, robotPosition, angle):
        robotPosition_new = []

        # as robotPosition[x , z] we use specific index to determine the x and z-coordinate
        rbPos_X = robotPosition[0] * math.cos(math.radians(angle)) - robotPosition[1] * math.sin(math.radians(angle))
        rbPos_Z = robotPosition[0] * math.sin(math.radians(angle)) + robotPosition[1] * math.cos(math.radians(angle))

        robotPosition_new.append(rbPos_X)
        robotPosition_new.append(rbPos_Z)

        return robotPosition_new

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
        action_list = action
        # Angle on each side of the robot's forward vector
        angle_range = 45  # TODO: Figure out the exact value
        angle_to_debris = self.get_obs(obs_num)

        getting_closer = 0

        # steer 1 = more negative, steer -1 = more positive
        # Move directly towards debris
        if action == 1:
            if action == 1:
                if -angle_range < (angle_to_debris + self.rotation_constant) < angle_range:
                    getting_closer = 1
            elif action == -1:
                if -angle_range < (angle_to_debris - self.rotation_constant) < angle_range:
                    getting_closer = 1
            elif action == 0:
                if -angle_range < angle_to_debris < angle_range:
                    getting_closer = 1

        print(getting_closer)
        return getting_closer

    # TODO Check if it reverses towards debris
    def getting_closer_to_dropzone(self, state, action):
        action_list = action
        angle_to_dropzone = self.get_obs(self.obs.angle_to_dropzone)
        getting_closer_to_dropzone = 0

        if action == 1:
            if action == 0:
                if -45 < angle_to_dropzone < 45:
                    getting_closer_to_dropzone = 1
            elif action == 1:
                if -45 < angle_to_dropzone + self.rotation_constant < 45:
                    getting_closer_to_dropzone = 1
            elif action == -1:
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
        action_list = action
        closer_to_dropzone = 0  # the domain is either 1 or 0 (boolean-alike)

        if action == 1:
            if action == 1:
                if self.get_obs(observation_index) is 1:
                    closer_to_dropzone = self.get_obs(observation_index)
            elif action == 0:
                if self.get_obs(observation_index) is 1:
                    closer_to_dropzone = self.get_obs(observation_index)
            elif action == -1:
                if self.get_obs(observation_index) is 1:
                    closer_to_dropzone = self.get_obs(observation_index)

        if action == 0:
            if action == 1:
                if self.get_obs(observation_index) is 1:
                    closer_to_dropzone = self.get_obs(observation_index)
            elif action == 0:
                if self.get_obs(observation_index) is 1:
                    closer_to_dropzone = self.get_obs(observation_index)
            elif action == -1:
                if self.get_obs(observation_index) is 1:
                    closer_to_dropzone = self.get_obs(observation_index)

        if action == -1:
            if action == 1:
                if self.get_obs(observation_index) is 1:
                    closer_to_dropzone = self.get_obs(observation_index)
            elif action == 0:
                if self.get_obs(observation_index) is 1:
                    closer_to_dropzone = self.get_obs(observation_index)
            elif action == -1:
                if self.get_obs(observation_index) is 1:
                    closer_to_dropzone = self.get_obs(observation_index)

        return closer_to_dropzone

    def angle_to_debris_1(self, state, action):
        debris_1 = [self.obs.debris_1_position_x, self.obs.debris_1_position_z]
        return self.angle_to_debris(state, action, debris_1)

    def angle_to_debris_2(self, state, action):
        debris_2 = [self.obs.debris_2_position_x, self.obs.debris_2_position_z]
        return self.angle_to_debris(state, action, debris_2)

    def angle_to_debris_3(self, state, action):
        debris_3 = [self.obs.debris_3_position_x, self.obs.debris_3_position_z]
        return self.angle_to_debris(state, action, debris_3)

    def angle_to_debris_4(self, state, action):
        debris_4 = [self.obs.debris_4_position_x, self.obs.debris_4_position_z]
        return self.angle_to_debris(state, action, debris_4)

    def angle_to_debris_5(self, state, action):
        debris_5 = [self.obs.debris_5_position_x, self.obs.debris_5_position_z]
        return self.angle_to_debris(state, action, debris_5)

    def angle_to_debris_6(self, state, action):
        debris_6 = [self.obs.debris_6_position_x, self.obs.debris_6_position_z]
        return self.angle_to_debris(state, action, debris_6)

    def angle_to_debris(self, state, action, debrisNumber):
        transform_value = 1 / 360
        robotPosition = [self.obs.robot_position_x, self.obs.robot_position_z]
        angle_to_debris = 0

        if action == 1:
            if action == 1:  # forward + right
                Angle = -self.rotation_constant
                angle_to_debris = self.robotAngle_New(robotPosition, debrisNumber, 1, Angle)

            if action == 0:  # forward + straight
                Angle = 0
                angle_to_debris = self.robotAngle_New(robotPosition, debrisNumber, 1, Angle)

            if action == -1:  # forward + left
                Angle = self.rotation_constant
                angle_to_debris = self.robotAngle_New(robotPosition, debrisNumber, 1, Angle)

        if action == -1:
            if action == 1:  # backward + right
                Angle = self.rotation_constant
                angle_to_debris = self.robotAngle_New(robotPosition, debrisNumber, -1, Angle)

            if action == 0:  # backward + straight
                Angle = 0
                angle_to_debris = self.robotAngle_New(robotPosition, debrisNumber, -1, Angle)

            if action == -1:  # backward + left
                Angle = -self.rotation_constant
                angle_to_debris = self.robotAngle_New(robotPosition, debrisNumber, -1, Angle)

        if action == 0:
            if action == 1:  # right
                Angle = -self.rotation_constant
                angle_to_debris = self.robotAngle_New(robotPosition, debrisNumber, 0, Angle)

            if action == 0:  # not moving
                Angle = 0
                angle_to_debris = self.robotAngle_New(robotPosition, debrisNumber, 0, Angle)

            if action == -1:  # left
                Angle = self.rotation_constant
                angle_to_debris = self.robotAngle_New(robotPosition, debrisNumber, 0, Angle)

        return angle_to_debris * transform_value


    def robotAngle_New(self, robotPosition, debrisNumber, throttle, angle):
        robotPosition_new = []

        # find the vector straight from the robot based on thrust
        for i in range(len(robotPosition)):
            if throttle == 1:
                robotPosition[i] += self.thrust_constant
                robotPosition_new.append(robotPosition[i])
            elif throttle == -1:
                robotPosition[i] -= self.thrust_constant
                robotPosition_new.append(robotPosition[i])
            elif throttle == 0:
                robotPosition_new.append(robotPosition[i])

        # find robot's new position by predicting
        robotPosition_new = self.robotPostion_new(robotPosition_new, angle)

        # find vector from robot's predicted position to a certain debris
        debrisVector = []
        for i in range(len(robotPosition_new)):
            result = debrisNumber[i] - robotPosition_new[i]
            debrisVector.append(result)

        # find the vector pointing forward from the robot's new position
        robotVector = []
        for i in range(len(robotPosition_new)):
            if throttle == 1:
                result = robotPosition_new[i] + self.thrust_constant
                robotVector.append(result)
            elif throttle == -1:
                result = robotPosition_new[i] + self.thrust_constant
                robotVector.append(result)
            elif throttle == 0:
                result = robotPosition_new[i] + self.thrust_constant
                robotVector.append(result)
                continue

        for i in range(len(robotVector)):
            robotVector[i] = robotVector[i] - robotPosition_new[i]

        # find angle from robot pointing forward to vector from robot to debris
        distanceVecotr = []
        for i in range(len(robotPosition_new)):
            result = debrisNumber[i] - robotPosition_new[i]
            distanceVecotr.append(result)

        # find angle between 'robotVector' and 'distanceVector'
        robotRotation_new = self.angle(robotVector, distanceVecotr)

        return robotRotation_new

    def dotproduct(self, v1, v2):  # find dot product of two vectors
        return sum((a * b) for a, b in zip(v1, v2))

    def length(self, v):  # find the length of a vector
        return math.sqrt(self.dotproduct(v, v))

    def angle(self, v1, v2):  # find the angle between two vectors
        return math.degrees(math.acos(self.dotproduct(v1, v2) / (self.length(v1) * self.length(v2))))

    # Rotation: 0 to 360
    def rotation(self, state, action):
        rotation = self.get_obs(self.obs.robot_rotation)
        new_rotation = 0

        # Transform rotation into a value between 0 and 1
        transform_value = 1 / 360
        
        if action == 0:
            return int(rotation) * transform_value
        elif action == 1:
            total_rotation = rotation + self.rotation_constant

            # Check if new rotation is above 360 and return value beyond 0 instead
            if total_rotation >= 360:
                new_rotation = int(total_rotation - 360)
            else:
                new_rotation = int(total_rotation)
        elif action == -1:
            total_rotation = rotation - self.rotation_constant

            # Check if new rotation is below zero and return value behind 360 instead
            if total_rotation < 0:
                new_rotation = int(360 - abs(total_rotation))
            else:
                new_rotation = int(total_rotation)

        return new_rotation * transform_value

    # Pointing / Turning towards debris base on angle
    def pointed_towards_debris(self, state, action):
        pointing = 0

        if action == 1:
            if self.get_obs(self.obs.angle_robot_debris_1) + self.rotation_constant < 0:
                pointing = 1
        elif action == -1:
            if self.get_obs(self.obs.angle_robot_debris_1) - self.rotation_constant > 0:
                pointing = 1
        elif action == 0:
            if self.get_obs(self.obs.robot_facing_debris):
                pointing = 1

        return pointing
