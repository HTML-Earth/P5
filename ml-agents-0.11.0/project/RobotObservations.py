# This file was generated by RobotAgent.cs
from enum import IntEnum


class RobotObservations(IntEnum):
    # Accessible by self.obs.'field_name'
    robot_position_x = 0
    robot_position_z = 1
    robot_rotation = 2
    robot_velocity_x = 3
    robot_velocity_y = 4
    robot_velocity_z = 5
    shovel_position = 6
    dropzone_position_x = 7
    dropzone_position_z = 8
    dropzone_radius = 9
    sensor_measurement_1 = 10
    sensor_measurement_2 = 11
    sensor_measurement_3 = 12
    sensor_measurement_4 = 13
    sensor_measurement_5 = 14
    sensor_measurement_6 = 15
    sensor_measurement_7 = 16
    sensor_measurement_8 = 17
    sensor_measurement_9 = 18
    sensor_measurement_10 = 19
    sensor_measurement_11 = 20
    sensor_measurement_12 = 21
    sensor_measurement_13 = 22
    sensor_measurement_14 = 23
    sensor_measurement_15 = 24
    sensor_measurement_16 = 25
    sensor_measurement_17 = 26
    sensor_measurement_18 = 27
    sensor_measurement_19 = 28
    sensor_measurement_20 = 29
    sensor_measurement_21 = 30
    sensor_measurement_22 = 31
    sensor_measurement_23 = 32
    sensor_measurement_24 = 33
    sensor_measurement_25 = 34
    sensor_measurement_26 = 35
    sensor_measurement_27 = 36
    sensor_measurement_28 = 37
    sensor_measurement_29 = 38
    sensor_measurement_30 = 39
    debris_1_position_x = 40
    debris_1_position_y = 41
    debris_1_position_z = 42
    debris_2_position_x = 43
    debris_2_position_y = 44
    debris_2_position_z = 45
    debris_3_position_x = 46
    debris_3_position_y = 47
    debris_3_position_z = 48
    debris_4_position_x = 49
    debris_4_position_y = 50
    debris_4_position_z = 51
    debris_5_position_x = 52
    debris_5_position_y = 53
    debris_5_position_z = 54
    debris_6_position_x = 55
    debris_6_position_y = 56
    debris_6_position_z = 57
    simulation_time = 58
    robot_in_dropzone = 59
    getting_closer_to_debris_1 = 60
    getting_closer_to_debris_2 = 61
    getting_closer_to_debris_3 = 62
    getting_closer_to_debris_4 = 63
    getting_closer_to_debris_5 = 64
    getting_closer_to_debris_6 = 65
    debris_in_shovel = 66
    angle_robot_debris_1 = 67
    angle_robot_debris_2 = 68
    angle_robot_debris_3 = 69
    angle_robot_debris_4 = 70
    angle_robot_debris_5 = 71
    angle_robot_debris_6 = 72
    debris_in_front = 73
    robot_facing_debris = 74
    angle_to_dropzone = 75
    debris_to_dropzone_1 = 76
    debris_to_dropzone_2 = 77
    debris_to_dropzone_3 = 78
    debris_to_dropzone_4 = 79
    debris_to_dropzone_5 = 80
    debris_to_dropzone_6 = 81
    next_angle_to_debris_1 = 82
    next_angle_to_debris_2 = 83
    next_angle_to_debris_3 = 84
    next_angle_to_debris_4 = 85
    next_angle_to_debris_5 = 86
    next_angle_to_debris_6 = 87
