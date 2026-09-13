# Device catalog

Neutrivox separates **exact device definitions** from **family profiles**.

- Exact definitions may contain concrete I/O channels and can be used directly for project I/O configuration.
- Family profiles describe a vendor/model family, typical transports and protocols. They intentionally do not invent I/O counts for hardware variants that have not been selected yet.
- Program upload/download is not implied by the presence of a family profile. Vendor-specific transfer remains disabled until a documented adapter is implemented and tested for that family.

## OWEN / ОВЕН

### Exact or verified profiles

- ПР100 documented variants
- ПМ210 OwenCloud gateway
- ПЕ210 OwenCloud Ethernet gateway
- ПВ210 OwenCloud Wi-Fi gateway

ПМ210 is modeled as a communication gateway rather than a PLC: field devices connect over RS-485/Modbus and the upstream OwenCloud connection uses cellular communication. ПЕ210 uses Ethernet for the upstream link and ПВ210 uses Wi-Fi.

### Family profiles

- ПР100, ПР102, ПР103, ПР200, ПР205, ПР225
- ПЛК110 [М02], ПЛК160 [М02], ПЛК200, ПЛК210, ПЛК210-4G
- СПК family
- Мх110 and Мх210 remote I/O families
- ПРМ expansion modules
- ПМ210, ПЕ210, ПВ210 communication gateways

Primary references:

- https://owen.ru/
- https://owen.ru/catalog/programmiruemie_logicheskie_kontrolleri/info/general_information
- https://owen.ru/product/pr100/documentation
- https://owen.ru/product/pm210

## Other manufacturers

Initial family profiles are included for:

- Siemens: LOGO!, SIMATIC S7-1200 G2, SIMATIC S7-1500
- Schneider Electric: Zelio Logic, Modicon M221, M241, M251, M262
- Mitsubishi Electric: MELSEC iQ-F / FX5
- OMRON: CP2E, NX1P2
- Delta: DVP, AS Series, AH Series
- WAGO: PFC200
- Allen-Bradley / Rockwell Automation: Micro800, CompactLogix 5380
- Beckhoff: CX Embedded PC controllers
- Weintek: HMI family for controller/HMI projects

These profiles are intentionally conservative. Exact CPU/module variants, I/O maps, electrical limits and programming-transfer adapters must be added from official documentation before Neutrivox claims exact hardware support.

## Integration rule

When a family profile and a verified exact profile overlap, the exact documented profile takes precedence. This lets Neutrivox grow to more vendors without weakening the accuracy of already verified devices.
