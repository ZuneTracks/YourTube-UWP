# Changelog

## v1.0.8.0 - Rotating live tile release

- Added a foreground live-tile queue that rotates the last video selected for
  playback, the latest **Trending Now** result, and the branded YourTube tile.
- Added local persistence for the last-played and trending tile metadata, so the
  queue can be rebuilt after subsequent public-data or playback actions.
- Preserved the existing Trending Now behavior as the fallback: when no
  last-played video is available, the queue starts with the latest trending result.
- Stored only non-secret public metadata for tile use (video ID, title, channel,
  thumbnail URI, and trending region). API keys, OAuth tokens, and media URLs are
  not persisted or displayed by the tile.
- Updated the ARM Windows 10 Mobile AppX package version to `1.0.8.0`.

## v1.0.7.4 - First public release

- Added a Windows 10 Mobile 15063+ ARM UWP app with a Metro-inspired YourTube UI.
- Added public YouTube Data API v3 search, regional trending, details, channels,
  categories, and inline category expansion.
- Added an official YouTube mobile watch page in-app viewing route with a browser
  fallback and no direct media extraction.
- Added foreground Trending Now live-tile updates.
- Added OAuth 2.0 authorization code plus PKCE architecture with system-browser
  sign-in and Credential Locker storage.
- Removed retained legacy credentials from the recovered source and excluded the
  recovered WP8 projects from the public release repository.

## Security and distribution notes

- No API key, OAuth client ID, OAuth client secret, access token, refresh token, or
  private signing certificate is included.
- The included AppX is for Developer Mode sideloading and is signed with a temporary
  development certificate. It is not a production or Microsoft Store package.
