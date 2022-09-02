# Versioning

Katabasis uses [calendar versioning](https://calver.org) and [semantic versioning](https://semver.org) (or combination thereof) where appropriate. For example, the version scheme used for some libraries is `YYYY.MM.DD` and for others its `MAJOR.MINOR.PATCH-TAG`.

## Semantic Versioning

Katabasis uses [GitVersion](https://github.com/GitTools/GitVersion) to determine the exact semantic version for each build with [GitHub actions](https://docs.github.com/en/free-pro-team@latest/actions/guides/about-continuous-integration) (automated workflows). 

## Releases

Git tags are releases; when a new tag is created, the version is automatically bumped automatically to the specified tag version.
For a complete list of the release versions, see the [tags on this repository](https://github.com/craftworkgames/Katabasis/tags).

## Pre-Releases

Rolling builds are the *next minor* version with a prefix of the build. E.g., if the current version is `0.1`, the next version would be `0.2`. So, the version of a pre-release would look something like `0.2-pre0158`.