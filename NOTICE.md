# NOTICE

**Dadeum (다듬)** is a modified version of **optimizerDuck**.

- Upstream: https://github.com/itsfatduck/optimizerDuck
- Upstream copyright: Copyright (C) 2025–2026 fatDuck (itsfatduck)
- Upstream license: GNU General Public License v3.0
- Forked from upstream commit `1ad3285ccbcf1acf4f1520499da30922c6c98c29` (2026-11-10, v2.29.0 line)
- Dadeum copyright: Copyright (C) 2026 Dadeum contributors
- Dadeum license: GNU General Public License v3.0 (see [LICENSE](./LICENSE))

Dadeum is not affiliated with or endorsed by the optimizerDuck project. "optimizerDuck" is the
name of the upstream project and is used here only to identify the origin of the code.

## Changes from upstream

The full list of changes is the Git history of this repository (`git log upstream/master..`).
A summary:

### 2026-11-12: rebrand to Dadeum
- Product name, window title, executable name (`Dadeum.exe`), version (restarted at 0.1.0),
  assembly metadata and user-facing strings in all 18 languages changed to Dadeum (Korean: 다듬).
- Data folder moved to `%LocalAppData%\Dadeum\` so Dadeum never shares revert files with an
  installed optimizerDuck.
- The update check now looks at Dadeum's own releases (`ibrkiller-penz/Dadeum`) and only accepts
  assets named `Dadeum*.exe`, so an upstream build is never offered as an update.
- Links (repository, docs, terms, privacy, disclaimer, acknowledgements) point at the Dadeum
  repository. The Discord buttons are hidden because Dadeum has no community server yet.
- Upstream-only project settings removed: ko-fi funding link, Discord release webhook,
  CODEOWNERS entry.
- The internal C# namespace and project/solution file names stay `optimizerDuck` on purpose,
  to keep merging upstream changes simple.

### 2026-11-12: prune ineffective tweaks, raise risk levels
- Removed "Prioritize Active Applications" (`Win32PrioritySeparation = 38`): 38 (0x26) is
  already the Windows client default, so it changed nothing.
- Removed the Prefetch disk-cleanup item: Windows rebuilds those files and clearing them slows
  the next launch of each program.
- "Optimize Multimedia Scheduler" no longer sets `NetworkThrottlingIndex = 0xFFFFFFFF`
  (other projects reverted it to the default of 10); the description no longer mentions it.
- Risk raised from Safe: "Disable System Power Throttling" (`PowerThrottlingOff`) to Risky,
  "Disable USB power saving" to Moderate, and five GPU driver tweaks (AMD ULPS, AMD ASPM,
  NVIDIA dynamic P-state, Intel async flips, Intel adaptive vsync) to Moderate. They trade
  idle power and heat for unmeasured gains.

### 2026-11-12: consequence and source for each item
- New `EvidenceUrl` on `[Optimization]`, shown in the details dialog as "Source". 22 items link
  verified documentation (mostly Microsoft Learn); items without one show "no verified source".
- New `.Consequence` translation key, shown as "What you give up". Required for every Moderate
  and Risky item (21 items), enforced by `OptimizationEvidenceTests`.
- Consequence text is written in English and Korean and translated into the other 16 locales
  (AI-assisted; a native review is still welcome, especially for Windows feature names).
- "Optimize System Services" no longer touches printing, file sharing, notifications, Windows
  Update, Xbox, Bluetooth or DiagTrack (the telemetry item owns it). About 200 services remain.
