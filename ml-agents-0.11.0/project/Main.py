from AgentController import Agent
from Sarsa import SarsaLFA
import sys

# python3.6 Main.py -[mode] [training-file]
# python3.6 Main.py -help
if __name__ == '__main__':
    training_mode = 1
    training_file_name = ""
    build_scene = None

    if len(sys.argv) > 1:
        # New training
        if sys.argv[1] == "-n":
            training_mode = 1

            # Build scene
            if not sys.argv[2] is None:
                build_scene = sys.argv[2]

        # Continue training
        elif sys.argv[1] == "-c":
            training_mode = 2

            # Build scene
            if not sys.argv[2] is None:
                build_scene = sys.argv[2]

            # Training file
            if not sys.argv[3] is None:
                training_file_name = sys.argv[3]
            else:
                raise Exception("Missing training-file argument.")

        elif sys.argv[1] == "-help":
            print("{:35s} Sets mode to 'training mode'. \n"
                  .format("-n [Optional: path to build scene]"))

            print("{:35s} Sets mode to 'training mode' with given training file name. "
                  "\n{:35s} Training file needs to be inside 'training-files' folder. \n"
                  .format("-c [Build scene] [training-file]", ''))

            print("{:35s} Example of new training session:\n "
                  "{:35s}python3.6 Main.py -n 1x3_scene.app\n\n"
                  "{:35s} Example of continued training session:\n "
                  "{:35s}python3.6 Main.py -c 1x3_scene.app 1_training.txt"
                  .format("", "", "", ""))
            sys.exit(0)

    # Algorithm object
    sarsa = SarsaLFA(build_scene)

    if training_mode == 1:
        sarsa.new_training_agent()
    elif training_mode == 2:
        sarsa.continue_training_agent(training_file_name)
