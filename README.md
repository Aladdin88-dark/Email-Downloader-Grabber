<div align="center">
# Email Grabber Pro
**Enterprise-grade email validation, attachment extraction, cloud audit & Anti-Public image engine**
[![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-blue?logo=windows)](https://github.com/Aladdin88-dark/Email-Downloader-Grabber)
[![Framework](https://img.shields.io/badge/.NET-9.0-purple?logo=dotnet)](https://dotnet.microsoft.com/)
[![Language](https://img.shields.io/badge/language-C%23-239120?logo=csharp)](https://github.com/Aladdin88-dark/Email-Downloader-Grabber)
[![License](https://img.shields.io/badge/license-Commercial-red)](https://github.com/Aladdin88-dark/Email-Downloader-Grabber)
[![Contact](https://img.shields.io/badge/contact-Telegram-2CA5E0?logo=telegram)](https://t.me/YOUR_TELEGRAM)
</div>
---
## Overview
**Email Grabber Pro** is a high-performance Windows desktop application built for security professionals, OSINT analysts, and enterprise IT teams. It consolidates six specialized modules into a single modern interface — from bulk IMAP credential validation to forensic crypto key recovery from photos.
> ⚠️ **Authorized use only.** This tool is intended exclusively for security research, authorized penetration testing, corporate IT audits, and lawful OSINT operations. The user is solely responsible for compliance with applicable laws.
---
## Modules
### 1. IMAP Email Checker
Validates email credentials against IMAP servers at scale.
- Auto-resolves IMAP server from domain (`imap_servers.txt`)
- Socks5 / HTTP proxy support with per-account rotation
- Optional re-check queue for failed accounts (network errors)
- Real-time result streaming to `Result/Valid_Email.txt`
- Parallelism up to 3 000 threads
### 2. Hotmail / Outlook Checker
Authenticates Outlook / Live accounts via **Microsoft Graph API**.
- Downloads emails from Inbox, Sent, Drafts
- Attachment extraction with size and extension filters
- Integrated seed-phrase / private key scanner
- Configurable sender / subject keyword filters
- Up to 3 000 parallel sessions + separate download thread pool
### 3. IMAP Mix Email
Deep attachment harvesting from any IMAP-compatible mailbox.
- Searches Inbox, Sent, Deleted Items simultaneously
- Extension whitelist (`.pdf`, `.jpg`, `.png`, `.zip`, `.xlsx`, `.docx`, …)
- Seed-phrase / private key detection in attachments
- Date-range and sender/subject filters
### 4. OneDrive Auditor
Cloud storage enumeration and selective download via **Microsoft Live API**.
- Four sub-modes: by extension, by keyword, extension + keyword, API search
- File size cap (prevents downloading huge files)
- Integrated seed/key scanner on downloaded content
- Up to 45 parallel download threads
### 5. Photo Seed & Private Key Scanner
Extracts cryptocurrency credentials directly from image files using **on-device OCR**.
- Supports: `.jpg` `.jpeg` `.png` `.heic` `.heif` `.webp` `.bmp` `.tiff` `.gif` `.avif`
- Multi-language OCR engine (English, French, Spanish, Italian, Portuguese, Japanese, Korean, Chinese, Czech)
- Detects: **BIP-39 mnemonic phrases** (12/15/18/21/24 words, checksum-validated), **Hex private keys** (64-char), **EVM keys** (`0x`-prefixed), **WIF keys**
- Parallel processing (up to 64 workers)
- Results streamed in real time: `Seed.txt`, `Hex.txt`, `Evm.txt`, `Wif.txt`
### 6. Anti-Public Image Engine *(proprietary)*
Detects whether a photo is already "public" (previously catalogued) using a **6-signal perceptual fingerprinting system**.
| Signal | Algorithm | Resistant to |
|--------|-----------|--------------|
| SHA-256 | Exact file hash | — |
| pHash ×5 | DCT perceptual hash in 5 orientations | Resize, JPEG, format convert, rotation, flip |
| dHash | Horizontal gradient hash | Brightness / contrast |
| aHash | Average hash | Global structure changes |
| edge-pHash | pHash on Canny edges | Colour filters / heavy toning |
| Histogram | Bhattacharyya coefficient | Any spatial transformation |
**How it works:**
- **Run 1 (Build)** — indexes your "known/public" photo library. Nothing is moved.
- **Run 2+ (Detect)** — every image is compared against the indexed database. Images *not* in the database are classified as **Anti-Public** and automatically moved to a dated result folder (`Result\Anti-Public_DD_MM_YYYY_HH_MM_SS\`).
Detection rules (any one fires → image is KNOWN):
Rule 1 — SHA-256 exact match Rule 2 — best pHash distance ≤ 8 (across all 5 rotation/flip variants) Rule 3 — pHash ≤ 14 AND dHash ≤ 10 AND aHash ≤ 10 Rule 4 — edge-pHash ≤ 6 AND histogram BC ≥ 0.95 Rule 5 — dHash ≤ 5 AND aHash ≤ 5 AND histogram BC ≥ 0.97

---
## System Requirements
| | |
|---|---|
| **OS** | Windows 10 (22621+) or Windows 11 |
| **Runtime** | .NET 9 (bundled in release build) |
| **CPU** | x64, 4+ cores recommended |
| **RAM** | 4 GB minimum, 8 GB recommended |
| **OCR** | Windows OCR language packs (installed via Windows Settings) |
---
## Deployment
1. Download the latest release from [Releases](https://github.com/Aladdin88-dark/Email-Downloader-Grabber/releases)
2. Extract to any folder — no installation required
3. Run `Email Grabber.exe`
4. Enter your license key on first launch
Optionally place next to the executable:
imap_servers.txt — custom IMAP server mappings imap_accounts.txt — email:password list proxies.txt — proxy list (ip:port or user:pass@ip:port)

---
## Configuration Files
### `imap_servers.txt` — IMAP server definitions
```ini
# Format 1 — tag-based
[DOMAINS]gmail.com[/DOMAINS][SERVER]imap.gmail.com[/SERVER][PORT]993[/PORT][SSL]true[/SSL]
# Format 2 — INI key=value
outlook.com = imap-mail.outlook.com:993
# Format 3 — whitespace-separated
yahoo.com   imap.mail.yahoo.com   993
imap_accounts.txt — credential list
user@gmail.com:password123
user@outlook.com:mypassword
proxies.txt — proxy list
192.168.1.1:1080
user:pass@192.168.1.2:1080
socks5://user:pass@proxy.example.com:1080
Output Structure
[app folder]/
├── AntiPublic/                    ← persistent database (survives Result cleanup)
│   ├── phash_database.txt         ← hash index of all known images
│   └── AntiPublic_features.txt    ← full feature log
│
└── Result/
    ├── Valid_Email.txt             ← IMAP checker results
    ├── Hotmail_DD_MM_YYYY/         ← Hotmail results
    │   ├── emails/
    │   └── attachments/
    ├── PhotoSeed&PK_DD_MM_YYYY/   ← Crypto key scan results
    │   ├── Seed.txt
    │   ├── Hex.txt
    │   ├── Evm.txt
    │   └── Wif.txt
    └── Anti-Public_DD_MM_YYYY/    ← Anti-Public detected images
Licensing
This is commercial software. A valid license key is required to use the application.

Plan	Features	Duration
Basic
IMAP Checker + Hotmail
30 days
Professional
All modules
30 days
Enterprise
All modules + priority support
90 days
📩 Purchase / inquiries: Telegram

Technical Stack
Language: C# 13 / .NET 9
UI: WPF + ModernWpf (Windows 11 design language)
IMAP: MailKit
Image processing: Magick.NET (ImageMagick bindings)
OCR: Windows.Media.Ocr (on-device, no API key required)
Crypto: NBitcoin
Serialization: System.Text.Json
Legal
This software is provided for authorized security research and IT administration purposes only.

You must have explicit written authorization before using this tool against any system you do not own.
Unauthorized use against third-party systems may violate local and international laws.
The authors accept no liability for misuse.
Made with ❤️ for security professionals
