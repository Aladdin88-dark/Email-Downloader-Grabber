<div align="center">

<img src="https://capsule-render.vercel.app/api?type=waving&color=gradient&customColorList=12&height=220&section=header&text=Email%20Grabber%20Pro&fontSize=56&fontColor=ffffff&fontAlignY=38&desc=Enterprise%20Email%20%26%20Security%20Analysis%20Suite&descSize=17&descAlignY=60&descColor=ddddff&animation=fadeIn" width="100%"/>

</div>

<div align="center">

[![RU](https://img.shields.io/badge/README-🇷🇺%20Русский-0057a8?style=for-the-badge)](./README.ru.md)
&nbsp;
[![Telegram](https://img.shields.io/badge/Contact-@YOUR__TELEGRAM__HERE-2CA5E0?style=for-the-badge&logo=telegram&logoColor=white)](https://t.me/YOUR_TELEGRAM_HERE)

</div>

<br/>

<div align="center">

<img src="https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-0078D4?style=flat-square&logo=windows&logoColor=white"/>
&nbsp;
<img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet&logoColor=white"/>
&nbsp;
<img src="https://img.shields.io/badge/Language-C%23%2013-239120?style=flat-square&logo=csharp&logoColor=white"/>
&nbsp;
<img src="https://img.shields.io/badge/License-Commercial-DC143C?style=flat-square"/>
&nbsp;
<img src="https://img.shields.io/badge/IMAP%20Database-3%2C000%2C000%2B%20servers-f57c00?style=flat-square"/>
&nbsp;
<img src="https://img.shields.io/badge/Status-Active%20Development-00C851?style=flat-square"/>

</div>

<br/>

<div align="center">

```
╔══════════════════════════════════════════════════════════════════════════╗
║  6 professional modules  ·  3M+ IMAP servers  ·  No proxy required     ║
║  On-device OCR  ·  Crypto forensics  ·  Anti-Public image engine        ║
╚══════════════════════════════════════════════════════════════════════════╝
```

</div>

---

<div align="center">

## At a Glance

</div>

<div align="center">

|   |   |   |
|:---:|:---:|:---:|
| 📡 **3,000,000+** | 🔑 **4 key types** | 🌐 **Proxy-free** |
| Built-in IMAP server database | Seed · Hex · EVM · WIF | Works on direct connection |
| 📷 **On-device OCR** | 👁️ **~1 KB per photo** | ⚡ **3,000 threads** |
| No API key, fully offline | Anti-Public stores only hashes | Max parallelism |

</div>

---

## 📦 Modules

<table>
<tr>
<td width="50%" valign="top">

### 📬 IMAP Email Checker

Validates email credentials against IMAP servers at massive scale.

```
✔  Works without proxies (direct connection)
✔  3,000,000+ built-in server mappings
✔  Auto-resolves server from email domain
✔  Socks5 / HTTP proxy with per-account rotation
✔  Re-check queue for failed accounts
✔  Up to 3,000 parallel threads
✔  Real-time streaming → Valid_Email.txt
```

</td>
<td width="50%" valign="top">

### 🔵 Hotmail / Outlook Checker

Microsoft account audit via Web API.

```
✔  Inbox · Sent · Drafts simultaneous download
✔  Attachment extraction (size + extension filters)
✔  Built-in BIP-39 seed phrase scanner
✔  Built-in private key scanner (Hex · EVM · WIF)
✔  Download emails by keyword (sender / subject)
✔  Date-range filters
✔  Up to 3,000 sessions + download thread pool
```

</td>
</tr>
<tr>
<td width="50%" valign="top">

### 📥 IMAP Mix Email

Deep forensic harvesting from any IMAP mailbox.

```
✔  Inbox · Sent · Deleted Items in parallel
✔  Extension whitelist (pdf jpg png zip xlsx…)
✔  Seed phrase / private key detection
✔  Download emails by keyword
✔  Sender / subject / date-range filters
✔  Works without proxies
```

</td>
<td width="50%" valign="top">

### ☁️ OneDrive Auditor

Cloud storage enumeration via Microsoft Live API.

```
✔  4 sub-modes: extension / keyword / both / API
✔  File size cap to skip large irrelevant files
✔  Seed phrase / private key scanner on results
✔  Up to 45 parallel download threads
✔  Works without proxies
```

</td>
</tr>
<tr>
<td width="50%" valign="top">

### 📷 Photo Seed & Key Scanner

Extracts cryptocurrency credentials from images using **on-device OCR**.

```
✔  No internet · no API key · fully offline
✔  jpg png heic heif webp bmp tiff gif avif
✔  OCR: EN FR ES IT PT JP KO ZH CS + more
✔  BIP-39 seed phrases (12–24 words, checksum)
✔  Hex private keys (64-char)
✔  EVM keys (0x-prefixed)
✔  WIF keys (5 / K / L prefix)
✔  Up to 64 parallel workers
✔  Real-time: Seed.txt Hex.txt Evm.txt Wif.txt
```

</td>
<td width="50%" valign="top">

### 👁️ Anti-Public Image Engine

Detects unique / previously unseen images using **6-signal perceptual fingerprinting**.

```
✔  Works without proxies or internet
✔  Stores only hash fingerprints (~1 KB/image)
✔  Original photos are NEVER stored on disk
✔  6 signals: SHA-256 · pHash×5 · dHash
            · aHash · edge-pHash · Histogram
✔  Rotation invariant: 0° 90° 180° 270° + flip
✔  Persistent hash database survives Result cleanup
✔  Auto-moves Anti-Public images to Result folder
```

</td>
</tr>
</table>

---

## 👁️ Anti-Public — How It Works

<div align="center">

| Signal | What It Does | Resistant To |
|:---:|:---|:---|
| **SHA-256** | Exact byte-level file match | — |
| **pHash ×5** | DCT perceptual hash in 5 orientations | Resize · JPEG · format conversion · rotation · flip |
| **dHash** | Horizontal gradient comparison | Brightness · contrast adjustments |
| **aHash** | Global average luminance hash | Overall structural changes |
| **edge-pHash** | pHash applied to Canny edge map | Colour grading · heavy filters · toning |
| **Histogram** | Bhattacharyya colour distribution | All spatial transformations |

</div>

<br/>

```
  ┌──────────────────────────────────────────────────────────────────────┐
  │                      WORKFLOW                                        │
  │                                                                      │
  │  RUN 1 — BUILD MODE                                                  │
  │  ┌────────────────────────────────────────────────────────────┐      │
  │  │  Feed your "known / public" library                        │      │
  │  │  → Only hash fingerprints saved  (~1 KB per image)         │      │
  │  │  → Original photos untouched. Nothing is moved.            │      │
  │  └────────────────────────────────────────────────────────────┘      │
  │                                                                      │
  │  RUN 2+ — DETECTION MODE                                             │
  │  ┌──────────────────────────┐   ┌──────────────────────────┐        │
  │  │  Image found in DB       │   │  Image NOT in DB          │        │
  │  │  → KNOWN (public)        │   │  → ANTI-PUBLIC (unique)   │        │
  │  │  → Stays in place        │   │  → Moved to Result\       │        │
  │  └──────────────────────────┘   └──────────────────────────┘        │
  └──────────────────────────────────────────────────────────────────────┘
```

**Detection rules** — first match wins:

```
  Rule 1  SHA-256 exact file match
  Rule 2  best pHash ≤ 8     across all 5 rotation/flip variants
  Rule 3  pHash ≤ 14   AND   dHash ≤ 10   AND   aHash ≤ 10
  Rule 4  edge-pHash ≤ 6    AND   histogram BC ≥ 0.95
  Rule 5  dHash ≤ 5    AND   aHash ≤ 5    AND   histogram BC ≥ 0.97
```

---

## 💳 Pricing

<div align="center">

<table>
<tr>
<td align="center" width="33%">

### 🗓️ Weekly
# $50
**7 days**

All 6 modules  
Full functionality  
Email support

</td>
<td align="center" width="33%">

### 📅 Monthly
# $80
**30 days**

All 6 modules  
Full functionality  
Priority support

</td>
<td align="center" width="33%">

### ♾️ Lifetime
# $200
**Forever**

All 6 modules  
All future updates  
Priority support

</td>
</tr>
</table>

<br/>

> 💬 **To purchase a license or ask any question:**
>
> ### [`@YOUR_TELEGRAM_HERE`](https://t.me/YOUR_TELEGRAM_HERE)
>
> *(Replace `YOUR_TELEGRAM_HERE` with your actual username)*

</div>

---

## 🚀 Quick Start

**1. Download** the latest release from the [Releases](../../releases) tab

**2. Extract** to any folder — portable, no installation required

**3. Run** `Email Grabber.exe`

**4. Enter** your license key on first launch

<br/>

Optional files next to the executable:

```
imap_servers.txt    — custom server overrides  (3M+ built-in, usually not needed)
imap_accounts.txt   — email:password list
proxies.txt         — proxy list              (optional — works without proxies)
```

---

## 📄 Input File Formats

<details>
<summary><b>imap_accounts.txt</b></summary>
<br/>

```
user@gmail.com:password
user@outlook.com:password
user@yahoo.com:password
```

</details>

<details>
<summary><b>imap_servers.txt</b> — three formats supported</summary>
<br/>

```ini
# Format 1 — tag-based (recommended)
[DOMAINS]gmail.com[/DOMAINS][SERVER]imap.gmail.com[/SERVER][PORT]993[/PORT][SSL]true[/SSL]

# Format 2 — INI key=value
outlook.com = imap-mail.outlook.com:993

# Format 3 — whitespace-separated
yahoo.com   imap.mail.yahoo.com   993
```

</details>

<details>
<summary><b>proxies.txt</b> — optional (works without proxies)</summary>
<br/>

```
192.168.1.1:1080
user:pass@192.168.1.2:1080
socks5://user:pass@proxy.example.com:1080
http://user:pass@proxy.example.com:8080
```

</details>

---

## 📁 Output Structure

```
app-folder/
│
├── AntiPublic/                               ← permanent hash DB (never cleared)
│   ├── phash_database.txt                    ← ~1 KB per image, no photos stored
│   └── AntiPublic_features.txt               ← full visual feature log
│
└── Result/
    ├── Valid_Email.txt                        ← IMAP checker valid credentials
    ├── Hotmail_DD_MM_YYYY_HH_MM_SS/
    │   ├── emails/
    │   └── attachments/
    ├── IMAP_DD_MM_YYYY_HH_MM_SS/
    │   └── account@domain/
    ├── OneDrive_DD_MM_YYYY_HH_MM_SS/
    ├── PhotoSeed&PK_DD_MM_YYYY_HH_MM_SS/
    │   ├── Seed.txt
    │   ├── Hex.txt
    │   ├── Evm.txt
    │   └── Wif.txt
    └── Anti-Public_DD_MM_YYYY_HH_MM_SS/
```

---

## 💻 System Requirements

| Requirement | Details |
|:---|:---|
| **Operating System** | Windows 10 (build 22621+) or Windows 11 |
| **Runtime** | .NET 9 — bundled inside the release binary |
| **CPU** | x64 architecture · 4+ cores recommended |
| **RAM** | 4 GB minimum · 8 GB recommended |
| **Proxy** | Optional — direct connection fully supported |
| **OCR** | Windows OCR language packs (Settings → Time & Language → Language) |

---

## 🔧 Technology Stack

<div align="center">

| Layer | Technology |
|:---:|:---|
| **Language** | C# 13 on .NET 9 |
| **UI** | WPF + ModernWpf — native Windows 11 design language |
| **IMAP** | MailKit — industry-standard mail library |
| **Image processing** | Magick.NET — ImageMagick bindings for .NET |
| **OCR** | Windows.Media.Ocr — on-device, zero latency, no API key |
| **Crypto** | NBitcoin — BIP-39 validation and key format support |
| **Serialization** | System.Text.Json |

</div>

---

## ⚖️ Legal Notice

> This software is intended exclusively for **authorized** security research, penetration testing, corporate IT audits, and lawful OSINT operations.
>
> You must hold explicit written permission before using this tool against any system you do not own or administer.
> Unauthorized use may constitute a criminal offense under applicable law. The authors and distributors accept no liability for misuse.

---

<div align="center">

**Questions? Ready to purchase?**

## [`@cyberpaladin`](https://t.me/cyberpaladin)

<br/>

<img src="https://capsule-render.vercel.app/api?type=waving&color=gradient&customColorList=12&height=120&section=footer" width="100%"/>

</div>
