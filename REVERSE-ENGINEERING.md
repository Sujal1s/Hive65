# Reverse-engineering evidence

Target: C:\Program Files (x86)\Kreo Hive 65\OemDrv.exe
SHA-256: BBEFB7D94EF321955738B64030CC8C8E6C1098A20084BF2DB7850D02D394B07B
Format: native PE32 x86; preferred image base 0x400000. Addresses below are virtual addresses at that base, not file offsets.
Method: read-only PE import/string analysis and Capstone disassembly. No modification or execution of the vendor binary.

## Observations

- Dev/kb/KB.ini: Fw=24 (0x18), VID=0x258a, PID=0x010C, IC2481=1, RGBIndex=0,1,2. KEY entries supply physical geometry and hardware IDs. There are 67 visible key rectangles; the zero-sized mute entry is excluded.
- 0x40bb0d-0x40bc36: firmware 0x18 selects the class with vtable 0x61764c.
- 0x48fbda-0x48fc30: firmware 0x18 requires usage page FF00, usage 1 and feature report length 0x208. 0x48fd3b-0x48fd3e stores the device handle and sets connected flag +0x234 to 1.
- 0x483ee8-0x483f07: when connected flag +0x234 is 1, calls vtable +0x40 with the raw RGB buffer and length 0x17a (378). The grouped-color path below this branch is not used for this implementation.
- 0x483de6-0x483e2d: reads key's byte-sized hardware index at key-record offset +0xc, multiplies it by 3, and writes channels at configured RGBIndex offsets into the raw buffer.
- Vtable 0x61764c + 0x40 points to 0x491100. This passes command 8, write operation 1, parameter 0, and delay 7 to 0x48ff00.
- 0x48ffca-0x49006f: zeroes a 520-byte feature report, selects report ID 6 for firmware 0x18 (9 otherwise), places command at byte 1, parameter at byte 2, zero at byte 3, total packet count at byte 4, zero-based packet index at byte 5, little-endian payload length at bytes 6/7, and payload at byte 8. Calls HidD_SetFeature via thunk 0x5be99c.
- The 378-byte payload fits one packet. Raw key color starts at report offset 8 + 3*hardware_id. Unused slots are zero. No checksum calculation is observed in this selected write function; other firmware classes have separate checksum paths.

## Implementation

Samples the selected monitor into independently positioned physical-key regions (67 visible keys), with optional six-zone or whole-screen modes; averages in linear sRGB with optional saturation weighting, applies brightness and temporal interpolation, fills the 126-slot RGB matrix, and sends report 06 08 00 00 01 00 7A 01 followed by the 378-byte matrix and zero padding through byte 519. Default mode is preview only. Device discovery validates attributes and HID capabilities before opening the output path. No other command IDs are implemented.

## Limits / unresolved hardware questions

No physical Hive 65 was available. USB descriptor values, device response, firmware acceptance, activation prerequisites, actual color order, key alignment, reliable refresh rate and restoration behavior still need physical testing. The vendor path sleeps 7 ms before a write and retries failures; this app targets 67 ms between frames and stops on a failed write. This app does not reproduce the vendor password read/check or full configuration synchronization at connect time. The evidence supports constructing the real-time packets but does not prove that no additional activation is necessary on every firmware revision.

Tests verify software behavior and encoding consistency with the static analysis. They do not substitute for USB capture or hardware tests.
