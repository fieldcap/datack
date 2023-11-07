# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.16] - 2023-11-07
### Changed
- Filter out database that do not have a FULL database when trying to create a LOG or DIFF backup.

## [1.0.15] - 2022-11-30
### Changed
- Fixed PostgreSQL backups

## [1.0.14] - 2022-11-30
### Added
- PostgreSQL support
### Changed
- Upgrade to .NET 7
- Small bug fixes with nullable checks

## [1.0.13] - 2022-07-18
### Changed
- Upgrade packages, code cleanup.
- Fixed issue with jobs not cancelling when the server throws an error.

## [1.0.12] - 2022-01-31
### Changed
- Fixed bug with timeout server checking.

## [1.0.11] - 2022-01-30
### Changed
- Fixed race condition where the job would be marked as completed while it's still setting up.

## [1.0.10] - 2022-01-10
### Changed
- Fixed issue with the timeout checker timing out a job that hasn't created tasks yet.

## [1.0.9] - 2022-01-03
### Changed
- Fixed bug where the timeout checker would accidentally mark a job as complete if there are no running tasks.

## [1.0.8] - 2021-11-17
### Changed
- Fixed re-connects of agents when the agent has no tasks left to run.
- Set task timeouts to a default of 3600 seconds.
- Add delay between executing tasks to prevent overloading of messages to clients and causing disconnects.
- Fixed timeout check on server when there are no tasks left to run.

## [1.0.7] - 2021-11-08
### Changed
- Improved handling of agent re-connects with tasks that have completed but not sent an update yet

## [1.0.6] - 2021-11-07
### Changed
- Increased SignalR timeouts
- Improved handling of agent re-connects while running tasks

## [1.0.5] - 2021-11-03
### Changed
- Improved handling of agent re-connects while running tasks
- Improved handling of completed and progress messages

## [1.0.4] - 2021-11-01
### Added
- Add update button for agents

## [1.0.3] - 2021-11-01
### Added
- Add HTTPS redirection

### Changed
- Add item select on the job run button
- Some fixed to the update scripts

## [1.0.2] - 2021-10-31
### Changed
- Maintenance build

## [1.0.1] - 2021-10-31
### Changed
- Fixed timeout issues
- Fixed throttling of sending messages

## [1.0.0] - 2021-10-25
### Added
- Initial release
