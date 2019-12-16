import random
from AgentController import Agent


class SarsaLFA:

    def __init__(self):
        self.agent = Agent()
        self.agent.setup_connection_with_unity()

        # Q-function
        self.q_function = {}
        self.weights = []

    def sarsa(self, discount, step_size, epsilon):
        # Weights

        for i in range(0, len(self.agent.features)):
            self.weights.append(random.random())

        state = self.agent.get_state()
        action = 0

        while True:
            self.agent.perform_action(action)
            reward = self.agent.get_reward()
            new_state = self.agent.get_state()
            new_action = self.choose_action(state, epsilon)
            delta = reward + discount * self.lookup_q(new_state, new_action) - self.lookup_q(state, action)
            for i in range(len(self.agent.features)):
                self.weights[i] = self.weights[i] + step_size * delta * self.agent.features[i](state, action)
            state = new_state
            action = new_action

    def choose_action(self, state, epsilon):
        actions = [0, 1, 2, 3, 4]
        if random.random() < epsilon:
            action = random.randint(0, 4)
        else:
            q = [self.lookup_q(state, a) for a in actions]
            max_q = max(q)

            # Check for duplicate amount of the same value
            count = q.count(max_q)
            if count > 1:
                best = [i for i in range(len(actions)) if q[i] == max_q]
                i = random.choice(best)
            else:
                i = q.index(max_q)
            action = actions[i]

        return action

    def lookup_q(self, state, action):
        return self.q_function.get((tuple(state), action), 0.0)

    # TODO: Try this instead of lookup_q
    def get_q_value(self, state, action, features):
        total_q_value = 0

        for i in range(0, len(self.weights)):
            total_q_value += self.weights[i] * features[i](state, action)

        self.q_function[(tuple(state), action)] = total_q_value
        return total_q_value
