---
name: Bug report
about: Report a problem in Clip Chopper
title: "fix(<scope>): <what is wrong>"
labels: "type: Bug"
---

<!-- Title: use the Conventional Commits form, for example `fix(player): the audio track list is empty`. Scopes: player, save, compression, advanced-save, naming, settings, cli, installer, core. -->

**Summary:** <!-- One sentence: what is wrong. -->

## What happens

<!-- What you see. Add a screenshot if it helps. -->

## Steps to reproduce

1. <!-- For example: Open a folder with an MKV file. -->
2.
3.

## Expected result

<!-- What you expected to see instead. -->

## Environment

- Clip Chopper version: <!-- Windows 11: Settings > Apps > Installed apps. Windows 10: Settings > Apps > Apps & features. Look for ClipChopper in the list. The installer file name also has it, for example ClipChopper-2.0.0.exe. -->
- Windows version: <!-- Run `winver`. For example: Windows 11 24H2, build 26100. -->
- Video format: <!-- Container, video codec, audio codec, resolution, frame rate. For example: MP4, H.264, AAC, 1080p, 60 fps. -->

## FFmpeg output or log

<!-- If a save fails, the app can show an error with a "View Log" section. Open it, copy all the text, and paste it into the block below. If you see no error window or no View Log section, write what you saw. -->

```text
```

## Area

<!-- Keep one or two areas and delete the rest. The maintainer sets the `area:` labels, the priority and the milestone. -->

- `area: Player`
- `area: Compression`
- `area: Save`
- `area: Advanced Save`
- `area: Core Library`
- `area: Infrastructure`
- `area: Tests`
- `area: CLI`
- `area: Documentation`

## Acceptance criteria

- [ ] <!-- The fixed behavior: one result that a person or a test can check. -->
- [ ] No regression in the existing test suite

## Surfaced by

<!-- Where you met the problem, in one sentence. -->
