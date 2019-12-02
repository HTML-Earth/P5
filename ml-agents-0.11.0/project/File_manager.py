import os


class TrainingFileManager:

    def __init__(self):
        self.file = None
        self.this_folder = None
        self.path_to_file = None

        self.this_folder = os.path.dirname(os.path.abspath(__file__)) + "/training-files/"

    def create_training_file(self):
        files = os.listdir('training-files/')
        if len(files) == 0:
            self.path_to_file = os.path.join(self.this_folder, '1_training.txt')
            self.file = open(self.path_to_file, "x")
        else:
            amount_of_files = len(files)
            self.path_to_file = os.path.join(self.this_folder, str(amount_of_files + 1) + '_training.txt')
            self.file = open(self.path_to_file, "x")
        self.file.close()

    def save_values(self, weights, q_function):
        self.file = open(self.path_to_file, "w")
        self.file.write(str(weights))
        self.file.write(str(q_function))
        self.file.close()
