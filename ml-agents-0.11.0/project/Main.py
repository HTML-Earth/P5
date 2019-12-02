from AgentController import Agent
from SarsaLFA import Sarsa
import sys

# python3.6 Main.py -[mode] [training-file]
# python3.6 Main.py -help
if __name__ == '__main__':
    training_mode = 1
    training_file_name = ""

    # No arguments
    if len(sys.argv) == 1:
        training_mode = 1
    # More than 1 argument
    else:
        if sys.argv[1] == "-new_training":
            training_mode = 1
        elif sys.argv[1] == "-continue_training":
            training_mode = 2
            if not sys.argv[2] is None:
                training_file_name = sys.argv[2]
            else:
                raise Exception("Missing training-file argument.")
        elif sys.argv[1] == "-inference":
            training_mode = 3
            if not sys.argv[2] is None:
                training_file_name = sys.argv[2]
            else:
                raise Exception("Missing training-file argument.")
        elif sys.argv[1] == "-help":
            print("{:35s} Sets mode to 'training mode'. \n"
                  .format("-new_training"))
            print("{:35s} Sets mode to 'training mode' with given training file name. "
                  "\n{:35s} Training file needs to be inside 'training-files' folder. \n"
                  .format("-continue_training [training-file]", ''))
            print("{:35s} Sets mode to 'inference mode' with given training-file name. "
                  "\n{:35s} Training file needs to be inside 'training-files' folder. \n"
                  "\n\n{:35s} Example of training file name: 3_training.txt"
                  .format("-inference [training-file]", '', ''))
            sys.exit(0)

    # Agent object
    agent = Agent()
    # Algorithm object
    sarsa = Sarsa()

    # If training-mode is 1, start new training
    if training_mode == 1:
        sarsa.new_training_agent(agent)
    elif training_mode == 2:
        sarsa.continue_training_agent(agent, training_file_name)
    elif training_mode == 3:
        sarsa.inference_run(agent, training_file_name)
