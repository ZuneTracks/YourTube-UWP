# YourTube UWP

`YouTube.Uwp.sln` contains the YourTube UWP app for Windows 10 Mobile Creators Update
(10.0.15063.0) and later. The recovered WP8 projects remain in their original
folders and are not referenced by the UWP project. Only the existing image files
are linked as package assets; no recovered application code, token handling, stream
handling, or binary libraries are reused.

## Current release: v1.0.9.0

v1.0.9.0 is the current YourTube UWP release. It includes the ARM Windows 10
Mobile Developer Mode sideload package and preserves the Search pivot/results
when returning with Back after opening or playing a video. This navigation
state is retained through `MainPage` navigation caching; Home and Categories
video navigation remain unchanged.

The original [`v1.0.8.4`](https://github.com/ZuneTracks/YourTube-UWP/releases/tag/v1.0.8.4)
release included an ARM Windows 10 Mobile
Developer Mode sideload package for devices running 10.0.15063.0 or later. The
release assets include the AppX, its public development certificate, the two
required ARM framework packages, and installation instructions.

This release fixes the pinned live-tile refresh regression so the Start tile updates
with the latest video metadata after playback and returns to the home screen. See
[CHANGELOG.md](CHANGELOG.md) for the complete release history.

This is not a Microsoft Store or production-signed distribution: the package uses
the documented `YourTubeDevelopment` temporary development certificate. Do not
redistribute a private PFX file. Before publishing a production distribution,
replace the publisher identity and development certificate with your own production
signing configuration.

## Build prerequisites

Install Visual Studio with the **Universal Windows Platform development** workload
and the Windows 10 SDK version **10.0.15063.0**. Open `YouTube.Uwp.sln`, select
`Debug | ARM`, then build and deploy to a physical Windows 10 Mobile 15063+ device.
ARM is the default project and package architecture because Windows 10 Mobile
devices are ARM-based. `Debug | Any CPU` remains available for architecture-neutral
development or emulator workflows, but use the ARM configuration for device
packages. The package manifest intentionally contains a placeholder publisher and
debug packaging uses the development publisher identity. Replace the publisher identity
and configure a production signing certificate before distribution.

The project deliberately uses the classic UWP project system instead of a current
Windows App SDK dependency, so every app API used is available on 15063.

## Mobile UI

The UWP UI intentionally echoes the recovered Windows Phone application without
reusing its unsupported Silverlight controls: a dark header, red accent strip,
pivot-style **Home**, **Search**, and **Categories** sections, an active-pivot
underline, large thumbnail cards, and compact metadata/detail pages. The header's
**Settings** button opens the separate credentials page. It uses only UWP XAML
controls supported by Windows 10 Mobile 15063.

Selecting a category expands it inline and loads up to 25 current popular videos
under that header. Selecting it again collapses the section; expanding another
category collapses the prior section while retaining its loaded results.

The package artwork is an original flat two-color Metro identity: a coral `Y`
monogram and lightweight wordmark on the app's charcoal mobile palette. It
deliberately does not reuse the recovered YouTube logo, badge, or wordmark
styling.

## Google Cloud configuration

1. Create a Google Cloud project and enable **YouTube Data API v3**.
2. Create an API key, restrict it to YouTube Data API v3, and apply the narrowest
   viable quota and application restrictions. Enter it at runtime in the app. It is
   stored in Windows Credential Locker, is sent only as the `key` parameter on
   public read-only v3 requests, and is never used for account authorization.
3. Create a **TVs and limited-input devices** OAuth client. Do not create or ship a
   client secret, and do not configure a redirect URI. Configure the generated
   client ID at runtime in the app. Changing that client ID clears the existing
   authorization, so complete sign-in again.
4. Ensure the OAuth consent screen, test users, scopes, and any verification
   requirements are completed in Google Cloud before production use.

No API key, OAuth client ID, OAuth client secret, developer key, analytics key, or
token is included in the UWP project. User-entered API keys and OAuth tokens are
kept separately in Credential Locker. The original recovered credential literals
have been replaced with explicit removal markers and must be treated as compromised:
revoke or rotate them in their original provider consoles.

The app is packaged as **YourTube**. Its package identity, development publisher,
and Credential Locker resource names are separate from earlier
`YouTubeReconstructed` development packages, so re-enter runtime configuration
after installing YourTube.

## Implemented public API v3 mappings

| App function | Official YouTube Data API v3 request |
| --- | --- |
| Search public videos | `GET https://www.googleapis.com/youtube/v3/search?part=snippet&type=video&q=...` |
| Trending / popular by region | `GET https://www.googleapis.com/youtube/v3/videos?part=snippet,contentDetails,statistics,status&chart=mostPopular&regionCode=...` |
| Category choices by region | `GET https://www.googleapis.com/youtube/v3/videoCategories?part=snippet&regionCode=...` |
| Popular videos in a selected category | `GET https://www.googleapis.com/youtube/v3/videos?part=snippet,contentDetails,statistics,status&chart=mostPopular&videoCategoryId=...&regionCode=...` |
| Video details | `GET https://www.googleapis.com/youtube/v3/videos?part=snippet,contentDetails,statistics,status&id=...` |
| Channel details | `GET https://www.googleapis.com/youtube/v3/channels?part=snippet,contentDetails,statistics&id=...` |

The public client maps results into platform-independent `VideoSummary`,
`VideoDetails`, and `ChannelDetails` DTOs. This replaces the recovered model types
that inherit WP8 `ResultItem`, dispatch through WP8 view-model singletons, and
contain platform-specific commands and transfer objects.

### Trending live tile

After a successful **Trending Now** request, YourTube persists the leading public
result and refreshes a UWP live-tile queue. After a user starts playback, that
queue rotates the last-played video, the latest Trending Now result, and a branded
YourTube app-icon tile. When there is no last-played metadata, the tile falls back
to the latest Trending Now result and the branded tile. Only public video metadata
(ID, title, channel, and thumbnail URI) is stored in local app settings; API keys,
OAuth tokens, and media URLs are never included.

The tile does not fetch or refresh data in the background: automatic polling would
need a separate UWP background-execution design and must not rely on a stored user
API key or on the retired WP8 scheduled agent.

## Account authorization architecture

`OAuthDeviceAuthorizationService` uses Google OAuth 2.0 device authorization with
the `https://www.googleapis.com/auth/youtube.upload` scope:

1. It obtains a short-lived device code and displays Google's verification URI and
   user code. The user completes authorization in a browser on another device; the
   app does not embed a sign-in page or receive a redirect.
2. It polls `https://oauth2.googleapis.com/token` at Google's requested interval
   until authorization succeeds, expires, or is canceled. A client secret is
   neither requested nor supported.
3. Access and refresh tokens are kept in Credential Locker. `GetValidAccessTokenAsync`
   refreshes an expiring access token without adding an API key.

The **Upload** page uses a brokered UWP file picker, sends `snippet` and `status`
metadata to `videos.insert`, and uploads the selected file in 256 KiB resumable
chunks. It reports determinate byte progress and permits cancellation between
chunks. The uploader requires completed OAuth authorization and never uses the
public API key. Other account features must explicitly request the minimum OAuth
scope and use a bearer access token. Examples include:

### Uploading a test video

1. In **Settings**, save a user-created **TVs and limited-input devices** OAuth
   client ID, use **Sign in**, and enter the displayed Google code at the displayed
   verification URL from another phone, tablet, or computer with a current browser.
   The Windows 10 Mobile browser is not used for this step.
2. Open **Upload**, select a local `.mp4`, `.wmv`, `.mov`, `.avi`, or `.mkv` file,
   enter a title, optional description, and privacy level, then select **Start
   upload**.
3. Select **Cancel** to stop the foreground transfer. A canceled session is not
   persisted or resumed after leaving the page.

| Feature | v3 resource | OAuth required |
| --- | --- | --- |
| Subscribe / unsubscribe | `subscriptions.insert` / `subscriptions.delete` | Yes |
| Manage playlists or playlist items | `playlists.*` / `playlistItems.*` | Yes |
| Rate a video | `videos.rate` | Yes |
| Post comments | `commentThreads.insert` / `comments.*` | Yes |
| Upload video | resumable `videos.insert` | Yes |

## Deliberate migration boundaries

The legacy app called retired GData v2 Atom/XML feeds, exchanged tokens with a
client secret, used a WP scheduled agent/background transfer APIs, and scraped
watch-page responses (`get_video_info`, `fmt_stream_map`, and signed media URLs).
Those paths are not carried forward.

* **Direct media URLs and downloads are removed.** Data API v3 provides metadata,
  not media streams. The details page opens YouTube's official HTTPS mobile watch
  page in a UWP `WebView`, avoiding iframe-player configuration requirements on
  Windows 10 Mobile. The normal YouTube watch URI remains the browser fallback. No
  playback URL is extracted.
* **Watch History and Watch Later have no equivalent public v3 collection API.**
  They require a product redesign (for example, app-local state or a user-created
  playlist where allowed); they are not silently emulated.
* **Favorites is not migrated as a global legacy feed.** Use a supported playlist
  workflow only after an explicit OAuth implementation.
* **WP8 scheduled agents, Silverlight pages, BackgroundTransferService, Phone tasks,
  and unsupported player libraries are not referenced.** The UWP live tile is
  updated only after a foreground Trending Now request; any future background work
  must be redesigned around UWP background-execution limits and UWP APIs available
  in the selected target version.

## Recovered-source audit

The retained WP8 source is intentionally excluded from `YouTube.Uwp.sln`. The audit
found retired GData v2 usage in `YouTube.ViewModel` request/model classes (including
search, videos, channels, comments, playlists, subscriptions, ratings, uploads,
watch history, watch later, and favorites); old OAuth/token exchange in
`YouTube.ViewModel.Helpers\AuthenticationHelper.cs`; direct-media extraction in
`YouTube.ViewModel.Helpers\YoutubeHelper.cs`; and WP platform/background code in
`YouTube.UI` and `YouTube.TaskAgent`.

Credential-bearing constants or request values were removed from the legacy
constants, authentication, comments, upload, view-model, and task-agent paths.
Their removal markers document why that recovered code must not be built, shipped,
or used as a credential source.
