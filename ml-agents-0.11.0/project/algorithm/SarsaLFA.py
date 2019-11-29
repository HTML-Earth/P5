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

    def __init__(self, gamma=0.9, eta=0.1):
        # Q function
        self.q = {}

        # Feature function
        self.feature = {}

        self.gamma = gamma
        self.eta = eta

        # All possible actions for the Agent (81 possible actions)
        self.actions = [(i, j, k, l) for i in range(-1, 2) for j in range(-1, 2) for k in range(-1, 2) for l in range(-1, 2)]

    def get_q_value(self, state, action):
        # Return value for given state and action
        # 0.0 is default if there exist no value for given state and action
        return self.q.get((state, action), 0.0)

    def get_feature_value(self, state, action):
        observations = state
        velocity_z = observations[5] # z means the velocity when moving forward and backward.
        sensors_front = [observations[11], observations[12], observations[40]]
        sensors_behind = [-observations[26], -observations[25], -observations[27]]

        featureValues = []

        # Check if robot is throttling into wall
        if sensors_front < [velocity_z]:
            featureValues.append(1)
        else:
            featureValues.append(0)

        # Check if robot is reversing into wall
        if sensors_behind > [velocity_z]:
            featureValues.append(1)
        else:
            featureValues.append(0)

        # getting closer to debris
        #for debris_location in debris_locations:
         #   if

        # Check if robot is within the dropzone
        if observations[60]:
            featureValues.append(1)
        else:
            featureValues.append(0)

        # Check if robot is getting closer to debris
        for i in range(61, 67):
            if observations[i]:
                featureValues.append(1)
            else:
                featureValues.append(0)

        return featureValues

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

    def sarsa_lfa(self, agent, feature_tuple):
        # Arbitrarily initalize weights which will be updated later
        # TODO Assume that there is an extra feature F0(s,a) whose value is always 1,
        #  so that w0 is not a special case.
        weights = []
        for i in range(len(feature_tuple)):
            weights.append(random.randint(0, 5))

        # Update observations to make sure current state is correct
        agent.update_observations()

        # Observe current state s
        cur_state = agent.observations
        # Select action a
        cur_action = (1, 0, 0, 0)

        while True:
            # Perform action and observe state s'
            new_state = agent.perform_action(cur_action)
            # Observe reward r
            reward = agent.rewards
            # Select action a' (using a policy based on the Q-function
            new_action = self.choose_action(cur_state, cur_action)

            # TODO Explain what delta is
            delta = reward + self.gamma * self.get_q_value(new_state, new_action) - self.get_q_value(cur_state, cur_action)

            # Update each weight for each feature
            for i in range(len(feature_tuple)):
                weights[i] = weights[i] + self.eta * delta * feature_tuple[i].get(cur_state, cur_action)

            # Update state and action so these will be performed
            cur_state = new_state
            cur_action = new_action
