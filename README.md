<div align="center">

<h1>✉️ Email Grabber Pro</h1>

<p><strong>Enterprise-grade email validation · attachment extraction · cloud audit · Anti-Public image engine</strong></p>

<p>
  <img src="https://img.shields.io/badge/platform-Windows%2010%20%2F%2011-0078D4?style=for-the-badge&logo=windows&logoColor=white" />
  <img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img src="https://img.shields.io/badge/C%23-13.0-239120?style=for-the-badge&logo=csharp&logoColor=white" />
  <img src="https://img.shields.io/badge/license-Commercial-DC143C?style=for-the-badge" />
</p>

</div>

---

## 📌 Overview

**Email Grabber Pro** is a high-performance Windows desktop application built for security professionals, OSINT analysts and enterprise IT teams.

Six specialized modules in one modern Windows 11–style interface — from bulk IMAP credential validation to forensic cryptocurrency key recovery from photos.

> ⚠️ **Authorized use only.** This tool is intended exclusively for security research, authorized penetration testing, corporate IT audits, and lawful OSINT operations. The user is solely responsible for compliance with applicable laws.

---

## ⚙️ Modules

<details>
<summary><strong>1 · IMAP Email Checker</strong> — validates credentials at scale</summary>
<br>

Validates email credentials against IMAP servers with high parallelism and automatic server resolution.

| Feature | Detail |
|---|---|
| Server resolution | Auto-resolved from `imap_servers.txt` by email domain |
| Proxy support | Socks5 / HTTP, per-account rotation |
| Parallelism | Up to **3 000 threads** |
| Re-check queue | Automatic retry on network errors |
| Output | Real-time stream → `Result/Valid_Email.txt` |

</details>

---

<details>
<summary><strong>2 · Hotmail / Outlook Checker</strong> — Microsoft account auditor</summary>
<br>

Authenticates Outlook and Live accounts via the Microsoft Web API and downloads associated data.

- Downloads emails from **Inbox**, **Sent**, **Drafts**
- Attachment extraction with size cap and extension whitelist
- Integrated **seed-phrase / private key scanner** in email bodies
- Configurable sender and subject keyword filters
- Up to **3 000 parallel sessions** + separate download thread pool

</details>

---

<details>
<summary><strong>3 · IMAP Mix Email</strong> — deep attachment harvesting</summary>
<br>

Extracts attachments from any IMAP-compatible mailbox across all folders simultaneously.

- Searches **Inbox**, **Sent**, **Deleted Items** in parallel
- Extension whitelist: `.pdf` `.jpg` `.png` `.zip` `.xlsx` `.docx` and more
- Seed-phrase / private key detection inside attachments
- Date-range and sender / subject keyword filters

</details>

---

<details>
<summary><strong>4 · OneDrive Auditor</strong> — cloud storage analysis</summary>
<br>

Enumerates and selectively downloads files from OneDrive via the Microsoft Live API.

- **Four sub-modes:** by extension · by keyword · both · internal API search
- File size cap to prevent downloading large irrelevant files
- Integrated seed / key scanner on all downloaded content
- Up to **45 parallel download threads**

</details>

---

<details>
<summary><strong>5 · Photo Seed & Private Key Scanner</strong> — OCR-based crypto forensics</summary>
<br>

Extracts cryptocurrency credentials directly from image files using **on-device OCR** — no internet, no API key required.

**Supported image formats**

`.jpg` `.jpeg` `.png` `.heic` `.heif` `.webp` `.bmp` `.tiff` `.gif` `.avif`

**Detected credential types**

| Type | Pattern |
|---|---|
| BIP-39 Seed phrase | 12 / 15 / 18 / 21 / 24 words — checksum validated |
| Hex private key | 64 hexadecimal characters |
| EVM key | `0x` + 64-char hex |
| WIF key | Starts with `5`, `K` or `L` |

**OCR language support:** English · French · Spanish · Italian · Portuguese · Japanese · Korean · Chinese (Simplified + Traditional) · Czech

Results streamed in real time: `Seed.txt` · `Hex.txt` · `Evm.txt` · `Wif.txt`

Up to **64 parallel workers.**

</details>

---

<details>
<summary><strong>6 · Anti-Public Image Engine</strong> — proprietary perceptual fingerprinting</summary>
<br>

Detects whether a photo is already **"public"** (previously seen and catalogued) using a 6-signal perceptual fingerprinting system — without any cloud service.

### Signal table

| # | Signal | Algorithm | Resistant to |
|---|---|---|---|
| 1 | SHA-256 | Exact file hash | — |
| 2 | pHash ×5 | DCT perceptual hash in **5 orientations** | Resize · JPEG compression · format conversion · rotation 90/180/270° · horizontal flip |
| 3 | dHash | Horizontal gradient hash | Brightness / contrast changes |
| 4 | aHash | Average hash | Global structure changes |
| 5 | edge-pHash | pHash computed on Canny-edge image | Colour filters · heavy toning |
| 6 | Histogram | Bhattacharyya coefficient | Any spatial transformation |

