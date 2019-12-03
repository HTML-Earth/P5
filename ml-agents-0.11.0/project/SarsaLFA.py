import random
from File_manager import TrainingFileManager
import time

# Q_w(s,a) = w_0 + w_1 F_1(s,a) +  ... + w_n F_n(s,a)
#
# SARSA_LFA(F, gamma, eta)
#   Inputs
#       F = <F_1, ... , F_n>: a set of features. Define F_0(s,a) = 1.
#       gamma ∈ [0,1]: discount factor
#       eta > 0: step size for gradient descent
#
#   Local
#       weights w = <w_0, ... , w_n>, initialized arbitrarily
#
#   observe current state s
#   select action a
#   repeat
#       do(a)
#       observe reward r and state s'
#       select action a' (using policy based on Q_w)
#       delta := r + gamma * Q_w(s', a') - Q_w(s,a)
#       for i = 0 to n do
#           w_i := w_i + eta * delta * F_i(s,a)
#       s := s'
#       a := a'
#   until termination


class Sarsa:

    def __init__(self, gamma=0.9, eta=0.3):
        # Q function
        self.q = {}
        # Feature function (Array of 11 0's)
        self.feature_values = [0] * 11
        # Weights
        self.weights = []

        self.gamma = gamma
        self.eta = eta

        # Total amount of rewards per episode
        self.reward_in_episode = 0

        # Total amount of rewards per time unit
        self.reward_per_time = 0

        # Episode number
        self.episode = 1

        # Time unit
        self.target_time = 10

        # All possible actions for the Agent (81 possible actions)
        self.actions = [(i, j, k, l)
                        for i in range(-1, 2)
                        for j in range(-1, 2)
                        for k in range(-1, 2)
                        for l in range(-1, 2)]

        self.training_file_manager = TrainingFileManager()

    # Return value for given state and action
    # 0.0 is default if there exist no value for given state and action
    def get_q_value(self, state, action):
        return self.q.get((tuple(state), action), 0.0)

    # Update Q-value for given state and action
    # TODO Q-value (reward) should be: Q_w(s,a) = w_0 + w_1 F_1(s,a) +  ... + w_n F_n(s,a)
    def learn_q(self, state, action, reward):
        total_q_value = 0
        for i in range(0, len(self.feature_values) - 1):
            total_q_value = self.weights[i] * self.feature_values[i]

        self.q[(tuple(state), action)] = total_q_value

    # Update Feature values array
    # TODO Move method to Agent class
    def update_feature_values(self, observations):
        # z means the velocity when moving forward and backward.
        velocity_z = observations[5]
        sensors_front = [observations[11], observations[12], observations[40]]
        sensors_behind = [-observations[26], -observations[25], -observations[27]]

        # Check if robot is throttling into wall
        if sensors_front < [velocity_z]:
            self.feature_values[0] = 1
        else:
            self.feature_values[0] = 0

        # Check if robot is reversing into wall
        if sensors_behind > [velocity_z]:
            self.feature_values[1] = 1
        else:
            self.feature_values[1] = 0

        # Check if robot is within the dropZone
        if observations[60]:
            self.feature_values[2] = 1
        else:
            self.feature_values[2] = 0

        # Check if robot is getting closer to debris
        index = 3
        for i in range(61, 67):
            if observations[i]:
                self.feature_values[index] = 1
            else:
                self.feature_values[index] = 0
            index += 1

        # Check if ready to pickup debris
        if observations[6] == 330 and observations[7] == 360 - 47:
            self.feature_values[index] = 1
        else:
            self.feature_values[index] = 0

        # Check if debris is in shovel
        if observations[67]:
            self.feature_values[10] = 1
        else:
            self.feature_values[10] = 0

    # Choose action based on policy (highest Q-value or random)
    def choose_action(self, state):
        if random.random() < self.eta:
            action = (random.randint(-1, 1), random.randint(-1, 1), random.randint(-1, 1), random.randint(-1, 1))
        else:
            q = [self.get_q_value(state, a) for a in self.actions]
            max_q = max(q)
            count = q.count(max_q)
            if count > 1:
                best = [i for i in range(len(self.actions)) if q[i] == max_q]
                i = random.choice(best)
            else:
                i = q.index(max_q)

            action = self.actions[i]
        return action

    def new_training_agent(self, agent):
        # Create reward files
        self.training_file_manager.create_time_file()
        self.training_file_manager.create_episode_file()

        # Create training file
        self.training_file_manager.create_training_file()

        # Update observations to make sure current state is correct
        agent.update_observations()
        observations = agent.observations

        # Update features
        self.update_feature_values(observations)

        # Observe current state s
        cur_state = self.feature_values
        # Select action a
        cur_action = (1, 0, 0, 0)

        # Arbitrarily initalize weights which will be updated later
        # TODO Assume that there is an extra feature F0(s,a) whose value is always 1,
        #  so that w0 is not a special case.
        for i in range(len(self.feature_values)):
            self.weights.append(random.randint(0, 5))

        self.train(agent, cur_state, cur_action)

    def continue_training_agent(self, agent, training_file_name):
        self.training_file_manager.set_training_file(training_file_name)

        self.weights = self.training_file_manager.read_weights()
        self.q = self.training_file_manager.read_q_function()

        agent.update_observations()
        observations = agent.observations

        self.update_feature_values(observations)

        cur_state = self.feature_values
        cur_action = (1, 0, 0, 0)

        self.train(agent, cur_state, cur_action)

    def train(self, agent, cur_state, cur_action):
        start_time = time.time()
        while True:
            # Perform action
            new_environment_info = agent.perform_action(*cur_action).get('Robot')
            new_observations = new_environment_info.vector_observations[0]

            # Update features
            self.update_feature_values(new_observations)

            # Observe state s'
            new_state = self.feature_values

            # Observe reward r
            reward = new_environment_info.rewards[0]

            # Select action a' (using a policy based on the Q-function
            new_action = self.choose_action(cur_state)

            # Insert (state, action) and reward into Q-function
            self.learn_q(cur_state, cur_action, reward)

            # TODO Explain what delta is
            delta = reward + self.gamma * self.get_q_value(new_state, new_action) - self.get_q_value(cur_state, cur_action)

            # Update each weight for each feature
            for i in range(len(self.feature_values)):
                self.weights[i] = self.weights[i] + self.eta * delta * self.feature_values[i]

            # Update state and action so these will be performed
            cur_state = new_state
            cur_action = new_action

            self.reward_in_episode += reward

            # Set time checkpoint
            checkpoint = time.time()
            time_passed = checkpoint - start_time

            # Every 10 seconds
            if time_passed > self.target_time:
                # Save time and reward
                self.training_file_manager.save_time_rewards(time_passed, self.reward_per_time)
                self.reward_per_time = 0

                # Save Weights and Q-function
                self.training_file_manager.save_values(self.weights, self.q)

                # Set new target time
                self.target_time += 10

            # Agent marked as Done
            if new_environment_info.local_done[0]:
                self.training_file_manager.save_episode_rewards(self.episode, self.reward_in_episode)
                self.episode += 1
                self.reward_in_episode = 0

                # Save Weights and Q-function
                self.training_file_manager.save_values(self.weights, self.q)
                agent.reset_simulation()

    def inference_run(self, agent, training_file_name):
        self.training_file_manager.set_training_file(training_file_name)

        agent.update_observations()

        observations = agent.observations
        cur_state = self.feature_values
        cur_action = (1, 0, 0, 0)

        self.update_feature_values(observations)

        self.weights = self.training_file_manager.read_weights()
        self.q = self.training_file_manager.read_q_function()

        while True:
            new_environment_info = agent.perform_action(*cur_action).get('Robot')
            new_observations = new_environment_info.vector_observations[0]
            new_action = self.choose_action(cur_state)
            new_state = self.feature_values

            # Update features
            self.update_feature_values(new_observations)

            cur_state = new_state
            cur_action = new_action

            if new_environment_info.local_done[0]:
                agent.reset_simulation()
