<div align="center">
  <img src="https://i.imgur.com/KbomEco.png" alt="AsyncRAT Logo">

  # AsyncRAT

  > A Remote Access Tool (RAT) designed to remotely monitor and control other computers through a secure encrypted connection.

  [![License](http://img.shields.io/:license-mit-blue.svg?style=flat-square)](/LICENSE)
</div>

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technical Details](#technical-details)
- [Installation & Deployment](#installation--deployment)
- [Plugins](#plugins)
- [Download](#download)
- [Contributing](#contributing)
- [Donation](#donation)
- [Legal Disclaimer](#legal-disclaimer)
- [License](#license)

---

## Overview

AsyncRAT is a Remote Access Tool built in C# that provides a secure, encrypted channel for remotely monitoring and controlling client machines. It includes:

- A plugin system to send and receive commands
- An access terminal for controlling connected clients
- A configurable client manageable via terminal
- A log server that records all significant events

---

## Features

### Client

| Feature | Description |
|---------|-------------|
| Screen Viewer & Recorder | View and record the client's screen in real time |
| Antivirus & Integrity Manager | Detect and manage antivirus presence and file integrity |
| SFTP Access | Upload and download files from the client |
| Chat Window | Bidirectional chat between client and server |
| Dynamic DNS & Multi-Server | Configurable support for DDNS and multiple servers |
| Password Recovery | Recover saved passwords from the client |
| JIT Compiler | On-the-fly compilation on the client side |
| Keylogger | Log keystrokes on the client |
| Anti-Analysis | Configurable techniques to evade analysis |
| Antimalware Startup | Prevent malware from starting up on the client |

### Server

| Feature | Description |
|---------|-------------|
| Controlled Updates | Push updates to clients from the server |
| Config Editor | Edit server and client configurations |
| Multiport Receiver | Accept connections on multiple ports (configurable) |
| Thumbnails | View client screen thumbnails |
| Binary Builder | Build client binaries (configurable) |
| Obfuscator | Obfuscate client binaries (configurable) |

---

## Technical Details

The following external services are used in this project:

| Service | Purpose |
|---------|---------|
| [pastebin.com](https://pastebin.com) | Used for the "PasteBin" option in the client builder |
| [github.com](https://github.com) | Used for downloading and uploading project changes |

---

## Installation & Deployment

AsyncRAT requires the [.NET Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) v10 (`net10.0-windows`) to run.

> **Note:** Visual Studio 2022 or above is required to compile this project.

---

## Plugins

The program integrates the following third-party DLLs:

| Plugin | Source |
|--------|--------|
| StealerLib | [gitlab.com/thoxy/stealerlib](https://gitlab.com/thoxy/stealerlib) |

---

## Download

Download the latest release from:  
[AsyncRAT-C-Sharp/releases](https://github.com/NYAN-x-CAT/AsyncRAT-C-Sharp/releases)

---

## Contributing

Contributions from C# and .NET developers are welcome!

1. Read through the project to understand its structure.
2. Fork the repository and apply your changes.
3. Open a Pull Request with an associated Issue that describes:
   - What you changed
   - Why you changed it
   - Why it should be implemented

---

## Donation

If you find this project useful, consider buying me a coffee!

**BTC:** `12DaUTCemhDEzNw7cAFg9FndzcWkYZt6C8`

---

## Legal Disclaimer

> **PLEASE READ:** I, the creator, and all those associated with the development and production of this program are **not responsible** for any actions or damages caused by this software. You bear full responsibility for your actions and acknowledge that this software was created **for educational purposes only**. This software is **not** intended to be used maliciously or on any system you do not own or have explicit permission to operate. By using this software, you automatically agree to the above.

---

## License

This project is licensed under the **MIT License** — see the [LICENSE](/LICENSE) file for details.
