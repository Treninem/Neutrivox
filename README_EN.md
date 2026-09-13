# Neutrivox 0.2.0

Neutrivox is a Windows desktop environment for industrial-automation design. One project combines equipment, I/O, connections, logic, simulation, diagnostics and safe physical-device binding.

## Installation

1. Run `Neutrivox-Setup-0.2.0.exe`.
2. Follow the installation wizard.
3. Start Neutrivox from the shortcut.

For the portable build, extract `Neutrivox-win-x64-0.2.0.zip` and run `Neutrivox.exe`.

## First start

A 7-day Professional trial starts automatically on first launch. When it ends, the application falls back to Free so basic design and simulation remain usable. A paid key can be activated in **Settings → License**.

## Main workflow

1. Select **New project** or **Open**.
2. Add controllers, relays, gateways and I/O modules in **Equipment**.
3. Describe channels in **Inputs & outputs**.
4. Build the project topology in **Diagram**.
5. Create networks, variables and instructions in **Logic**.
6. Run the project without hardware in **Simulation** and inspect I/O and trace results.
7. Resolve blocking findings in **Validation**.
8. Save the project as a `.neutrivox` file.

Neutrivox creates a persistent recovery snapshot roughly every 30 seconds. The latest snapshot can be restored from **Projects**.

## Physical devices

**Connection** supports:

- COM inventory and scoped Modbus RTU discovery;
- TCP/502 endpoint discovery within an explicitly supplied IPv4 address or CIDR range;
- cancellation;
- compatibility checking;
- a separate explicit confirmation step before a discovered device is bound to a project object.

A reachable IP/COM endpoint is not treated as proof of a device model. If a documented profile cannot be identified, Neutrivox does not silently claim one.

## Deployment to controllers

Physical writes use a strict safety gate. Before a write can start, all of the following must be true:

- an explicit project target is selected;
- a physical device is bound and identified;
- an exact documented profile is available;
- that profile has hardware-verified read/write support;
- a tested deployment adapter is registered;
- preflight has no blocking errors;
- the project/plan has not changed since validation;
- the user manually types `DEPLOY` immediately before execution.

For supported OWEN programmable relays, Neutrivox contains an integration point for the official **Owen Logic Replication Utility**. Its executable path is configured in **Settings**. Neutrivox does not invent a proprietary programming protocol and does not mark untested models as physically writable.

## Equipment families

The catalog includes exact PR100 profiles and family-level planning entries for OWEN PR/PLC/SPK/Mx/PRM/PM-PE-PV210, Siemens, Schneider Electric, Mitsubishi Electric, OMRON, Delta, WAGO, Allen-Bradley, Beckhoff and Weintek. Family profiles intentionally do not invent exact I/O counts; exact I/O appears only after a verified model profile is added.

## Licensing

Public plans and prices are stored in RUB. License payloads use RSA signatures. The application contains only the public verification key; the owner's private signing key must never be placed in the repository, installer or customer machines.

The owner can issue licenses with `tools/Neutrivox.LicenseIssuer`. A key may optionally be bound to a specific device fingerprint.

## Local data

`%LOCALAPPDATA%\Neutrivox` stores user settings, trial/license state and the recovery snapshot. Project `.neutrivox` files are stored wherever the user chooses.

## Release boundary

Digital design, persistence, logic, simulation, diagnostics, licensing and Windows packaging are automatically verified in CI. Physical writes to a specific controller model cannot be certified by software-only tests: every profile/adapter combination requires a separate real-hardware validation. Until that validation exists, the safety gate keeps physical writing blocked.
