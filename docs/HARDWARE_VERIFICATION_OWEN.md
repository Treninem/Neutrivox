# Neutrivox — verified OWEN hardware facts

This document records only facts verified against current public OWEN documentation. It is intentionally separate from deployment implementation: a documented communication interface does not automatically prove that a proprietary programming/download protocol is available to Neutrivox.

## PR100

OWEN identifies PR100 as a compact programmable relay. The current product documentation lists USB Device, RS-485, Modbus RTU/Modbus ASCII and Owen Logic among the device interfaces/software context.

For PR100, OWEN's documentation states that programming is performed through Owen Logic and identifies microUSB as the programming interface. The same documentation describes RS-485 communication through Modbus RTU or Modbus ASCII for the relevant variants.

## Confirmed implications for Neutrivox

1. PR100 may be modelled offline in Neutrivox.
2. PR100 I/O information may be represented by verified hardware profiles.
3. RS-485 discovery/diagnostics may use Modbus where the selected variant supports it.
4. RS-485 Modbus access must not be presented as the programming/download mechanism.
5. A physical PR100 deployment adapter must not be marked production-ready until the exact programming data format and transport sequence are independently documented and implemented/tested.
6. A recognized COM port is only an endpoint; it is not by itself proof of PR100 identity.

## Current verified PR100 variants referenced by OWEN

The product documentation currently lists, among others:

- PR100-230.0804.01.0 — 8 DI, 4 relay DO, no RS-485;
- PR100-230.0804.01.1 — 8 DI, 4 relay DO, 1×RS-485;
- PR100-230.1208.01.0 — 12 DI, 8 relay DO, no RS-485;
- PR100-230.1208.01.1 — 12 DI, 8 relay DO, 1×RS-485;
- PR100-24.0804.03.0 — 4 AI, 4 DI, 4 relay DO, no RS-485;
- PR100-24.0804.03.1 — 4 AI, 4 DI, 4 relay DO, 1×RS-485;
- PR100-24.1208.03.0 — 4 AI, 8 DI, 8 relay DO, no RS-485;
- PR100-24.1208.03.1 — 4 AI, 8 DI, 8 relay DO, 1×RS-485.

## Source documents

- OWEN PR100 product/documentation page: https://owen.ru/product/pr100/documentation
- OWEN PR100 programming and configuration documentation: https://docs.owen.ru/product/pr100/doc/rukovodstvo-po-ekspluatacii-pr100/nastrojka-i-programmirovanie
- OWEN Logic 3.1 user guide: https://docs.owen.ru/product/programmnoe_obespechenie_owen_logic/986

## Engineering rule

Neutrivox must distinguish three different claims:

`Device profile` → `Communication capability` → `Verified programming/deployment capability`

Only the third permits the physical deployment action.
