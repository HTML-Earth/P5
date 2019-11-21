# import matplotlib.pyplot as plt
import numpy as np
import time

from mlagents.envs.environment import UnityEnvironment


class Agent:
    # Environment variables
    env = None
    env_info = None
    default_brain = None
    brain = None

    # Observation of environment variables
    observations = None
    robot_position = None
    arm_position = None
    shovel_position = None
    debris_position = None
    dropzone_position = None
    dropzone_radius = None

    # Setup connection between Unity and Python
    def setup_connection_with_unity(self):
        # Connect to Unity and get environment
        self.env = UnityEnvironment(file_name=None, worker_id=0, seed=1)

        # Reset the environment
        self.env_info = self.env.reset(train_mode=True)

        # Set the default brain to work with
        self.default_brain = "Robot"
        self.brain = self.env_info[self.default_brain]

    # Update observations variable with information about the environment
    def initial_observations(self):
        self.observations = self.env_info[self.default_brain].vector_observations[0]

        # Assets/P5/Scripts/RobotAgent.cs - CollectObservations
        self.robot_position = [self.observations[0], self.observations[1]]
        self.arm_position = self.observations[2]
        self.shovel_position = self.observations[3]
        self.dropzone_position = [self.observations[4], self.observations[5]]
        self.dropzone_radius = self.observations[6]

        # distance sensors (7-36)

        # visible debris (37)

        # debris positions (38-56)
        self.debris_position = [self.observations[38], self.observations[40]]

    def update_observations(self):
        self.observations = self.env_info[self.default_brain].vector_observations[0]

        # Assets/P5/Scripts/RobotAgent.cs - CollectObservations
        self.robot_position = [self.observations[0], self.observations[1]]
        self.debris_position = [self.observations[38], self.observations[40]]

    # Action functions
    # Throttle wheels forward or backwards (1 forwards and -1 backwards)
    # Turn wheels right or left (1 right and -1 left)
    # Rotate arm up or down (1 down and -1 up)
    # Rotate shovel up or down (1 down and -1 up)
    def perform_action(self, throttle, angle, arm_rotation, shovel_rotation):
        # Assets/P5/Scripts/RobotAgent.cs - AgentAction
        action = np.array([throttle, angle, arm_rotation, shovel_rotation])
        return self.env.step({self.default_brain: action}, memory=None, text_action=None)

    # Closes simulation
    def close_simulation(self):
        self.env.close()

    # Prints position of robot and debris
    def print_positions(self):
        print("Robot position:"
              "\nx: " + str(self.observations[0]) +
              "\nz: " + str(self.observations[1]))
        print("Debris position:"
              "\nx: " + str(self.observations[2]) +
              "\nz: " + str(self.observations[3]))


# Main function which will be run after the above code has finished (Setup of connection and definition of functions)
if __name__ == '__main__':
    # Agent object
    agent = Agent()

    # Setup connection
    agent.setup_connection_with_unity()

    # Start time
    start_time = time.time()

    # Update observations, robot_position and debris position for the default brain
    agent.initial_observations()

    # Examine the state space for the default brain
    print("Agent state looks like: \n{}".format(agent.observations))

    # Print initial positions
    print("\nInitial: Positions of observations:")
    agent.print_positions()

    # Drive the robot until the debris and the robot has the same X value
    while agent.debris_position[0] - agent.robot_position[0] > 0:
        # Update information about the environment after action/step is performed
        agent.env_info = agent.perform_action(1, 0, 0, 0)

        # Update observations
        agent.update_observations()

    # Print ended positions
    print("\nEnded: Positions of observations:")
    agent.print_positions()

    # Close simulation
    agent.close_simulation()

    # End time
    end_time = time.time()
    print("\nTime to reach goal: " + str(end_time - start_time))
