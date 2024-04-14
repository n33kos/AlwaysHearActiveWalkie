# 1.4.5
- Fixed bug causing walkie talkies to transmit when button is not held
- Increased spatial audio sound max when using active walkies but standing next to other players
- Fixed bug causing nearby players to use spatial audio instead of walkie talkie effect when near another active walkie.

# 1.4.4
- Made distance values configurable
- Lowered the max distance at which a walkie talkie can be heard 

# 1.4.3
- Adjust player sound effects based on. louder of two audio "sources"
  - This should result in only applying the walkie talkie sound effect to a player's voice if the walkie talkie sound would be louder than the spatial audio sound of the player.
  - Fixes known issue in version 1.4.2
- Lower `throttleInterval` to 0.35.
  - This should result in a slightly faster response time adjusting other players' volume dynamically.
- Update Icon
- Automatically start app in .bat development script

# 1.4.2
- Add gihub repo
- Update README
- ### Known Issues:
  - When talking into a walkie nearby other players, It sounds like the speaker is talking through the walkie even if a nearby player has no walkie talkie active.

# 1.4.1
- Increase audible walkie distance upper range to 20f

# 1.4.0
- Reduce performance impact by throttling calculations on update loop
- Fix bug causing audio range to cut off abruptly. It now should get quieter by distance from active walkie.

# 1.3.0
- Fix bug where dropping a walkie while someone is communicating cuts out audio
- ### Known Issues:
  - Exiting and entering the walkie range abruptly cuts out instead of lowering volume gradually

# 1.2.0
- Fix Walking in and out of walkie range during communication
- ### Known Issues:
  - Dropping a walkie talkie while another is in the middle of a communication cuts off audio. It resumes if they start a new communication.

# 1.1.0
- Fix misunderstanding preventing functionality
- ### Known Issues:
  - Walking in and out of range while communication is in progress doesn't work

# 1.0.0
- Only listen to powered-on walkies
- Limit distance at which users can hear walkies to 15
- ### Known Issues:
  - Build fully broken

# 0.1.0
- Force all users able to hear walkies at all times