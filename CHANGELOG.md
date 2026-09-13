# Changelog

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
