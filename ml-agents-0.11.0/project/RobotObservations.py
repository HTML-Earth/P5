from enum import IntEnum


class RobotObservations(IntEnum):
    # Accessible by self.obs.'field_name'
    robot_position_x = 0
    robot_position_z = 1

    robot_rotation = 2

    robot_velocity_x = 3
    robot_velocity_y = 4
    robot_velocity_z = 5

    arm_position = 6
    shovel_position = 7

    dropzone_position_x = 8
    dropzone_position_z = 9
    dropzone_radius = 10

    sensor_measurement_1 = 11
    sensor_measurement_2 = 12
    sensor_measurement_3 = 13
    sensor_measurement_4 = 14
    sensor_measurement_5 = 15
    sensor_measurement_6 = 16
    sensor_measurement_7 = 17
    sensor_measurement_8 = 18
    sensor_measurement_9 = 19
    sensor_measurement_10 = 20
    sensor_measurement_11 = 21
    sensor_measurement_12 = 22
    sensor_measurement_13 = 23
    sensor_measurement_14 = 24
    sensor_measurement_15 = 25
    sensor_measurement_16 = 26
    sensor_measurement_17 = 27
    sensor_measurement_18 = 28
    sensor_measurement_19 = 29
    sensor_measurement_20 = 30
    sensor_measurement_21 = 31
    sensor_measurement_22 = 32
    sensor_measurement_23 = 33
    sensor_measurement_24 = 34
    sensor_measurement_25 = 35
    sensor_measurement_26 = 36
    sensor_measurement_27 = 37
    sensor_measurement_28 = 38
    sensor_measurement_29 = 39
    sensor_measurement_30 = 40

    debris_1_position_x = 41
    debris_1_position_y = 42
    debris_1_position_z = 43

    debris_2_position_x = 44
    debris_2_position_y = 45
    debris_2_position_z = 46

    debris_3_position_x = 47
    debris_3_position_y = 48
    debris_3_position_z = 49

    debris_4_position_x = 50
    debris_4_position_y = 51
    debris_4_position_z = 52

    debris_5_position_x = 53
    debris_5_position_y = 54
    debris_5_position_z = 55

    debris_6_position_x = 56
    debris_6_position_y = 57
    debris_6_position_z = 58

    simulation_time = 59

    robot_in_dropzone = 60

    getting_closer_to_debris_1 = 61
    getting_closer_to_debris_2 = 62
    getting_closer_to_debris_3 = 63
    getting_closer_to_debris_4 = 64
    getting_closer_to_debris_5 = 65
    getting_closer_to_debris_6 = 66

    debris_in_shovel = 67

    angle_robot_debris_1 = 68
    angle_robot_debris_2 = 69
    angle_robot_debris_3 = 70
    angle_robot_debris_4 = 71
    angle_robot_debris_5 = 72
    angle_robot_debris_6 = 73

    debris_in_front = 74

    robot_facing_debris = 75

    angle_to_dropzone = 76

    debris_to_dropzone_1 = 77
    debris_to_dropzone_2 = 78
    debris_to_dropzone_3 = 79
    debris_to_dropzone_4 = 80
    debris_to_dropzone_5 = 81
    debris_to_dropzone_6 = 82

    nxt_angle_to_debris_1 = 83
    nxt_angle_to_debris_2 = 84
    nxt_angle_to_debris_3 = 85
    nxt_angle_to_debris_4 = 86
    nxt_angle_to_debris_5 = 87
    nxt_angle_to_debris_6 = 88