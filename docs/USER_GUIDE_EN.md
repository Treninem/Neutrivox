# Neutrivox — User Guide

## Purpose

Neutrivox is designed around one unified automation project: equipment, I/O, diagram, logic, simulation, diagnostics, and later work with physical devices all belong to the same project.

## Basic workflow

1. Create a project.
2. Choose devices from the catalog.
3. Configure I/O channels.
4. Create logic networks and operations.
5. Validate the project.
6. Run simulation without physical hardware.
7. Save the project.
8. When hardware is connected, open Connection & Discovery.
9. Review the detected endpoint and proposed binding.
10. Before physical transfer, review the target list and explicitly confirm the operation.

## Important rule

An IP address, COM port, or Modbus slave address alone does not prove a device model. Neutrivox shows the identification level and must not claim unsupported hardware as compatible.

## PR100

PR100 is programmed using Owen Logic; official documentation identifies micro USB as the programming interface. RS-485 on relevant variants is used for Modbus RTU/Modbus ASCII. Programming and Modbus access are therefore treated as separate technical capabilities.

## Simulation

Simulation uses virtual inputs and outputs from the same project. It must not send commands to physical equipment.

## Multiple-device transfer

Neutrivox creates a sequential deployment plan. Each device has its own step, result, and message. An individual failure must remain visible in the overall result.

## Licensing

Free provides the basic capabilities. Paid plans expand the feature set and may be sold as separate license keys through an external marketplace. The marketplace is not part of the application core.

## Safety

Operate only equipment and systems you are authorized to control. Physical actions require explicit confirmation and compatibility checks.
