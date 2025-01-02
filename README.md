[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

# StoryBrew-NG

## Overview
**StoryBrew-NG** (Next Generation) is a complete **refactor** and remake of the renowned and loved by the community StoryBrew project. Redesigned from the ground with **cross-platform** support in mind, it empowers developers to create, compile, and manage storyboard scripts for osu! efficiently with a comprehensive suite that includes a **CLI tool**, a **GUI desktop application** made with osu!Framework and **libraries** for osu! story board script development.

For more detailed **documentation and tutorials** refer to the StoryBrew **Wiki**. While StoryBrew-NG introduces changes, much of the original documentation remains a valuable resource.

## Features
- CLI for quick project creation and compilation.
- Desktop GUI for intuitive and streamlined development.
- Comprehensive libraries for scripting and resource management.
- Examples and tests to support script and GUI development.

## Project Structure

### Executables
- **StoryBrew**: A CLI tool for compiling and creating new storyboard script projects.
- **StoryBrew.Desktop**: A desktop application providing a GUI for storyboard vizualiation and development.

### Libraries
- **StoryBrew.Common**: A foundational library used by storyboard scripts.
- **StoryBrew.Game**: Core library handling the GUI components.
- **StoryBrew.Game.Tests**: Testing suite for the `StoryBrew.Game` library.
- **StoryBrew.Resources**: Resource library used by `StoryBrew.Game`.
- **StoryBrew.Scripts**: Library containing example scripts to aid developers.

## Dependencies
This project do not relies anymore on **BrewLib** and depends on the following packages:
- **OpenTK**: `4.9.3`
- **SkiaSharp**: `3.116.1`
- **ppy.osu.Framework**: `2024.1224.0`
- **ppy.osu.Game**: `2024.1224.1`
- **Damnae.Tiny**: `1.2.0`
- **ManagedBass**: `3.1.1`
- **Microsoft.CodeAnalysis.Common**: `4.11.0`
- **Microsoft.CodeAnalysis.CSharp**: `4.11.0`
- **Microsoft.Net.Compilers.Toolset**: `4.11.0`
- **Microsoft.NET.Test.Sdk**: `17.0.0`
- **NUnit3TestAdapter**: `4.4.2`

Ensure you have these packages available and restored when building the project.

## Build
1. Clone this repository:
   ```bash
   git clone <url>
   ```
2. Navigate to the project directory:
   ```bash
   cd StoryBrew-NG
   ```
3. Build the solution:
   ```bash
   dotnet build
   ```

### CLI Usage (`StoryBrew`)
Run the CLI tool to create or compile storyboard script projects:
- Create a new project:
  ```bash
  StoryBrew new <project-name>
  ```
- Build an existing project:
  ```bash
  StoryBrew build <project-path> <mapset-path>
  ```
- Get more help with:
  ```bash
  StoryBrew help
  ```

## Contributing
We welcome contributions to these project. Feel free to submit issues, suggest features, or create pull requests to improve the tool.

## License
This project is licensed under the [MIT License](LICENSE).