### How it works

```
Run 1 — BUILD MODE
  Scan your "known / public" photo library.
  All images are indexed into the database.
  Nothing is moved.

Run 2+ — DETECTION MODE
  Each image is compared against the database using 5 independent rules.
  Images NOT matching → classified as Anti-Public
  → automatically moved to: Result\Anti-Public_DD_MM_YYYY_HH_MM_SS\
```

### Detection rules (any one match = image is KNOWN)

```
Rule 1  SHA-256 exact file match
Rule 2  best pHash ≤ 8  across all 5 rotation/flip variants
Rule 3  pHash ≤ 14  AND  dHash ≤ 10  AND  aHash ≤ 10
Rule 4  edge-pHash ≤ 6  AND  histogram BC ≥ 0.95
Rule 5  dHash ≤ 5  AND  aHash ≤ 5   AND  histogram BC ≥ 0.97
```

The **database is stored permanently** next to the executable in `AntiPublic/` and is never deleted with the Result folder.

</details>

---

## 💻 System Requirements

| | |
|---|---|
| **OS** | Windows 10 (build 22621+) or Windows 11 |
| **Runtime** | .NET 9 — bundled in the release build |
| **CPU** | x64, 4+ cores recommended |
| **RAM** | 4 GB minimum · 8 GB recommended |
| **OCR** | Windows OCR language packs (Settings → Time & Language → Language) |

---

## 🚀 Quick Start

```
1. Download the latest release from the Releases tab
2. Extract to any folder — no installation required
3. Run Email Grabber.exe
4. Enter your license key on first launch
```

Optionally place these files next to the executable:

```
imap_servers.txt    — custom IMAP server mappings
imap_accounts.txt   — email:password credential list
proxies.txt         — proxy list
```

---

## 📄 Input File Formats

**`imap_accounts.txt`**
```
user@gmail.com:password123
user@outlook.com:mypassword
```

**`imap_servers.txt`** — three supported formats
```ini
# Tag-based
[DOMAINS]gmail.com[/DOMAINS][SERVER]imap.gmail.com[/SERVER][PORT]993[/PORT][SSL]true[/SSL]

# INI key=value
outlook.com = imap-mail.outlook.com:993

# Whitespace-separated
yahoo.com   imap.mail.yahoo.com   993
```

**`proxies.txt`**
```
192.168.1.1:1080
user:pass@192.168.1.2:1080
socks5://user:pass@proxy.example.com:1080
http://user:pass@proxy.example.com:8080
```

---

## 📁 Output Structure

```
app-folder/
│
├── AntiPublic/                        ← permanent DB (never cleared)
│   ├── phash_database.txt             ← perceptual hash index
│   └── AntiPublic_features.txt        ← full visual feature log
│
└── Result/
    ├── Valid_Email.txt                 ← IMAP checker — valid credentials
    │
    ├── Hotmail_DD_MM_YYYY_HH_MM_SS/   ← Hotmail run results
    │   ├── emails/
    │   └── attachments/
    │
    ├── IMAP_DD_MM_YYYY_HH_MM_SS/      ← IMAP Mix run results
    │   └── account@domain/
    │
    ├── OneDrive_DD_MM_YYYY_HH_MM_SS/  ← OneDrive run results
    │
    ├── PhotoSeed&PK_DD_MM_YYYY/       ← Photo scanner results
    │   ├── Seed.txt
    │   ├── Hex.txt
    │   ├── Evm.txt
    │   └── Wif.txt
    │
    └── Anti-Public_DD_MM_YYYY/        ← Anti-Public detected images
```

---

## 🔧 Technical Stack

| Component | Technology |
|---|---|
| Language | C# 13 / .NET 9 |
| UI framework | WPF + ModernWpf (Windows 11 design language) |
| IMAP protocol | MailKit |
| Image processing | Magick.NET (ImageMagick bindings) |
| On-device OCR | Windows.Media.Ocr — no API key required |
| Crypto validation | NBitcoin |
| Serialization | System.Text.Json |

---

## 💳 Licensing

This is **commercial software**. A valid license key is required to run the application.

| Plan | Modules included | Duration |
|---|---|---|
| **Basic** | IMAP Checker + Hotmail | 30 days |
| **Professional** | All 6 modules | 30 days |
| **Enterprise** | All 6 modules + priority support | 90 days |

📩 **Purchase & inquiries** → Telegram

---

## ⚖️ Legal Disclaimer

This software is provided for **authorized** security research and IT administration purposes only.

- You must have **explicit written authorization** before using this tool against any system you do not own
- Unauthorized use against third-party systems may violate local and international laws including the CFAA, GDPR, and equivalents
- The authors and distributors accept **no liability** for misuse

---

<div align="center">

Built for security professionals &nbsp;·&nbsp; Windows 10 / 11 &nbsp;·&nbsp; .NET 9

</div>
