# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Cities: Skylines II game modification (mod) that adds Ukrainian localization. The mod loads Ukrainian translation files and integrates them with the game's localization system.

## Build Commands

```bash
# Build Debug version
dotnet build -c Debug

# Build Release version  
dotnet build -c Release

# Clean build outputs
dotnet clean
```

Note: Building requires the CSII_TOOLPATH environment variable to be set, pointing to Cities: Skylines II modding tools. The project references custom MSBuild targets from this location.

## Publishing Commands

The mod uses MSBuild publish profiles for deployment to PDX Mods platform:

```bash
# Initial mod publication
dotnet publish /p:PublishProfile=PublishNewMod

# Update existing mod version
dotnet publish /p:PublishProfile=PublishNewVersion

# Update mod configuration only
dotnet publish /p:PublishProfile=UpdatePublishedConfiguration
```

Publishing requires PDX account credentials in `Properties/pdx_account.txt` (format: email on line 1, password on line 2).

## Architecture

### Core Components

- **Mod.cs**: Main entry point implementing `IMod` interface. Handles:
  - Loading Ukrainian localization from `localization/uk-UA.loc`
  - Copying localization to game's StreamingAssets folder
  - Registering locale with Cities: Skylines II localization manager
  - Setting Ukrainian as active locale

### Localization System

The mod works by:
1. Reading binary `.loc` file containing Ukrainian translations
2. Creating a `LocaleAsset` and registering it with the game's asset database
3. Adding the locale to the game's localization manager
4. Forcing a locale reload to apply translations

Key constants:
- Locale ID: `uk-UA`
- Target game version: 1.3.*
- Current mod version: 1.0.18

### File Structure

- `/localization/uk-UA.loc`: Binary localization file with Ukrainian translations
- `/Properties/PublishConfiguration.xml`: Mod metadata (name, version, description)
- `/Properties/Previews/`: Screenshot images for mod showcase
- `/Properties/Thumbnail.png`: Mod thumbnail

## Development Notes

- The mod requires Cities: Skylines II to be installed (default Steam location expected)
- Uses extensive logging via `ILog` for debugging localization loading issues
- Implements proper cleanup in `OnDispose()` method
- Binary `.loc` files contain: system language, localized name, translation key-value pairs, and index data