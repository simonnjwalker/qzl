## [1.0.8] - 2024-08-20
### Added
- Vertical layout parameter (for console only) -la|--layout vertical

## [1.0.7] - 2024-10-19
### Fixed
- Changed -v options to be None, Minimum, Low, Default, High, Full
https://github.com/simonnjwalker/qzl/issues/3
- MS Access connection-string now uses full path to the file
https://github.com/simonnjwalker/qzl/issues/2

### Removed
- The --type parameter is removed (folded into -c and -f)
https://github.com/simonnjwalker/qzl/issues/1

## [1.0.6] - 2024-08-29
### Added
- Best guess from few parameters (heuristics) 
- Suppress guessing with -nh|noheuristic parameter

## [1.0.5] - 2024-08-17
### Added
- MS Access support
- Batch queries from .SQL files

### Fixed
- No error messages from data providers

## [1.0.4] - 2024-08-08
### Fixed
- Typo with qzl sql --formet 

### Removed
- Postgres and Oracle support is not implemented

## [1.0.3] - 2024-03-11
### Fixed
- Output to a scalar (-m Scalar) now saves single value to a text file

### Added
- Additional script commands for testing

## [1.0.2] - 2024-03-10
### Fixed
- No more errors when loading tables from Excel with no columns

## [1.0.1] - 2024-03-10
### Fixed
- Install script commands

### Added
- Create empty database works for Excel and SQLite

## [1.0.0] - 2024-03-10
### Added
- Initial release