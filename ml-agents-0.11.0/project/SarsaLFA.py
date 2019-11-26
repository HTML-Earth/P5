import random
import Agent-controller as Agent


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
#
#

class Sarsa:

    def __init__(self, gamma=0.9, eta=0.1):
        self.gamma = gamma
        self.eta = eta
        self.actions = "noget_smart()"

    def getQ(self, state, action):
        return "q"

    def choose_action(self, state, action):
        if random.random() < self.eta:
            action = (random.randint(-1,1), random.randint(-1,1), random.randint(-1,1), random.randint(-1,1))
        else:
            q = [self.getQ(state, a) for a in self.actions]
            maxQ = max(q)
            count = q.count(maxQ)
            if count > 1:
                best = [i for i in range(len(self.actions)) if q[i] == maxQ]
                i = random.choice(best)
            else:
                i = q.index(maxQ)

            action = self.actions[i]
        return action

    def sarsa_lfa (self, agent, feature_tuple, gamma, eta):
        weights = None
        q = {}

        agent.update_observations()
        cur_state = agent.observations
        cur_action = (1, 0, 0, 0)

        while True:
            new_state = agent.perform_action(cur_action)
            reward = agent.previous_rewards
            new_action = self.choose_action(cur_state, cur_action)
            delta = reward + gamma * self.getQ(new_state, new_action) - self.getQ(cur_state, cur_action)
            for i in range (len(feature_tuple)):
                weights[i] = weights[i] + eta * delta * feature_tuple[i].get(cur_state, cur_action)

            cur_state = new_state
            cur_action = new_action
            