from mlagents.envs.environment import UnityEnvironment

class Agent:

    def __init__(self):
        # Unity variables
        self.env = None
        self.env_info = None
        self.default_brain = None

        # Observations
        self.observations = None


        # Actions (Nothing, forward, turn left, reverse, turn right)
        self.actions = [0, 1, 2, 3, 4]

        self.features = [self.feature_0, self.move_forward]

    def setup_connection_with_unity(self):
        # Connect to Unity and get environment
        self.env = UnityEnvironment(worker_id=0, seed=3)

        # Reset the environment
        self.env_info = self.env.reset(train_mode=True)

        # Set the default brain
        self.default_brain = "Robot"

    # Update/initialize observations which act as our states
    def update_observations(self):
        # TODO: Explain [0]
        self.observations = self.env_info[self.default_brain].vector_observations[0]

    def get_state(self):
        state = []
        self.update_observations()

        for i in range(len(self.observations)):
            state.append(self.observations[i])

        return state

    def perform_action(self, action):
        self.env_info = self.env.step({self.default_brain: action})

    def get_reward(self):
        return self.env_info[self.default_brain].rewards[0]

    def feature_0(self, state, action):
        return 1

    def move_forward(self, state, action):

        state_list = list(state)
        velocity_z = state_list[2]

        if velocity_z == 0 and action == 1:
            return 1
        elif velocity_z < -0.1 and action == 1:
            return 1
        elif velocity_z > 0.1 and action == 1:
            return 1
        else:
            return 0


