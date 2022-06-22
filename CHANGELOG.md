# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.3] - 2022-06-22
### Changed
- Noticed that there was an issue with accessing materials when using a package. This is because the folders are immutable within a package. To resolve as a quick fix, made it so that a new material is created for each element and bond. This is very inefficient, so will be revised shortly!

## [1.1.2] - 2022-06-22
### Added
- Added .meta files.

## [1.1.1] - 2022-06-22
### Added
- Added 'CHANGELOG.md' to track changes to this project.
- Added 'package.json'.
- Added all files into the 'Editor' folder to make the 'CLEVR-Moleculue-Generator' work.
