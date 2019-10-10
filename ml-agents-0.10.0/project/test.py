from mlagents.envs.environment import UnityEnvironment

env = UnityEnvironment(file_name=None, worker_id=0, seed=1)

#env.step(action, memory=None, text_action=None)

print(str(env))

env.reset(train_mode=True, config=None)

env.close()

print("done");