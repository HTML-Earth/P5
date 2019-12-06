import random
import time
from File_manager import TrainingFileManager
from AgentController import Agent


class SarsaLFA:

    def __init__(self, gamma=0.5, eta=0.4, epsilon=0.9):
        self.agent = Agent()
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
        self.reward_per_time = 0

        # Time
        self.start_time = None
        self.episode = 1
        self.target_time = 10

    def get_q_value(self, state, action):
        total_q_value = 0

        for i in range(0, len(self.weights)):
            total_q_value += self.weights[i] * self.agent.features[i](state, action)

        self.q_function[(tuple(state), action)] = total_q_value
        return total_q_value

    def choose_action(self, state):
        actions = self.agent.actions

        if random.random() < self.epsilon:
            action = (random.randint(-1, 1), random.randint(-1, 1), random.randint(-1, 1), random.randint(-1, 1))
        else:
            q = [self.get_q_value(state, a) for a in actions]
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
        action = (1, 0, 0, 0)
        self.start_time = time.time()

        while True:
            self.agent.perform_action(*action)

            reward = self.agent.get_reward()
            new_state = self.agent.get_state()
            new_action = self.choose_action(state)

            # Calculate data point for linear regression
            delta = reward + self.gamma * self.get_q_value(new_state, new_action) - self.get_q_value(state, action)

            # Update weights
            for i in range(0, len(self.weights)):
                self.weights[i] = self.weights[i] + self.eta * delta * self.agent.features[i](state, action)

            state = new_state
            action = new_action

            self.reward_per_episode += reward
            self.reward_per_time += reward

            self.save()

    def new_training_agent(self):
        # Create reward files
        self.training_file_manager.create_time_file()
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

    def save(self):
        # Set time checkpoint
        checkpoint = time.time()
        time_passed = checkpoint - self.start_time

        # Every 10 seconds
        if time_passed > self.target_time:
            # Save time and reward
            self.training_file_manager.save_time_rewards(time_passed, self.reward_per_time)
            self.reward_per_time = 0

            # Save Weights and Q-function
            self.training_file_manager.save_values(self.weights, self.q_function)

            # Set new target time
            self.target_time += 10

        if self.agent.is_done():
            self.training_file_manager.save_episode_rewards(self.episode, self.reward_per_episode)
            self.episode += 1
            self.reward_per_episode = 0

            # Save Weights and Q-function
            self.training_file_manager.save_values(self.weights, self.q_function)
            self.agent.reset_simulation()
