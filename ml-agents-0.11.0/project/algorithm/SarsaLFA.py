import random

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

    def __init__(self, gamma=0.9, eta=0.6):
        # Q function
        self.q = {}
        # Feature function
        self.feature_values = [0 for i in range(0, 10)]

        self.gamma = gamma
        self.eta = eta

        # All possible actions for the Agent (81 possible actions)
        self.actions = [(i, j, k, l) for i in range(-1, 2) for j in range(-1, 2) for k in range(-1, 2) for l in range(-1, 2)]

    def get_q_value(self, state, action):
        # Return value for given state and action
        # 0.0 is default if there exist no value for given state and action
        return self.q.get((tuple(state), action), 0.0)

    def get_feature_value(self, state, action):
        observations = state

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

        # Check if robot is within the dropzone
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

    def choose_action(self, state, action):
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

    def sarsa_lfa(self, agent):
        # Update observations to make sure current state is correct
        agent.update_observations()

        # Observe current state s
        cur_state = agent.observations
        # Select action a
        cur_action = (1, 0, 0, 0)

        self.get_feature_value(cur_state, cur_action)

        # Arbitrarily initalize weights which will be updated later
        # TODO Assume that there is an extra feature F0(s,a) whose value is always 1,
        #  so that w0 is not a special case.
        weights = []
        for i in range(len(self.feature_values)):
            weights.append(random.randint(0, 5))

        while True:
            # Perform action and observe state s'
            new_state = agent.perform_action(*cur_action).get('Robot').vector_observations[0]
            # Observe reward r
            reward = agent.get_reward()[0]
            # Select action a' (using a policy based on the Q-function
            new_action = self.choose_action(cur_state, cur_action)

            print("Chosen action: {:4s}" + str(new_action))
            print("Reward: " + str(reward))

            # TODO Explain what delta is
            delta = reward + self.gamma * self.get_q_value(new_state, new_action) - self.get_q_value(cur_state, cur_action)

            # Update features
            self.get_feature_value(cur_state, cur_action)

            # Update each weight for each feature
            for i in range(len(self.feature_values)):
                weights[i] = weights[i] + self.eta * delta * self.feature_values[i]

            # Update state and action so these will be performed
            cur_state = new_state
            cur_action = new_action
