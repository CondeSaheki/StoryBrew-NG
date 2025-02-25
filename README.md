[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

# StoryBrew-NG

**StoryBrew-NG** (Next Generation) is a complete refactor of the beloved StoryBrew project, rebuilt from the ground up for **improved performance** and **cross-platform** compatibility (including native Linux support). A key feature of this refactor is its enhanced **flexibility**, achieved by leveraging the standard .NET build process for compiling storyboards.

The new GUI desktop application, built on **osu!Framework**, empowers creators to efficiently manage osu! storyboards paired with enhanced libraries for script creation and development.

**Documentation & Tutorials**: Explore the [StoryBrew Wiki] (placeholder) for detailed guides. Until StoryBrew-NG is ready and has introduced all new features, much of the original documentation remains relevant.

## Table of Contents
- [Overview](#overview)
- [Usage](#usage)
  - [Project Creation](#creation)
  - [Workflow](#workflow)
- [Project Structure](#project-structure)
- [Dependencies](#dependencies)
- [Building from Source](#build)
- [Contributing](#contributing)
- [License](#license)

## Overview

StoryBrew-NG modernizes the original StoryBrew with:
- **Cross-platform support** making it compatible with Linux.
- **Modular architecture** for easier maintenance and extensibility
- **Improved performance** through optimized rendering and code generation
- **Hybrid workflow** combining GUI editing and CLI scripting

## Usage

### Creation

#### Option 1: Using the Editor

1. Launch the **StoryBrew** application.
2. Use the **New Project** wizard to generate a template project automatically.

#### Option 2: Command Line (dotnet template)

1. Install the template package:
   ```bash
   dotnet new --install <storybrew_template>
   ```
2. Create a new project:
   ```bash
   dotnet new <storybrew_template> -n MyStoryboardProject
   ```

Once created, customize your storyboard scripts using your preferred IDE (e.g., Visual Studio, Rider, or VS Code).

### Workflow

#### Editor Workflow

1. **Update Project**: Click the [placeholder] button in the editor to:
   - Compile your code (`dotnet build`)
   - Establish a real-time pipe connection with the editor for visualization (`dotnet run -- pipe`)
2. **Visualize & Edit**: Interact with the timeline, adjust parameters visually, and see changes immediately.

#### CLI Workflow

1. Build your project:
   ```bash
   dotnet build -c Release
   ```
2. View command options:
   ```bash
   dotnet run -- help
   ```

Iterate by editing scripts and rebuilding until you achieve your desired storyboard.

## Project Structure

- **StoryBrew**: A foundational library for storyboard projects. ~~Distributed as a NuGet package.~~
- **StoryBrew.Generator**: A Source Generator that makes the entrypoint for storyboard projects, this new component is automatically used when you compile your project ~~Distributed as a NuGet package.~~
- **StoryBrew.Desktop**: A desktop application providing a GUI for storyboard visualization and development. 
- **StoryBrew.Game**: Core library handling the GUI components.
- **StoryBrew.Game.Tests**: Testing suite for the `StoryBrew.Game` library.
- **StoryBrew.Resources**: Resource library used by `StoryBrew.Game` library.

The modular design ensures that the internal workings remain streamlined while offering robust functionality for storyboard development.

## Dependencies

This project no longer relies on **BrewLib**, **Damnae.Tiny** or **System.Drawing.Common** allowing more flexibility and simpler maintenance, currently it depends on the following packages:

- **ManagedBass:** `3.1.1` (API's using this are marked obsolete)
- **Newtonsoft.Json:** `13.0.3`
- **Newtonsoft.Json.Bson:** `1.0.3`
- **Newtonsoft.Json.Schema:** `4.0.1`
- **OpenTK.Mathematics:** `4.9.3`
- **SkiaSharp:** `3.116.1`
- **ppy.osu.Framework:** `2025.220.1`
- **ppy.osu.Game:** `2025.221.0`
- **Microsoft.NET.Test.Sdk:** `17.13.0`
- **NUnit3TestAdapter:** `5.0.0`
- **Microsoft.CodeAnalysis:** `4.12.0`
- **Microsoft.CodeAnalysis.CSharp:** `4.12.0`

## Build

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Git

### Steps

1. Clone the repository:
   ```bash
   git clone <url>
   cd StoryBrew-NG
   ```
2. Restore dependencies:
   ```bash
   dotnet restore
   ```
3. Build the solution:
   ```bash
   dotnet build -c Release
   ```

## Contributing

We welcome contributions to this project. Feel free to submit issues, suggest features, or create pull requests to improve the tool.

## License

Distributed under the MIT License. See [LICENSE](LICENSE) for details.