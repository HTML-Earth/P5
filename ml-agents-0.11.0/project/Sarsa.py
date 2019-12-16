import random
from File_manager import TrainingFileManager
from AgentController import Agent
import matplotlib.pyplot as plt
import RobotObservations as observation


class SarsaLFA:

    def __init__(self, build_scene, gamma=0.99, eta=0.9, epsilon=0.3):
        self.agent = Agent()
        self.agent.setup_connection_with_unity(build_scene)
        self.training_file_manager = TrainingFileManager()

        # Q-function
        self.q_function = {}

        # Variables
        self.gamma = gamma
        self.eta = eta
        self.epsilon = epsilon

        # Weights
        self.weights = []

        # Rewards
        self.reward_per_episode = 0

        self.episode = 1

        # Observation list class(IntEnum)
        self.obs = observation.RobotObservations

    def lookup_q(self, state, action):
        return self.q_function.get((tuple(state), action), 0.0)

    def get_q_value(self, state, action):
        total_q_value = 0

        for i in range(0, len(self.weights)):
            total_q_value += self.weights[i] * self.agent.features[i](state, action)

        self.q_function[(tuple(state), action)] = total_q_value
        return total_q_value

    def choose_action(self, state):
        actions = self.agent.actions

        if random.random() < self.epsilon:
            action = (random.randint(0, 2), random.randint(0, 2))
        else:
            q = [self.lookup_q(state, a) for a in actions]
            max_q = max(q)
            count = q.count(max_q)
            if count > 1:
                best = [i for i in range(len(actions)) if q[i] == max_q]
                i = random.choice(best)
            else:
                i = q.index(max_q)

            action = actions[i]
        return action

    def train_agent(self):
        state = self.agent.get_state()
        action = (0, 0)

        # Variables for x- and y- coordinates
        x_episode = []
        y_delta = []
        y_reward = []

        times_done = 0

        while self.episode <= 1000:
            self.agent.perform_action(*action)

            new_state = self.agent.get_state()
            reward = self.agent.get_reward()
            new_action = self.choose_action(state)

            # Calculate data point for linear regression
            delta = reward + self.gamma * self.get_q_value(new_state, new_action) - self.get_q_value(state, action)

            # Update weights
            for i in range(0, len(self.weights)):
                self.weights[i] = self.weights[i] + self.eta * delta * self.agent.features[i](state, action)

            state = new_state
            action = new_action

            self.reward_per_episode += reward

            if self.agent.is_done():
                goal_state = 0

                if self.agent.get_obs(self.obs.times_won) > times_done:
                    times_done = self.agent.get_obs(self.obs.times_won)
                    goal_state = 1

                # Save x- and y- values
                x_episode.append(self.episode)
                y_delta.append(delta)
                y_reward.append(self.reward_per_episode)

                self.training_file_manager.save_episode_rewards(self.episode, self.reward_per_episode, goal_state)

                self.episode += 1
                self.reward_per_episode = 0
                # TODO: I think this may cause some conflict with AgentReset where we also reset the environment

        # Delta in relation to episodes
        plt.subplot(2, 1, 1)
        plt.plot(x_episode, y_delta)
        plt.ylabel('y - Delta')

        # Reward in relation to episodes
        plt.subplot(2, 1, 2)
        plt.plot(x_episode, y_reward)
        plt.ylabel('y - Reward')
        plt.xlabel('x - Episode')
        plt.title('Delta and Reward in relation to Episode')

        plt.show()

        # Save weights and Q-function
        self.training_file_manager.save_values(self.weights, self.q_function)

    def new_training_agent(self):
        # Create reward file
        self.training_file_manager.create_episode_file()

        # Create training file
        self.training_file_manager.create_training_file()

        for i in range(0, len(self.agent.features)):
            self.weights.append(random.random())

        self.train_agent()

    def continue_training_agent(self, training_file_name):
        # Set path to training file name
        self.training_file_manager.set_training_file(training_file_name)

        self.weights = self.training_file_manager.read_weights()
        self.q_function = self.training_file_manager.read_q_function()

        self.train_agent()
