# Smalledit - Docs

This is the documentation of Smalledit, a small terminal text editor written
in C#.

This project was created as part of the NPRG035 Programming in C# at the
Faculty of Mathematics and Physics at Charles University.

## Features

- **File Management**: Create, open, save, and save-as operations with unsaved changes protection
- **Text Editing**: Cut, copy, paste, select all, with real-time status bar showing position and statistics
- **Search Functionality**: Case-insensitive text search with highlighting and find next capabilities
- **View Options**: Toggle line numbers and word wrapping for improved readability
- **Terminal Integration**: Menu-driven interface with keyboard shortcuts and command-line file opening

## Architecture

Smalledit follows a modular object-oriented architecture with clear separation of concerns:

- **TextEditorWindow**: Main window class that coordinates all components and handles UI events
- **FileManager**: Manages file operations, tracks file state, and handles unsaved changes
- **SearchManager**: Handles text search functionality, highlighting, and cursor positioning
- **BottomStatusBar**: Displays real-time editor status and statistics
- **FindDialog**: Modal dialog for text search input
- **QuitDialog**: Modal dialog for handling unsaved changes on quit
- **Utils**: Static utility classes containing layout constants, text processing, and helper functions

## Dependencies

- **.NET 8.0**: Target framework for the application
- **Terminal.Gui v1.x**: Cross-platform terminal UI toolkit providing widgets, dialogs, and event handling
