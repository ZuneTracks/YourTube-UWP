---
title: UniTube UWP Privacy Policy
---

# UniTube UWP Privacy Policy

**Effective date: August 28, 2026**

This Privacy Policy explains how the UniTube UWP application ("UniTube" or the
"App") handles information when you use it.

## Overview

UniTube is an independent, open-source UWP client. The project does not operate
an account system, its own backend service, advertising service, or analytics
service. The App does not sell personal information.

The App connects directly to Google and YouTube services that you choose to use.
Those services process information under their own policies, including the
[Google Privacy Policy](https://policies.google.com/privacy).

## Information the App handles

### YouTube and Google service data

When you search, browse, view trending content, or open a video, the App sends
the information needed to make that request, such as your search terms, selected
region, or video identifier, directly to Google or YouTube. The App can receive
public video and channel metadata, including titles, channel names, thumbnails,
descriptions, statistics, and video identifiers.

If you choose to authorize a Google account, the App requests the
`youtube.readonly` and `youtube.upload` scopes. Through those scopes, the App
can access the account data that the YouTube Data API makes available for the
selected account, such as the channel summary, uploads, subscriptions, playlists,
playlist videos, and liked videos. The App uses that information to provide the
Profile feature.

### Credentials and authorization tokens

If you enter a YouTube Data API key, OAuth client ID, or OAuth client secret, the
App stores the API key and client secret in Windows Credential Locker. The client
ID is stored in the App's local settings. Following Google authorization, access
tokens, refresh tokens, and token expiry metadata are stored in Windows
Credential Locker.

The App transmits credentials and tokens only to Google endpoints as necessary to
make YouTube Data API requests, complete authorization, refresh authorization, or
upload a video. The project maintainers do not receive these credentials or
tokens. Selecting **Clear API key** removes the saved API key; selecting
**Sign out** removes the stored Google authorization tokens.

### Uploads

When you choose a video file for upload, the App reads that file only to perform
the upload you initiate. It sends the file, its title, optional description, and
selected privacy setting directly to YouTube using the YouTube Data API. The App
does not retain a copy of the selected video file after the upload attempt.

### Local app data

To maintain the live tile, the App stores the latest selected and trending public
video metadata locally: video ID, title, channel name, thumbnail URL, and
selected trending region. It does not store media URLs for this feature.

The App also keeps a local diagnostic log of recent app events. The log is
limited in size and may contain event timestamps, feature names, error types,
error codes, and Google error identifiers. It is designed to redact API keys,
OAuth client credentials, device codes, access tokens, and refresh tokens. You
can review, clear, or explicitly export the diagnostic log from the Diagnostics
page. An exported log is saved only to the location you select.

## How information is used

The App uses the information described above solely to provide its features:
searching and browsing YouTube content, displaying a live tile, authorizing a
Google account, showing permitted account data, uploading a selected video, and
diagnosing app failures. The App does not use this information for advertising,
cross-app tracking, or sale.

## Third-party services

Google and YouTube receive requests made through the App, including the network
information that normally accompanies an internet connection, such as your IP
address. When the App opens an official YouTube watch page, that page is governed
by YouTube's policies and may use its own cookies or similar technologies.

Your use of Google and YouTube services is also subject to their applicable terms
and policies. You can review and revoke a connected App's Google Account access
through your Google Account settings.

## Your choices

You can use public browsing features without authorizing a Google account. You
may choose not to enter an API key or OAuth credentials, though features that
need them will be unavailable. You can remove stored credentials using the
Settings page, clear diagnostic logs from the Diagnostics page, and remove local
App data by uninstalling the App.

## Data security and retention

Credential data is stored using Windows Credential Locker. Other local data is
stored in the App's local settings until you clear it, uninstall the App, or it
is replaced by newer live-tile or diagnostic data. Although the App uses
platform-provided storage and HTTPS connections to Google and YouTube, no method
of storage or transmission is completely secure.

## Children's privacy

The App is not directed to children. If you use YouTube through the App, you
must meet the age and other eligibility requirements that Google and YouTube
apply to their services.

## Changes to this policy

The project maintainers may update this policy by publishing a revised version at
this page. The effective date identifies when the policy was last updated.

## Contact

For questions about this policy, open an issue in the
[UniTube UWP GitHub repository](https://github.com/ZuneTracks/YourTube-UWP/issues).
