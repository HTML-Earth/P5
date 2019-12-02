from AgentController import Agent
from SarsaLFA import Sarsa
import sys

# python3.6 Main.py -[training-mode]
# python3.6 Main.py -help
if __name__ == '__main__':
    training_mode = 1

    for arg in sys.argv:
        if arg == sys.argv[0]:
            continue
        if arg[0] == "-":
            if arg == "-new_train":
                training_mode = sys.argv[1]
            elif arg == "-inference":
                raise Exception("Not implemented yet.")
            # TODO Add continue training mode
            elif arg == "-help":
                print("{:16s} Sets mode to 'training mode'" .format("-new_training"))
                print("{:16s} Sets mode to 'inference mode' (Not implemented yet)" .format("-inference"))
                sys.exit(0)
        else:
            raise Exception("Unknown argument. Type -help to view options.")

    # Agent object
    agent = Agent()
    # Algorithm object
    sarsa = Sarsa()

    # If training-mode is 1, start new training
    if training_mode == 1:
        sarsa.train_agent(agent)
