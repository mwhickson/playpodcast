# playpodcast

## Overview

Having had some success with various platforms in the pursuit of the quintessential podcast player (for me), I found the least satisfying bit was the actual startup of, and control over playback.

To that end, I started with playback, and worked backwards to the UI I want to control things.

I almost gave up and let a webview take the brunt of the UI and playback handling (Go + Wails was promising in that regard); however, I don't really want a browser-based player.

Miniaudio and LibVLC appear to be the best options re: playback, and although the temptation to just use C/C++ and be done with it was high, I'm not particularly fond of the tooling for C/C++ (e.g. Make, CMake, Meson, vcpkg, etc.).

And so, I opted for LibVLC and C# (via LibVLCSharp).

Besides, it gives me the opportunity to use Terminal.GUI, which has also been an item on my TODO checklist.

Welcome to `playpodcast`!

## Features

- cache for http calls
- downloads for offline listening
- iTunes search
- OPML import/export
- streaming playback
- subscription management
- view podcast and episode detail

## Tools

- LibVLCSharp : https://github.com/videolan/libvlcsharp
- Terminal.GUI : https://github.com/gui-cs/Terminal.Gui
- iTunes Search API : https://performance-partners.apple.com/search-api
- sqlite : https://www.sqlite.org/
- XML, RSS, OPML

## Screenshots

![screenshot (work in progress)](screenshot.png)

## License

[MIT License](LICENSE)
