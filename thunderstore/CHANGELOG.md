# 1.4.2
- Add gihub repo
- Update README

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