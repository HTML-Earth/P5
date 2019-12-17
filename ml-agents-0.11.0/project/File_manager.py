import os


class TrainingFileManager:

    def __init__(self):
        # Training files
        self.file = None
        self.path_to_file = None

        # Reward files
        self.time_file = None
        self.episode_file = None
        self.time_file_name = None
        self.episode_file_name = None

        # Folder variables
        self.folder_name = "Resources/training-files/"
        self.training_file_folder = os.path.dirname(os.path.abspath(__file__)) + "/Resources/training-files/"

        # Reward folder variables
        self.time_file_folder = os.path.dirname(os.path.abspath(__file__)) + "/Resources/time-files/"
        self.episode_file_folder = os.path.dirname(os.path.abspath(__file__)) + "/Resources/episode-files/"

    def create_training_file(self):
        # If folder, 'training-files' does not exist, create it
        if not os.path.isdir(self.folder_name):
            os.mkdir(self.folder_name)

        # listdir returns an array of filenames in 'training-files'
        files = os.listdir(self.folder_name)

        # If '1_training.txt' does not exist, create it
        if not os.path.isfile(self.training_file_folder + "1_training.txt"):
            # Insert '1_training.txt' at the end of the path
            self.path_to_file = os.path.join(self.training_file_folder, '1_training.txt')
            self.file = open(self.path_to_file, "x")
        # Create '#_training.txt' where # is based on the amount of files in 'training-files'
        else:
            amount_of_files = len(files)
            self.path_to_file = os.path.join(self.training_file_folder, str(amount_of_files + 1) + '_training.txt')
            self.file = open(self.path_to_file, "x")

        self.file.close()

    def set_training_file(self, training_file_name):
        self.path_to_file = os.path.join(self.training_file_folder, training_file_name)

        file_index_number = training_file_name[0]
        self.episode_file_name = str(file_index_number) + "_episode_file.txt"
        self.time_file_name = str(file_index_number) + "_time_file.txt"

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

            action = split[1].split(', ')[1]

            reward = state_action_reward_string.split(':')[1]
            reward = float(reward)

            # Add Q(s, a) : r
            q_function[(state_tuple, action)] = reward

        self.file.close()

        return q_function

    def save_values(self, weights, q_function):
        self.file = open(self.path_to_file, "w")

        # Write weights and Q-function to training file
        weights_formatted = [round(x, 2) for x in weights]
        self.file.writelines(str(weights_formatted) + "\n")
        for entry in q_function:
            self.file.write(str(entry) + ":" + str(round(q_function[entry], 2)) + "\n")

        self.file.close()

    def create_time_file(self):
        if not os.path.isdir(self.time_file_folder):
            os.mkdir(self.time_file_folder)

        files = os.listdir(self.time_file_folder)

        if not os.path.isfile(self.time_file_folder + "1_time_file.txt"):
            self.time_file_name = "1_time_file.txt"
            self.path_to_file = os.path.join(self.time_file_folder, self.time_file_name)
            self.time_file = open(self.path_to_file, "x")
        else:
            amount_of_files = len(files)
            self.time_file_name = str(amount_of_files + 1) + "_time_file.txt"

            self.path_to_file = os.path.join(self.time_file_folder, self.time_file_name)
            self.time_file = open(self.path_to_file, "x")

        self.time_file.write("Time,Reward\n")
        self.time_file.close()

    def save_time_rewards(self, time_passed, reward_per_time):
        self.time_file = open(self.time_file_folder + self.time_file_name, "a")
        self.time_file.write(str(int(time_passed)) + "," + str(round(reward_per_time, 2)) + "\n")
        self.time_file.close()

    def create_episode_file(self):
        if not os.path.isdir(self.episode_file_folder):
            os.mkdir(self.episode_file_folder)

        files = os.listdir(self.episode_file_folder)

        if not os.path.isfile(self.episode_file_folder + "1_episode_file.txt"):
            self.episode_file_name = "1_episode_file.txt"
            self.path_to_file = os.path.join(self.episode_file_folder, self.episode_file_name)
            self.episode_file = open(self.path_to_file, "x")
        else:
            amount_of_files = len(files)
            self.episode_file_name = str(amount_of_files + 1) + "_episode_file.txt"

            self.path_to_file = os.path.join(self.episode_file_folder, self.episode_file_name)
            self.episode_file = open(self.path_to_file, "x")

        self.episode_file.write("Episode,Reward,Goal-state reached,Completion-time\n")
        self.episode_file.close()

    def save_episode_rewards(self, episode, reward_in_episode, goal_state, completion_time):
        self.episode_file = open(self.episode_file_folder + self.episode_file_name, "a")
        self.episode_file.write(str(episode) + "," +
                                str(round(reward_in_episode)) + "," +
                                str(goal_state) + "," +
                                str(completion_time) + "\n")
        self.episode_file.close()
