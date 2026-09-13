# Changelog

## 0.2.0

- Connected the existing project persistence layer to the desktop UI: Open, Save and Save As now work with `.neutrivox` files.
- Added persistent 30-second recovery snapshots that survive application restarts and can be restored from the Projects page.
- Added a persistent 7-day Professional trial with automatic fallback to the usable Free edition after expiry.
- Added production RSA license-signature verification; the private signing key is not stored in the repository or application.
- Added an offline `Neutrivox.LicenseIssuer` owner tool for creating signed, optionally device-bound license payloads.
- Added Settings & License UI with RU/EN persistence, plan display, activation, device fingerprint display and OWEN utility configuration.
- Replaced the placeholder project validation page with project health, logic validation and release pre-check results.
- Expanded the visible project catalog across OWEN, Siemens, Schneider Electric, Mitsubishi Electric, OMRON, Delta, WAGO, Allen-Bradley, Beckhoff and Weintek families.
- Added explicit COM/Modbus RTU and TCP/502 discovery flows with cancellation and a separate confirmation step before binding a discovered device.
- Added a safe sequential deployment UI with target selection, stale-plan protection, explicit `DEPLOY` confirmation, cancellation and execution logging.
- Registered the official OWEN Replication Utility adapter integration point; physical transfer remains blocked unless the exact profile is marked hardware-verified/read-write supported.
- Unified application, installer and CI package versioning at 0.2.0 and added version checks to the Windows release workflow.
- Added smoke coverage for trial expiry/fallback, persistent settings, persistent recovery and invalid RSA signatures.

## 0.1.1

- Expanded the built-in equipment catalog beyond OWEN PR100.
- Added OWEN PM210, PE210 and PV210 communication gateways to the project catalog.
- Corrected PM210 modeling: RS-485/Modbus on the field side with cellular OwenCloud uplink; PE210 uses Ethernet and PV210 uses Wi-Fi.
- Added OWEN family profiles for PR102, PR103, PR200, PR205, PR225, PLC110 [M02], PLC160 [M02], PLC200, PLC210, PLC210-4G, SPK, Mx110/Mx210 and PRM devices.
- Added initial controller-family profiles for Siemens, Schneider Electric, Mitsubishi Electric, OMRON, Delta, WAGO, Allen-Bradley/Rockwell Automation and Beckhoff, plus Weintek HMI profiles.
- Added protocol/transport identifiers for Wi-Fi, cellular, CAN, fieldbus, PROFINET, S7, EtherNet/IP, EtherCAT, CANopen, OPC UA and MQTT.
- Discovery/compatibility now starts from the multi-vendor family registry and then overlays exact verified OWEN profiles.
- Documented the difference between exact device support and family-level planning profiles in `docs/DEVICE_CATALOG.md`.

## 0.1.0

- Added initial release packaging.
- Added Windows build validation.
- Added release documentation.
