# s&box Notepad Widget

A simple in-editor notepad widget for s&box. Allows creating, editing, and saving notes in the editor.

## Features
- Create new notes
- Edit existing notes
- Save and load notes

## Installation
1. Copy the `NotepadWidget.cs` file into your `Editor` folder in s&box project.
2. Compile the project.
3. The widget will be available under `View` tab in the editor.

## TODO
- [ ] Notes ListView Sorting (currently alphabetically)
- [ ] Refactor widgets arrangement (some might clip)

## Disclaimer
**There may be a problem with loading large text files (I tested a 100 MB text file, it loads but it takes a while). I don't know how to speed it up or at least prevent the main thread from freezing, because the problem seems to lie in rendering the text rather than loading it.**

## License
This project is released into the public domain under the Unlicense. You are free to use, modify, and distribute it for any purpose without restriction.
