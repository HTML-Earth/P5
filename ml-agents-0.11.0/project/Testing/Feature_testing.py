import unittest
from AgentController import Agent


class TestFeatures(unittest.TestCase):

    agent = Agent()
    transform_value = 1 / 360

    def test_rotation_01(self):
        self.agent.observations[2] = 10
        rotation = self.agent.observations[2]

        expect_new_rotation = round((rotation + self.agent.rotation_constant) / 360, 4)
        new_rotation = round(self.agent.rotation(0, (0, 1)), 4)

        self.assertEqual(new_rotation, expect_new_rotation)

    def test_rotation_02(self):
        self.agent.observations[2] = 10
        rotation = self.agent.observations[2]

        expect_new_rotation = round((rotation - self.agent.rotation_constant) * self.transform_value, 4)
        new_rotation = round(self.agent.rotation(0, (0, -1)), 4)

        self.assertEqual(new_rotation, expect_new_rotation)

    @unittest.expectedFailure
    def test_rotation_03(self):
        self.agent.observations[2] = 0
        rotation = self.agent.observations[2]

        new_rotation = round(self.agent.rotation(0, (0, -1)), 4)
        expect_new_rotation = round((rotation - self.agent.rotation_constant) * self.transform_value, 4)

        self.assertEqual(new_rotation, expect_new_rotation)

    @unittest.expectedFailure
    def test_rotation_04(self):
        agent = Agent()
        agent.observations[2] = 359
        rotation = agent.observations[2]

        expect_new_rotation = round((rotation + agent.rotation_constant) / 360, 4)
        new_rotation = round(agent.rotation(0, (0, 1)), 4)

        self.assertEqual(new_rotation, expect_new_rotation)

    @unittest.expectedFailure
    def test_rotation_05(self):
        agent = Agent()
        agent.observations[2] = 1
        rotation = agent.observations[2]

        expect_new_rotation = round((rotation + agent.rotation_constant) / 360, 4)
        new_rotation = round(agent.rotation(0, (0, -1)), 4)

        self.assertEqual(expect_new_rotation, new_rotation)


if __name__ == '__main__':
    unittest.main()
