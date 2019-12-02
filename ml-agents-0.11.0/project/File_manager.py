import os


class TrainingFileManager:

    def __init__(self):
        self.file = None
        self.path_to_file = None

        self.folder_name = "training-files"
        self.this_folder = os.path.dirname(os.path.abspath(__file__)) + "/training-files/"

    def create_training_file(self):
        # If folder, 'training-files' does not exist, create it
        if not os.path.isdir(self.folder_name):
            os.mkdir(self.folder_name)

        # listdir returns an array of filenames in 'training-files'
        files = os.listdir(self.folder_name)
        print(files)

        # If '1_training.txt' does not exist, create it
        if not os.path.isfile(self.this_folder + "1_training.txt"):
            # Insert '1_training.txt' at the end of the path
            self.path_to_file = os.path.join(self.this_folder, '1_training.txt')
            self.file = open(self.path_to_file, "x")
        # Create '#_training.txt' where # is based on the amount of files in 'training-files'
        else:
            amount_of_files = len(files)
            self.path_to_file = os.path.join(self.this_folder, str(amount_of_files + 1) + '_training.txt')
            self.file = open(self.path_to_file, "x")

        self.file.close()

    def save_values(self, weights, q_function):
        self.file = open(self.path_to_file, "w")

        # Write weights and Q-function to training file
        self.file.writelines(str(weights) + "\n")
        for entry in q_function:
            self.file.write(str(entry) + ":" + str(q_function[entry]) + "\n")

        self.file.close()

    def set_training_file(self, training_file_name):
        self.path_to_file = os.path.join(self.this_folder, training_file_name)

    def read_weights(self):
        self.file = open(self.path_to_file, "r")

        # Read first line in file
        line = self.file.readline()
        self.file.close()

        # Remove 1:'[', and 2: ']' \n and convert to list
        weights = line[1:-2].split(',')

        # Convert strings in list to floats
        return [float(i) for i in weights]

    def read_q_function(self):
        self.file = open(self.path_to_file, "r")

        # Skip first line and read next line in file
        self.file.readline()

        q_function = {}

        # For every entry, add to q_function
        for line in self.file.readlines():
            state_action_reward_string = line[1:-1]
            split = state_action_reward_string.split(')')

            state = split[0][1:].split(',')
            state = [float(i) for i in state]
            state_tuple = tuple(state)

            action = split[1][3:].split(',')
            action = [float(i) for i in action]
            action_tuple = tuple(action)

            reward = state_action_reward_string.split(':')[1]
            reward = float(reward)

            # Add Q(s, a) : r
            q_function[(state_tuple, action_tuple)] = reward

        self.file.close()

        return q_function
