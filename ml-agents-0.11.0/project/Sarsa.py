import random


class SarsaLFA:

    # Q-function
    q_function = {}

    def Sarsa(self, features, discount, step_size, epsilon):
        # Weights
        weights = []

        for i in range(0, len(features)):
            weights.append(random.random())

        state = self.get_state()
        action = 0

        while True:
            self.perform_action(action)
            reward = self.get_reward()
            new_state = self.get_state()
            new_action = self.choose_action(state, epsilon)
            delta = reward + discount * self.lookup_q(new_state, new_action) - self.lookup_q(state, action)
            for i in range(len(features)):
                weights[i] = weights[i] + step_size * delta * features[i](state, action)
            state = new_state
            action = new_action



    def get_state(self):
        return state

    def perform_action(self, action):
        pass

    def get_reward(self):
        return reward

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



