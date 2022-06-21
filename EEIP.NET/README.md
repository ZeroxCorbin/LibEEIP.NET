# EEIP.NET

EtherNet/IP (Ethernet Industrial Protocol = EIP) compatible client for communication with devices in .NET without special knowledge about EtherNet/IP.
EtherNet/IP is adaptation of Common Industrial Protocol (CIP) to Ethernet networks over TCP/UDP/IP networking.

Supports
- Explicit messaging
  - Uses TCP
  - Unconnected Message Manager (UCMM)
- Implicit messaging
  - Uses UDP
  - IO scanner of IO adapter device
- EIP message
  - Encapsulation
  - Common Package format
- CIP
  - EPath
  - Object Library

It can also be used for LabView integration (see LabView example in source code).

## Original implementation

Versions 1.x by [Stefan Rossmann Engineering Solutions](https://sre-solutions.com) (Wörrstadt, Germany)

<a href="http://www.eeip-library.de">Implementation Guide and documentation</a>

Source code is on [Github](https://github.com/rossmann-engineering/EEIP.NET).

<a href="https://sourceforge.net/projects/eeip-net/files/latest/download" rel="nofollow"><img alt="Download EEIP.NET" src="https://a.fsdn.com/con/app/sf-download-button"></a>

## Refactored implementation

Versions 2.x by Marek Ištvánek as part of work for [Kinali](https://www.kinali.cz), (Brno, Czech Republic) while implementing motorized slit feature of [Scienta Omicron](https://scientaomicron.com/) (Uppsala, Sweden) photoelectron spectroscopy analyzers using [Nanotec](https://en.nanotec.com) motor control.

Source code is on [Github](https://github.com/scienta-scientific/EEIP.NET).

Supports
- .NET Standard 2.0
- EIP encapsulation and CIP messages object wrappers and error handling
- EPath construction
- Custom UCMM service codes
- Multiple implicit IO connections
- Disposability