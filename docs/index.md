---
title: YourTube UWP
---

# YourTube UWP

YourTube is an original UWP client for Windows 10 Mobile 10.0.15063 (Creators
Update) and later. It is designed for ARM devices and uses the official YouTube
Data API v3 for public discovery features.

## Latest release: v1.0.8.4

[Download the ARM Developer Mode sideload package](https://github.com/ZuneTracks/YourTube-UWP/releases/tag/v1.0.8.4)

The `v1.0.8.4` release fixes the live-tile refresh regression and keeps the
rotating live tile behavior working for the latest video metadata:

- The last video selected for playback.
- The latest public Trending Now result.
- A branded YourTube app-icon frame.

Without a previously played video, the tile starts with Trending Now. The tile
stores only public video metadata locally and does not expose API keys, OAuth
tokens, or media URLs.

## Installing

Use the `INSTALL.txt` file included with the release. The ARM AppX, its public
development certificate, and both ARM framework dependencies must be installed on
a Windows 10 Mobile Developer Mode device.

## Important notes

- This is a Developer Mode sideload package, not a Microsoft Store package.
- No private signing key, API key, OAuth client secret, or user token is published.
- Playback uses YouTube's official mobile watch page; the app does not extract or
  download media streams.

[View the full changelog](https://github.com/ZuneTracks/YourTube-UWP/blob/main/CHANGELOG.md)
