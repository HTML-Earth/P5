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

### Not done:
- Robot with debris getting closer to dropzone
- Robot getting closer to debris
- Robot performing 'lift-debris' with debris in shovel
- Debris inside dropzone 
    - To tell it that the reward is connected to this feature

---------------------

## Rewards:
### Positive rewards
- Debris in dropzone
- Locate debris
- Debris in shovel and going to dropzone
- Debris in shovel 
- Drive towards debris

### Negative rewards
- Time passing without debris in shovel
- Drive into walls
- Drive over debris
- Drop debris outside of dropzone
- Debris rolls out of dropzone
    - High enough penalty so it's not worth to put it back in and repeat

## Other simulation implementation
- Reset simulation if robot is flipped

---------------------

## Other:
Run project outside of IDE
- Run 'python3.6 Agent_controller.py' in terminal