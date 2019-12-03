import random
from AgentController import Agent


class SarsaLFA:

    def __init__(self, agent, gamma=0.3, eta=4, epsilon=0.9):
        self.agent = agent

        self.q_function = {}

        self.actions = agent.actions

        self.gamma = gamma
        self.eta = eta
        self.epsilon = epsilon

        self.weights = []
        for i in range(0, len(self.agent.feature_values)):
            self.weights.append(random.randint(-2, 2))

    def choose_action(self, state):
        if random.random() < self.epsilon:
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

    def get_q_value(self, state, action):
        total_q_value = 0

        for i in range(0, len(self.agent.feature_values)):
            total_q_value += self.weights[i] * self.agent.get_feature_values(state, action)[i]

        self.q_function[(tuple(state), action)] = total_q_value
        return total_q_value

    def train_agent(self):
        state = self.agent.get_state()
        action = (1, 0, 0, 0)

        while True:
            self.agent.perform_action(*action)

            reward = self.agent.get_reward()
            new_state = self.agent.get_state()
            new_action = self.choose_action(state)

            # Get Q-values and feature values
            feature_values = self.agent.feature_values
            q_value = self.get_q_value(state, action)

            self.agent.update_feature_values()
            new_q_value = self.get_q_value(new_state, new_action)

            # Calculate data point for linear regression
            delta = reward + self.gamma * new_q_value - q_value

            # Update weights
            for i in range(0, len(feature_values)):
                self.weights[i] = self.weights[i] + self.eta * delta * feature_values[i]

            state = new_state
            action = new_action

            if self.agent.is_done():
                self.agent.reset_simulation()


if __name__ == '__main__':
    agent = Agent()
    sarsa = SarsaLFA(agent)
    sarsa.train_agent()
