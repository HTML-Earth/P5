from AgentController import Agent
from SarsaLFA import Sarsa
import sys

# python3.6 main.py -[training-mode]
# python3.6 main.py -help
if __name__ == '__main__':
    training_mode = 1

    for arg in sys.argv:
        if arg == sys.argv[0]:
            print("Type -help to view options.")
            continue
        if arg[0] == "-":
            if arg == "-train":
                training_mode = sys.argv[1]
            elif arg == "-inference":
                raise Exception("Not implemented yet.")
            elif arg == "-help":
                print("{:16s} Sets mode to 'training mode'" .format("-train"))
                print("{:16s} Sets mode to 'inference mode' (Not implemented yet)" .format("-inference"))
                sys.exit(0)
        else:
            raise Exception("Unknown argument. Type -help to view options.")

    # Agent object
    agent = Agent()
    # Algorithm object
    sarsa = Sarsa()

    # If input is 1, start new training
    # TODO If input is 2, resume training
    sarsa.train_agent(agent, training_mode)

    # Close simulation
    agent.close_simulation()
    print(agent.timeElapsed)
