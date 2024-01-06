# AlwaysHearActiveWalkies

Repo for the AlwaysHearActiveWalkies lethal company mod.

## Thunderstore
https://thunderstore.io/c/lethal-company/p/Suskitech/AlwaysHearActiveWalkies/

## Configuration
Configuration leverages [Bepinex Configuration API](https://docs.bepinex.dev/articles/user_guide/configuration.html). Either load up the mod once to generate a config file or manually add it to:

`{INSTALL_DIRECTORY}/Lethal Company/BepInEx/suskitech.LCAlwaysHearActiveWalkie.cfg`

Example configuration file template:
```
## Settings file was created by plugin LC Always Hear Active Walkies v1.4.4
## Plugin GUID: suskitech.LCAlwaysHearActiveWalkie

[General]

# Setting type: Single
# Default value: 12
AudibleDistance = 12

# Setting type: Single
# Default value: 20
WalkieRecordingRange = 20

# Setting type: Single
# Default value: 20
PlayerToPlayerSpatialHearingRange = 20
```

#### Configurable Variables
- AudibleDistance
  - Default: `12`
  - The max distance at which a walkie talkie can be heard 
- WalkieRecordingRange
  - Default `20`
  - The max distance from which a walkie talkie can "hear" players.
- PlayerToPlayerSpatialHearingRange
  - Default `20`
  - Max distance at which a player can hear other players.


## Contribution
I don't have a lot of time to dedicate to this mod but it has gained enough traction that I'll try to keep it updated and working for as log as its needed. Ideally the game will add this in as a native feature soon and it will be obviated.

Bug reports and PRs for improvements are welcome but due to the difficulty of testing this mod I will probably be a bit slow to review and integrate them.

## Contributors
Many thanks to these contributors!

[@etrant](https://github.com/etrant)
