# Contributing

## Testing changes

At this time there is only a manual test to exercise most of the functionality of the library.

### Testing Tips

If you don't have a known good BLE device that's easy to use for testing, consider installing the LightBlue app on mobile app

* [iOS](https://apps.apple.com/us/app/lightblue/id557428110)
* [Android](https://play.google.com/store/apps/details?id=com.punchthrough.lightblueexplorer&hl=en_US&gl=US)

You can create a virtual device like a Polar HR Sensor in the app. Then use the manual test to connect to the virtual device and read GATT characteristics.

Also see the [README's Tips](../README.md#tips).

## Overview

Linux.Bluetooth is an open-source project. We encourage community members like yourself to contribute.

You can contribute today by creating a **feature request**, **issue**, or **discussion** on the forum. From there we can have a brief discussion as to where this fits into the backlog priority. If this is something that fits within the architecture, we'll kindly ask you to create a **Pull Request**. Any PR made without first having an issue/discussion may be closed.

Issues posted without a description may be closed immediately. Use the discussion boards if you have a question, not Issues.

We will close your _PR_ if it doesn't have an approved issue / feature request.

We reserve the right to close your "_issue_" or feature request if:

* It's an inquiry, not an issue.
* Error in your code for not following the documentation
* Not providing a description and steps to reproduce
* Not providing a sample when asked to do so

## "Keep It Simple Sally"

There have been requests in the past where individuals wanted changes for the sake of personal ease rather than how it would affect the ecosystem compatibility. This project should maintain compatibility with Linux distros supported by .NET, be careful of _one-offs_.

## Branching Strategy

Below is a basic branching hierarchy and strategy.

| Branch | Purpose
|-|-|
| `master`    | All releases are tagged published using the `master` branch
| `develop`   | The **default** & active development branch. When a feature set is completed and ready for public release, the `develop` branch will be merged into `master` and a new NuGet package will be published.
| `feature/*` | New feature branch. Once completed, it is merged into `develop` and the branch must be deleted.
| `stable/*`  | Stable release base build which shares cherry-picked merges from `develop`. This branch **must not** be deleted.

## Regards

> Thank you,
>
> Damian Suess<br />
> Xeno Innovations, Inc. / Suess Labs
