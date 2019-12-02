# Implementation things
All of these are suggestions.

## Observations:
### Done:
- Velocity
- Direction

### Not done:
- Visible Debris distance from dropzone
- Visible Debris distance from robot
- Discovered Debris distance from dropzone
- Discovered Debris distance from robot
- Robot inside dropzone
- Debris in shovel

---------------------

## Features:
### Done:
- Robot driving into walls
- Robot performing 'lift-debris' with debris in shovel
- Robot getting closer to debris

### Not done:
- Robot with debris getting closer to dropzone
- Debris inside dropzone 
    - To tell it that the reward is connected to this feature

---------------------

## Rewards:
### Positive rewards
- Debris in dropzone (Implemented)
- Locate debris (Implemented)
- Debris in shovel 
- Debris in shovel and going to dropzone
- Drive towards debris (Implemented)

### Negative rewards
- Time passing without debris in shovel (Implemented without regards to shovel)
- Drive into walls (Implemented)
- Drive over debris
- Drop debris outside of dropzone
- Debris rolls out of dropzone (Implemented)
    - High enough penalty so it's not worth to put it back in and repeat
- Debris fell out of shovel and not in dropzone

## Other simulation implementation
- Reset simulation if robot is flipped

---------------------

## Other:
Run project outside of IDE
- Run 'python3.6 Agent_controller.py' in terminal
