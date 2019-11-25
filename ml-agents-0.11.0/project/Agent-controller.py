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

    # Setup connection between Unity and Python
    def setup_connection_with_unity(self):
        # Connect to Unity and get environment
        self.env = UnityEnvironment(file_name=None, worker_id=0, seed=1)

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

    # Action functions
    def perform_action(self, throttle, angle, arm_rotation, shovel_rotation):
        action = np.array([throttle, angle, arm_rotation, shovel_rotation])
        return self.env.step({self.default_brain: action})

    # Closes simulation
    def close_simulation(self):
        self.env.close()


# Main function which will be run after the above code has finished (Setup of connection and definition of functions)
if __name__ == '__main__':
    # Agent object
    agent = Agent()

    # Setup connection
    agent.setup_connection_with_unity()

    # Update observations, robot_position and debris position for the default brain
    agent.initial_observations()

    # Examine the state space for the default brain
    print("Agent state looks like: \n{}".format(agent.observations))

    # Print debris visibility
    # agent.print_visibility()

    # Drive the robot until the debris and the robot has the same X value
    while True:
        # Update information about the environment after action/step is performed
        agent.env_info = agent.perform_action(1, 0, 0, 0)

        # Update observations
        agent.update_observations()

    # Close simulation
    # agent.close_simulation()
