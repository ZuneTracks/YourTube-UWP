# Changelog

## v1.5.0.0 - Video upload prototype

- Replaced the upload prototype's custom-redirect PKCE sign-in with Google's OAuth
  2.0 device authorization flow for TVs and limited-input devices.
- Removed the redirect URI and protocol-handler setup. The app now asks only for a
  limited-input device client ID and secret, shows Google's verification URL and
  user code, and waits for authorization on another device.
- Kept access and refresh tokens in Windows Credential Locker, preserved foreground
  cancellation, and keeps the user-supplied client secret in Credential Locker.
- Added a persistent, secret-safe diagnostics page for application exceptions and
  OAuth/Credential Locker milestones. The log remains available after a restart
  and can be saved as a text file.
- Restored the Settings **About** flyout from the recovered 1.0.9.0 source.
- Retained the existing pinned live-tile refresh behavior.

## v1.0.9.0 - Store packaging preparation and navigation fix

- Added an explicit ARM Store-upload configuration that creates an unsigned
  `.appxupload` candidate with public symbols.
- Added a build-time guard requiring Visual Studio Store association metadata
  before Store packaging.
- Updated the package version to use a Store-compatible fourth version field of
  `0`.
- Preserved the Search pivot and its prior results when navigating Search ->
  video -> Back by caching `MainPage`; Home and Categories video navigation
  behavior is unchanged.

## v1.0.8.4 - Live tile refresh fix

- Fixed the live-tile regression where a previously pinned app tile would not
  reflect the most recently played video after returning to the Start screen.
- Kept the rotating live-tile behavior but added unique tile update tags so the
  last-played and trending metadata refresh cleanly without being stuck on stale
  content.
- Preserved the fallback sequence: last played, then Trending Now, then the
  branded YourTube tile.
- Updated the ARM Windows 10 Mobile AppX package version to `1.0.8.4`.

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
