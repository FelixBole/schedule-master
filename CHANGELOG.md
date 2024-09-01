# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.7.0] - 2024-09-01
### Feat
- Added possibility to use a more stable time configuration for the game, that doesn't reset it's configuration after play mode is exited
- Made it possible to select whether to use a provided "Game" time configuration or use a dev config that doesn't change
- Added possibility to accelerate / slow time during runtime with a custom inspector on the TimeManager
- Added a "Save" method to the TimeManager to register the current runtime configuration progress in the used TimeConfigurationSO (only when not in testing mode to avoid overriding the testing config scriptable objects). This method also returns the updated TimeConfigurationSO so it can be plugged into any Save / Load system.
- Added elements to demo scene for clearer understanding of how to use the external TimeConfiguration vs the Testing one in the TimeManager.
### Fix
- Fixed an issue in which events with 0 checks were not triggered

## [0.6.0] - 2024-09-01
### Feat
- Extended checkers associations in order to select if it should run an AND or OR logic on the checkers associated to the event
- Added easy toggle of Logic AND | OR choice in the custom editor
- For simplicity, the save button is now always present in the custom editor when managing checkers for an event

## [0.5.0] - 2024-09-01
### Feat
- Extended the ScheduleEventsSO to enable plugging in any custom solution for loading and getting events by disabling a boolean toggle to stop using the internal solution.
- Added helper methods to ScheduleEventsSO
#### Event Creation
- Added creation prevention when end date is before start date
#### Search by timestamp
- Added option to search using only the day of the week, displaying all events available on that day regardless of date / hour / year / season...
- Added option to search using only the date
- Added option to search using only the hour
- Added option to search using only the Year
- Added option to search using only the Season
- Added combined search to enable searching with (for example) only Day & Season or only Hour & Year, any number of combinations
- Added search between 2 timestamps
#### Sample content
- Added example script to setup and fire the TimeManager in the demo scene
### Fix
- Fixed a display issue in which the eye icon functionality was reverted
### Style
- Enhanced the style of the custom editors (a little)

## [0.4.3] - 2023-08-18
### Feat
- Added current day ratio calculation

## [0.4.2] - 2023-08-15
### Feat
- Added a public getter for the DateTime Day Configuration

## [0.4.1] - 2023-08-15
### Feat
- Added Play/Pause/SetNewDay methods to TimeManager

## [0.4.0] - 2023-08-15
### Feat
- Configured the package for extendability
- Made private fields and methods protected (+virtual)

## [0.3.0] - 2023-08-13
### Feat
- Added Customizable Event Check system
- Added new Custom editor windows
- Added TimeManager events on key timestamps
