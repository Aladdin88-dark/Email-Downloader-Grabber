<div align="center">
# ✉️ Email Grabber Pro
**Enterprise-grade email validation · attachment extraction · cloud audit · Anti-Public image engine**
![Platform](https://img.shields.io/badge/platform-Windows%2010%20%2F%2011-0078D4?logo=windows&logoColor=white)
![Framework](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)
![Language](https://img.shields.io/badge/C%23-13.0-239120?logo=csharp&logoColor=white)
![License](https://img.shields.io/badge/license-Commercial-crimson)
</div>
---
## What is this?
**Email Grabber Pro** is a high-performance Windows desktop application for security professionals, OSINT analysts and enterprise IT teams.
Six specialized modules in one modern interface — from bulk IMAP credential validation to forensic crypto key recovery from photos.
> ⚠️ For authorized security research, corporate IT audits, and lawful OSINT only.
---
## Modules
### 1 · IMAP Email Checker
Validates email credentials against IMAP servers at scale.
| Feature | Detail |
|---|---|
| Server resolution | Auto-resolves from `imap_servers.txt` |
| Proxy support | Socks5 / HTTP, per-account rotation |
| Parallelism | Up to 3 000 threads |
| Re-check queue | Retries on network errors |
| Output | Real-time stream to `Result/Valid_Email.txt` |
---
### 2 · Hotmail / Outlook Checker
Authenticates Outlook / Live accounts via Microsoft Web API.
- Downloads emails from Inbox, Sent, Drafts
- Attachment extraction with size and extension filters
- Integrated seed-phrase / private key scanner
- Configurable sender and subject keyword filters
- Up to 3 000 parallel sessions + separate download thread pool
---
### 3 · IMAP Mix Email
Deep attachment harvesting from any IMAP-compatible mailbox.
- Searches Inbox, Sent, Deleted Items simultaneously
- Extension whitelist: `.pdf` `.jpg` `.png` `.zip` `.xlsx` `.docx` and more
- Seed-phrase / private key detection in attachments
- Date-range and sender / subject filters
---
### 4 · OneDrive Auditor
Cloud storage enumeration and selective download via Microsoft Live API.
- Four sub-modes: by extension / keyword / both / API search
- File size cap to avoid downloading huge files
- Integrated seed/key scanner on downloaded content
- Up to 45 parallel download threads
---
### 5 · Photo Seed & Private Key Scanner
Extracts cryptocurrency credentials directly from image files using **on-device OCR**.
**Supported formats**
`.jpg` `.jpeg` `.png` `.heic` `.heif` `.webp` `.bmp` `.tiff` `.gif` `.avif`
**Detected credentials**
| Type | Format |
|---|---|
| BIP-39 Seed phrase | 12 / 15 / 18 / 21 / 24 words, checksum-validated |
| Hex private key | 64 hexadecimal characters |
| EVM key | `0x`-prefixed 64-char key |
| WIF key | Starts with `5`, `K` or `L` |
**OCR languages:** English, French, Spanish, Italian, Portuguese, Japanese, Korean, Chinese (Simplified + Traditional), Czech
Results streamed in real time to: `Seed.txt` · `Hex.txt` · `Evm.txt` · `Wif.txt`
---
### 6 · Anti-Public Image Engine *(proprietary)*
Detects whether a photo is already **"public"** (previously catalogued) using a 6-signal perceptual fingerprinting system.
**Signal table**
| Signal | Algorithm | Resistant to |
|---|---|---|
| SHA-256 | Exact file hash | — |
| pHash ×5 | DCT perceptual hash in 5 orientations | Resize, JPEG, format convert, rotation ±90/180/270°, flip |
| dHash | Horizontal gradient hash | Brightness / contrast changes |
| aHash | Average hash | Global structure changes |
| edge-pHash | pHash on Canny edges | Colour filters, heavy toning |
| Histogram | Bhattacharyya coefficient | Any spatial transformation |
**Workflow**
- **Run 1 — Build mode** Index your "known / public" photo library. Nothing is moved.
- **Run 2+ — Detect mode** Every image is compared against the database. Images not in the database are classified as Anti-Public and automatically moved to `Result\Anti-Public_DD_MM_YYYY_HH_MM_SS\`
**Detection rules** — any single rule firing classifies the image as KNOWN:
Rule 1 SHA-256 exact match Rule 2 best pHash ≤ 8 (across all 5 rotation/flip variants) Rule 3 pHash ≤ 14 AND dHash ≤ 10 AND aHash ≤ 10 Rule 4 edge-pHash ≤ 6 AND histogram BC ≥ 0.95 Rule 5 dHash ≤ 5 AND aHash ≤ 5 AND histogram BC ≥ 0.97

---
## System Requirements
| | |
|---|---|
| OS | Windows 10 (build 22621+) or Windows 11 |
| Runtime | .NET 9 — bundled in release build |
| CPU | x64, 4+ cores recommended |
| RAM | 4 GB minimum, 8 GB recommended |
| OCR | Windows OCR language packs |
---
## Quick Start
Download the latest release (see Releases tab)
Extract to any folder — no installation required
Run Email Grabber.exe
Enter your license key on first launch
**Optional files next to the executable:**
imap_servers.txt custom IMAP server mappings imap_accounts.txt email:password credential list proxies.txt proxy list

---
## Input Formats
**imap_accounts.txt**
user@gmail.com:password user@outlook.com:mypassword

**imap_servers.txt**
[DOMAINS]gmail.com[/DOMAINS][SERVER]imap.gmail.com[/SERVER][PORT]993[/PORT][SSL]true[/SSL] outlook.com = imap-mail.outlook.com:993 yahoo.com imap.mail.yahoo.com 993

**proxies.txt**
192.168.1.1:1080 user:pass@192.168.1.2:1080 socks5://user:pass@proxy.example.com:1080

---
## Output Structure
app-folder/ │ ├── AntiPublic/ ← persistent DB, NOT deleted with Result │ ├── phash_database.txt │ └── AntiPublic_features.txt │ └── Result/ ├── Valid_Email.txt ├── Hotmail_DD_MM_YYYY/ │ ├── emails/ │ └── attachments/ ├── PhotoSeed&PK_DD_MM_YYYY/ │ ├── Seed.txt │ ├── Hex.txt │ ├── Evm.txt │ └── Wif.txt └── Anti-Public_DD_MM_YYYY/

---
## Technical Stack
| Component | Library |
|---|---|
| Language | C# 13 / .NET 9 |
| UI | WPF + ModernWpf (Windows 11 style) |
| IMAP | MailKit |
| Image processing | Magick.NET (ImageMagick) |
| OCR | Windows.Media.Ocr — on-device, no API key |
| Crypto validation | NBitcoin |
| Serialization | System.Text.Json |
---
## Licensing
Commercial software. A valid license key is required.
| Plan | Modules | Duration |
|---|---|---|
| Basic | IMAP Checker + Hotmail | 30 days |
| Professional | All 6 modules | 30 days |
| Enterprise | All modules + priority support | 90 days |
📩 **Purchase and inquiries:** Telegram `@YOUR_USERNAME`
---
## Legal
This software is intended for **authorized** security research and IT administration only.
- You must have explicit authorization before using this tool against any system you do not own
- Unauthorized use may violate local and international law
- The authors accept no liability for misuse
